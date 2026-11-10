using MenuLib;
using MenuLib.MonoBehaviors;
using MenuLib.Structs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lego.ChaosMod.Backend;
public sealed class ChaosConfigManager
{
    // do not change this
    const int CANVAS_WIDTH = 640;
    const int CANVAS_HEIGHT = 360;

    private static REPOButton? titleButton;
    private static bool registered = false;
    private static string PAGE_NAME = "";
    internal static void Register() {
        if (registered) {
            TitleMenuAnimation();
            return;
        }
        registered = true;

        PAGE_NAME = Language.GetText("button_config");
        MenuAPI.BuilderDelegate callback = (Transform parent) => {
            MenuAPI.CreateREPOButton(PAGE_NAME, CreateConfigMenu, parent, new Vector2(CANVAS_WIDTH / 2f, 0));
        };

        MenuAPI.BuilderDelegate titleCallback = (Transform parent) => {
            titleButton = MenuAPI.CreateREPOButton(PAGE_NAME, CreateConfigMenu, parent, new Vector2(CANVAS_WIDTH / 2f, -200f));
            TitleMenuAnimation();
        };

        MenuAPI.AddElementToMainMenu(titleCallback);
        MenuAPI.AddElementToLobbyMenu(callback);
        MenuAPI.AddElementToEscapeMenu(callback);
    }

    internal static void TitleMenuAnimation()
    {
        if (!SemiFunc.IsMainMenu() || titleButton == null) {
            titleButton = null!;
            return;
        }

        if (MenuPageMain.instance == null || !MenuPageMain.instance.doIntroAnimation) return;

        ChaosMod.Instance.StartCoroutine(ButtonAnimation());
    }

    static IEnumerator ButtonAnimation()
    {
        yield return new WaitUntil(() => MenuPageMain.instance.waitTimer > 3f);

        if (titleButton == null) yield break;

        RectTransform rect = titleButton.rectTransform;
        float targetY = 0f;
        while (rect != null && rect.localPosition.y < -0.01f && SemiFunc.IsMainMenu()) {
            rect.localPosition = new Vector3(
                CANVAS_WIDTH / 2f,
                Mathf.Lerp(rect.localPosition.y, targetY, Time.deltaTime * 2f),
                0f
            );
            
            yield return null;
        }

        if (rect == null || !SemiFunc.IsMainMenu()) yield break;

        rect.localPosition = new Vector3(CANVAS_WIDTH / 2f, targetY, 0f);
    }

    class CategoryItem
    {
        public string keyName = "missing_translation";
        public string defaultValue = "Missing Translation";
        public REPOButton element;
        public CategoryItem() {}
    }

    static REPOPopupPage? categoryPage;
    static string[] categories = ["options_general", "options_events"];
    static string[] categories_default = ["General", "Events"];
    static bool hasChanges = false;
    static bool isInConfigMenu = false;
    static string lastSelectedCategory = "";
    static REPOPopupPage? menuPage;
    static Queue<CategoryItem> categoryItems = new();
    internal static void CreateConfigMenu()
    {
        if (categoryPage != null) {
            Object.Destroy(categoryPage.gameObject);
            categoryPage = null!;
        }
        
        categoryPage = MenuAPI.CreateREPOPopupPage(Language.GetText("options_categories", "Categories"), REPOPopupPage.PresetSide.Left, true, true, 0);
        categoryPage.scrollView.scrollSpeed = 3f;
        categoryPage.maskPadding = categoryPage.maskPadding with { top = 35 };
        
        // create shits
        // configPage.maskPadding = new Padding(0, 0, 0, 0);
        // var repoButton = MenuAPI.CreateREPOButton("A button", () => Logging.Log("I was clicked!"), configPage.scrollView.transform, Vector2.zero);

        categoryItems.Clear();

        for (int i = 0; i < categories.Length; i++) {
            string name = categories[i];
            string default_str = categories_default[i];
            string translated = Language.GetText(name, default_str);
            categoryPage.AddElementToScrollView(scrollView => {
                var btn = MenuAPI.CreateREPOButton(translated, () => CreateSettingCategoryMenu(translated), scrollView);
                btn.labelTMP.fontStyle = TMPro.FontStyles.Normal;
                categoryItems.Enqueue(new() { defaultValue = default_str, element = btn, keyName = name });
                return btn.rectTransform;
            });
        }

        categoryPage.AddElement(parent => {
            var exitButton = MenuAPI.CreateREPOButton(Language.GetText("options_back", "Back"), () => {
                if (hasChanges) {
                    MenuAPI.OpenPopup(Language.GetText("options_save_and_close", "Discard and Close"), Color.red, Language.GetText("options_alert_discard_changes", "Are you sure that you want to discard changes?"), () => {
                        hasChanges = false;
                        categoryItems.Clear();
                        categoryPage.ClosePage(true);
                        lastSelectedCategory = "";
                    });
                    return;
                }

                categoryItems.Clear();
                categoryPage.ClosePage(true);
                lastSelectedCategory = "";
            }, parent, new Vector2(66f, 18f));
            categoryItems.Enqueue(new() { defaultValue = "Back", element = exitButton, keyName = "options_back" });
        });

        categoryPage.OpenPage(false);
    }

