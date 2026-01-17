using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChaosMod
{
    // this is client-side script
    internal class OptionsMenuController : MonoBehaviour
    {
        public enum OptionType : int
        {
            Checkbox = 0,
            Range = 1,
            Value = 2,
            Color = 3,
        }

        public class OptionItem
        {
            public string name;
            public OptionType type;
            public float minValue;
            public float maxValue;
            public string rangeType;
            public OptionItem(string name, OptionType type, float minValue = 0f, float maxValue = 1f, string rangeType = "%")
            {
                this.name = name;
                this.type = type;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.rangeType = rangeType;
            }
        }

        private Image Cover;
        private Image menu_bg;
        private GameObject menu;

        private GameObject item_template;
        private GameObject boolean_template;
        private GameObject range_template;
        private GameObject float_template;
        private GameObject color_template;
        private Dictionary<string, GameObject[]> option_data = new Dictionary<string, GameObject[]>();

        internal VerticalLayoutGroup layout;
        internal UnityEvent<OptionType, string, object> onValueChanged = new UnityEvent<OptionType, string, object>();
        internal bool isShown = false;
        internal Dictionary<string, object> datas = new Dictionary<string, object>();
        void Start()
        {
            // Cover
            var cover = (RectTransform)transform.GetChild(0);
            // Util.RectTransformFullscreen(cover);
            cover.sizeDelta = new Vector2(Screen.width, Screen.height);

            Cover = cover.GetComponent<Image>();
            Cover.CrossFadeAlpha(0f, 0f, true);

            // BG
            menu_bg = transform.GetChild(1).GetComponent<Image>();

            // actual menu
            menu = transform.GetChild(2).gameObject;

            // 템플릿 설정
            item_template = menu.transform.GetChild(0) // Viewport
                            .GetChild(0)               // Content
                            .GetChild(4).gameObject;   // Item
            boolean_template = item_template.transform.parent.GetChild(0).gameObject;
            range_template = item_template.transform.parent.GetChild(1).gameObject;
            float_template = item_template.transform.parent.GetChild(2).gameObject;
            color_template = item_template.transform.parent.GetChild(3).gameObject;
            layout = item_template.transform.parent.GetComponent<VerticalLayoutGroup>();

            item_template.SetActive(false);
            boolean_template.SetActive(false);
            range_template.SetActive(false);
            float_template.SetActive(false);
            color_template.SetActive(false);

            //AddCategory("General");
            //var option = new List<OptionItem> { new OptionItem("Enabled", OptionType.Checkbox), new OptionItem("Timer Range", OptionType.Range), new OptionItem("Chance", OptionType.Value), new OptionItem("Color", OptionType.Color) };
            //for (int i = 0; i < 50; i++)
            //{
            //    AddOption("Option " + i, option);
            //}
            Hide();
        }

        public void AddOption(string optionName, List<OptionItem> options)
        {
            var item = Instantiate(item_template, item_template.transform.parent);
            item.name = "Option - " + optionName;
            item.SetActive(true);

            var setting = item.AddComponent<OptionSetting>();
            setting.categoryTxt = item.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            setting.categoryTxt.text = optionName;
            setting.objectCount = options.Count;
            setting.child = item.transform.GetChild(0);
            setting.Arrow = (RectTransform)item.transform.GetChild(3);
            setting.controller = this;

            Transform child = setting.child.GetChild(1);
            foreach (OptionItem option in options)
            {
                switch (option.type)
                {
                    case OptionType.Checkbox:
                        {
                            var check = Instantiate(boolean_template, child);
                            check.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.name;
                            check.SetActive(true);
                            check.transform.GetChild(1).GetComponent<Toggle>()
                                .onValueChanged.AddListener((bool enabled) => {
                                    if (_notify)
                                        onValueChanged.Invoke(OptionType.Checkbox, $"{optionName}:{option.name}", enabled);
                                });
                            // this will be have an name like "Option Name:Property Name"
                            option_data.Add($"{optionName}:{option.name}", new GameObject[] { check.transform.GetChild(1).gameObject });
                            datas.Add($"{optionName}:{option.name}", false);
                            break;
                        }
                    case OptionType.Range:
                        {
                            var range = Instantiate(range_template, child);
                            range.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.name;
                            range.SetActive(true);

                            var minInput = range.transform.GetChild(1).GetComponent<TMP_InputField>();
                            var maxInput = range.transform.GetChild(2).GetComponent<TMP_InputField>();
                            minInput.onEndEdit.AddListener((string txt) => {
                                float value = float.Parse(txt);
                                float fMax = float.Parse(maxInput.text);
                                // no getting bigger than maxInput
                                value = Mathf.Min(value, fMax);
                                value = Mathf.Clamp(value, option.minValue, option.maxValue);
                                minInput.SetTextWithoutNotify(value.ToString());
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Range, $"{optionName}:{option.name}", new float[] { value, fMax });
                            });
                            maxInput.onEndEdit.AddListener((string txt) => {
                                float value = float.Parse(txt);
                                float fMin = float.Parse(minInput.text);
                                // no getting bigger than minInput
                                value = Mathf.Max(value, fMin);
                                value = Mathf.Clamp(value, option.minValue, option.maxValue);
                                maxInput.SetTextWithoutNotify(value.ToString());
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Range, $"{optionName}:{option.name}", new float[] { fMin, value });
                            });
                            option_data.Add($"{optionName}:{option.name}", new GameObject[] { range.transform.GetChild(1).gameObject, range.transform.GetChild(2).gameObject });
                            datas.Add($"{optionName}:{option.name}", new float[] { 0f, 1f });
                            break;
                        }
                    case OptionType.Value:
                        {
                            var value = Instantiate(float_template, child);
                            value.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.name;
                            value.SetActive(true);

                            var input = value.transform.GetChild(1).GetComponent<TMP_InputField>();
                            input.onEndEdit.AddListener((string txt) => {
                                float value = float.Parse(txt);
                                value = Mathf.Clamp(value, option.minValue, option.maxValue);
                                input.SetTextWithoutNotify(value.ToString());
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Value, $"{optionName}:{option.name}", value);
                            });
                            option_data.Add($"{optionName}:{option.name}", new GameObject[] { value.transform.GetChild(1).gameObject });
                            datas.Add($"{optionName}:{option.name}", 0f);
                            break;
                        }
                    case OptionType.Color:
                        {
                            // im too lazy to do another spagetti code
                            setting.objectCount++;

                            var color = Instantiate(color_template, child);
                            color.SetActive(true);
                            var preview = color.transform.GetChild(5).GetComponent<Image>();
                            color.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener((float value) => {
                                Color cCol = preview.color;
                                cCol.r = value;
                                preview.color = cCol;
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Color, $"{optionName}:{option.name}", preview.color);
                            });
                            color.transform.GetChild(2).GetComponent<Slider>().onValueChanged.AddListener((float value) => {
                                Color cCol = preview.color;
                                cCol.g = value;
                                preview.color = cCol;
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Color, $"{optionName}:{option.name}", preview.color);
                            });
                            color.transform.GetChild(3).GetComponent<Slider>().onValueChanged.AddListener((float value) => {
                                Color cCol = preview.color;
                                cCol.b = value;
                                preview.color = cCol;
                                if (_notify)
                                    onValueChanged.Invoke(OptionType.Color, $"{optionName}:{option.name}", preview.color);
                            });
                            color.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.name;
                            option_data.Add($"{optionName}:{option.name}", new GameObject[] { // r g b color_preview
                                color.transform.GetChild(1).gameObject, color.transform.GetChild(2).gameObject,
                                color.transform.GetChild(3).gameObject, color.transform.GetChild(4).gameObject
                            });
                            datas.Add($"{optionName}:{option.name}", Color.black);
                            break;
                        }
                }
            }

            layout.enabled = false;
            layout.enabled = true;
        }

        public bool GetBool(string name)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return false;
            }

            if (!objects[0].TryGetComponent<Toggle>(out var toggle))
            {
                ChaosMod.Logger.LogError($"{name} has no Toggle Component.");
                return false;
            }

            return toggle.isOn;
        }

        public float[] GetRange(string name)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return null;
            }

            if (objects.Length != 2)
            {
                ChaosMod.Logger.LogError($"{name} is not range type.");
                return null;
            }

            var minInput = objects[0].GetComponent<TMP_InputField>();
            var maxInput = objects[1].GetComponent<TMP_InputField>();

            if (!float.TryParse(minInput.text, out float fMin))
            {
                ChaosMod.Logger.LogError($"Failed to parse min float. (got {minInput.text})");
                return null;
            }
            if (!float.TryParse(maxInput.text, out float fMax))
            {
                ChaosMod.Logger.LogError($"Failed to parse max float. (got {maxInput.text})");
                return null;
            }

            return new float[] { fMin, fMax };
        }

        public float GetValue(string name)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return float.NaN;
            }

            if (!objects[0].TryGetComponent<TMP_InputField>(out var input))
            {
                ChaosMod.Logger.LogError($"{name} has no Input Field (TMP)");
                return float.NaN;
            }

            if (!float.TryParse(input.text, out float fValue))
            {
                ChaosMod.Logger.LogError("Failed to parse string to float.");
                return float.NaN;
            }

            return fValue;
        }

        public Color GetColor(string name)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return Color.magenta;
            }

            if (objects.Length != 4)
            {
                ChaosMod.Logger.LogError($"{name} is not Color option.");
                return Color.magenta;
            }

            return new Color(
                objects[0].GetComponent<Slider>().value,
                objects[1].GetComponent<Slider>().value,
                objects[2].GetComponent<Slider>().value
            );
        }

        private bool _notify = false;
        public bool SetValue(string name, float[] range, bool notify = true)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return false;
            }

            _notify = notify;

            if (objects.Length == 2)
            {
                var minInput = objects[0].GetComponent<TMP_InputField>();
                var maxInput = objects[1].GetComponent<TMP_InputField>();

                minInput.text = range[0].ToString();
                maxInput.text = range[1].ToString();
                minInput.onEndEdit.Invoke(minInput.text);
                maxInput.onEndEdit.Invoke(maxInput.text);
                _notify = true;
                datas[name] = range;
                return true;
            }

            _notify = true;
            return false;
        }

        public bool SetValue(string name, float value, bool notify = true)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return false;
            }

            _notify = notify;
            var input = objects[0].GetComponent<TMP_InputField>();
            input.text = value.ToString();
            input.onEndEdit.Invoke(input.text);
            _notify = true;
            datas[name] = value;
            return true;
        }

        public bool SetValue(string name, bool enabled, bool notify = true)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return false;
            }

            _notify = notify;
            objects[0].GetComponent<Toggle>().isOn = enabled;
            _notify = true;
            datas[name] = enabled;
            return true;
        }

        public bool SetValue(string name, Color color, bool notify = true)
        {
            if (!option_data.TryGetValue(name, out var objects))
            {
                ChaosMod.Logger.LogError($"Did not found option name with \"{name}\"");
                return false;
            }

            _notify = notify;
            objects[0].GetComponent<Slider>().value = color.r;
            objects[1].GetComponent<Slider>().value = color.g;
            objects[2].GetComponent<Slider>().value = color.b;
            _notify = true;
            datas[name] = color;
            return true;
        }

        // just text thing because it look sucks when you just throw all shit in that
        public void AddCategory(string text)
        {
            var gObject = new GameObject("Category Text - " + text, typeof(RectTransform), typeof(TextMeshProUGUI));
            gObject.transform.SetParent(item_template.transform.parent, false);

            var tmpTxt = gObject.GetComponent<TextMeshProUGUI>();
            tmpTxt.text = text;
            tmpTxt.fontSize = 25;
            tmpTxt.alignment = TextAlignmentOptions.MidlineLeft;
            tmpTxt.rectTransform.sizeDelta = new Vector2(760f, 25f);
            tmpTxt.font = ChaosMod.Instance.pretendard;
        }

        public void Hide()
        {
            Cover.CrossFadeAlpha(0f, 1f, true);
            menu_bg.gameObject.SetActive(false);
            menu.SetActive(false);
            isShown = false;
        }

        public void Show()
        {
            Cover.CrossFadeAlpha(1f, 1f, true);
            menu_bg.gameObject.SetActive(true);
            menu.SetActive(true);
            isShown = true;
        }
    }
}
