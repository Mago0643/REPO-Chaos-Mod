using MenuLib;
using MenuLib.MonoBehaviors;
using MenuLib.Structs;
using System.Collections;
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

    static REPOPopupPage? configPage;
    static string[] categories = ["General", "Events", "Language"];
    static bool hasChanges = false;
    static bool isInConfigMenu = false;
    static string lastSelectedCategory = "";
    static REPOPopupPage menuPage;
    internal static void CreateConfigMenu()
    {
        if (configPage != null) {
            Object.Destroy(configPage.gameObject);
            configPage = null!;
        }
        
        configPage = MenuAPI.CreateREPOPopupPage("Categories", REPOPopupPage.PresetSide.Left, true, true, 0);
        configPage.scrollView.scrollSpeed = 3f;
        configPage.maskPadding = configPage.maskPadding with { top = 35 };
        

        // create shits
        // configPage.maskPadding = new Padding(0, 0, 0, 0);
        // var repoButton = MenuAPI.CreateREPOButton("A button", () => Logging.Log("I was clicked!"), configPage.scrollView.transform, Vector2.zero);

        foreach (string name in categories) {
            string shit = name;
            configPage.AddElementToScrollView(scrollView => {
                var btn = MenuAPI.CreateREPOButton(shit, () => CreateSettingCategoryMenu(shit), scrollView);
                btn.labelTMP.fontStyle = TMPro.FontStyles.Normal;
                return btn.rectTransform;
            });
        }

        configPage.AddElement(parent => {
            var exitButton = MenuAPI.CreateREPOButton("Back", () => configPage.ClosePage(true), parent, new Vector2(66f, 18f));
        });

        configPage.OpenPage(false);
    }

    static void CreateSettingCategoryMenu(string category)
    {
        if (lastSelectedCategory == category) return;
        if (isInConfigMenu) {
            if (hasChanges) {
                bool isYes = false;
                MenuAPI.OpenPopup("Unsaved changes", Color.red, "You have unsaved changes, are you sure that you want to change category?", () => {
                    isYes = true;
                    hasChanges = false;
                }, () => isYes = false);
                if (!isYes) return;
            } else if (menuPage != null) menuPage.ClosePage(true);
        }

        isInConfigMenu = true;

        var page = MenuAPI.CreateREPOPopupPage(category, REPOPopupPage.PresetSide.Right, false, false, 5f);
        page.scrollView.scrollSpeed = 3f;

        menuPage = page;

        switch (category) {
            case string s when s == categories[0]: { // General
                
                break;
            }
            case string s when s == categories[1]: { // Events

                break;
            }
            case string s when s == categories[2]: { // Languages

                break;
            }
            default: {
                Logging.Error($"Failed to make config menu: Did not found category with name: \"{category}\"");
                break;
            }
        }

        page.AddElement(parent => {
            var saveButton = MenuAPI.CreateREPOButton("Save and Close", () => {
                hasChanges = false;
                page.ClosePage(true);
            }, parent, new Vector2(page.rectTransform.localPosition.x + 200f, 18f));

            var exitButton = MenuAPI.CreateREPOButton("Discard and Close", () => {
                if (hasChanges) MenuAPI.OpenPopup("Discard and Close", Color.red, "Are you sure that you want to discard changes?", () => {
                    hasChanges = false;
                    page.ClosePage(true);
                });
                else page.ClosePage(true);
            }, parent, new Vector2(page.rectTransform.localPosition.x + 240f, 18f));
        });
        page.OpenPage(true);
        lastSelectedCategory = category;
    }
}