    static void CreateSettingCategoryMenu(string category)
    {
        var save = ChaosMod.GetPreferences();
        if (lastSelectedCategory == category) return;
        if (isInConfigMenu) {
            if (hasChanges) {
                MenuAPI.OpenPopup(Language.GetText("options_alert_unsaved", "Unsaved changes"), Color.red, Language.GetText("options_alert_unsaved_category", "You have unsaved changes, are you sure that you want to change category?"), () => {
                    hasChanges = false;
                    menuPage!.ClosePage(true);
                });
                return;
            } else if (menuPage != null) menuPage.ClosePage(true);
        }

        isInConfigMenu = true;

        var page = MenuAPI.CreateREPOPopupPage(category, REPOPopupPage.PresetSide.Right, false, false, 5f);
        page.scrollView.scrollSpeed = 3f;

        menuPage = page;

        switch (category) {
            case string s when s == Language.GetText(categories[0], categories_default[0]): { // General
                CreateCategoryGeneral();
                break;
            }
            case string s when s == Language.GetText(categories[1], categories_default[1]): { // Events
                CreateCategoryEvents();
                break;
            }
            default: {
                Logging.Error($"Failed to make config menu: Did not found category with name: \"{category}\"");
                break;
            }
        }

        page.AddElement(parent => {
            var saveButton = MenuAPI.CreateREPOButton(Language.GetText("options_save_and_close", "Save and close"), () => {
                hasChanges = false;
                page.ClosePage(true);
                lastSelectedCategory = "";
                ChaosMod.Instance.SaveSettings();
            }, parent, new Vector2(page!.rectTransform.localPosition.x + 300f, 20f));

            string discard = Language.GetText("options_save_and_close", "Discard and Close");
            var exitButton = MenuAPI.CreateREPOButton(discard, () => {
                if (hasChanges) {
                    MenuAPI.OpenPopup(discard, Color.red, Language.GetText("options_alert_discard_changes", "Are you sure that you want to discard changes?"), () => {
                        hasChanges = false;
                        page.ClosePage(true);
                        lastSelectedCategory = "";

                    });
                } else {
                    page.ClosePage(true);
                    lastSelectedCategory = "";
                }
            }, parent, new Vector2(page!.rectTransform.localPosition.x + 500f, 20f));

            categoryItems.Enqueue(new() { defaultValue = "Save and Close", element = saveButton, keyName = "options_save_and_close" });
            categoryItems.Enqueue(new() { defaultValue = "Discard and Close", element = exitButton, keyName = "options_discard_and_close" });
        });
        page.OpenPage(true);
        lastSelectedCategory = category;
    }

    public static string[] Languages { get; } = [
        "한국어",
        "English"
    ];
    internal static Dictionary<string, string> LanguageToKey { get; } = new() {
        {"한국어", "ko"},
        {"English", "en-us"},
    };

    static void CreateCategoryGeneral()
    {
        var page = menuPage!;
        var save = ChaosMod.GetPreferences();

        // language
        page.AddElementToScrollView(parent => {
            var slider = MenuAPI.CreateREPOSlider(
                Language.GetText("options_language", "Language"),
                Language.GetText("options_language_desc", "The language to use in this mod."),
                (string s) => {},
                parent,
                Languages,
                "English"
            );
            slider.SetValue(slider.stringOptions.IndexOf(save.Get("Language", "English")), false);
            slider.onValueChanged = f => {
                string item = slider.stringOptions.ElementAtOrDefault(System.Convert.ToInt32(f)) ?? slider.stringOptions.FirstOrDefault();

                if (LanguageToKey.TryGetValue(item, out string key)) {
                    Language.Load(key);
                } else {
                    Logging.Error("Did not found registered language name: " + item);
                    return;
                }

                save.Set("Language", item);
                ChaosMod.Instance.SaveSettings();

                page.ClosePage(true); // just make the menu again
                hasChanges = false;
                page.OpenPage(true);

                UpdateMenuNames();
            };
            return slider.rectTransform;
        });

        // max timer
        page.AddElementToScrollView(parent => {
            var slider = MenuAPI.CreateREPOSlider(
                Language.GetText("options_timer", "Timer Duration"),
                Language.GetText("options_timer_desc", "A value that determines whether an event should run every few seconds. Default is 20."),
                i => {
                    save.Set("Timer-len", i);
                    hasChanges = true;
                    ChaosMod.MaxEventTimer = i;
                }, parent, min: 5, max: 120, defaultValue: 20
            );
            slider.SetValue(save.Get("Timer-len", 20), false);
            return slider.rectTransform;
        });

        // disable
        page.AddElementToScrollView(parent => {
            var toggle = MenuAPI.CreateREPOToggle(
                Language.GetText("options_disable_timer", "Disable Timer"),
                b => {
                    save.Set("Disable-Timer", b);
                    hasChanges = true;
                    ChaosMod.DISABLE_TIMER = b;
                }, parent, leftButtonText: "ON", rightButtonText: "OFF", defaultValue: false
            );
            toggle.SetState(save.Get("Disable-Timer", false), false);
            return toggle.rectTransform;
        });
    }

    static void CreateCategoryEvents()
    {
        
    }

    static void UpdateMenuNames()
    {
        foreach (var item in categoryItems) {
            item.element.labelTMP.text = Language.GetText(item.keyName, item.defaultValue);
        }
        
        if (categoryPage != null) categoryPage.headerTMP.text = Language.GetText("options_categories", "Categories");
        if (menuPage != null) menuPage.headerTMP.text = Language.GetText(lastSelectedCategory);
    }
}