using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FrontlineLogistics.TerritorialGmi
{
    internal sealed class SettingsPanelWidgets
    {
        private readonly List<KeyValuePair<TMP_Text, int>> localized = new List<KeyValuePair<TMP_Text, int>>();
        private TMP_Text fontSource;
        private TMP_InputField inputStyle;
        private Button buttonStyle;
        internal Image BackgroundStyle { get; private set; }

        internal bool FindNativeStyle()
        {
            var ui = UIManager.uiManagerInstance;
            var settings = ui.systemSettingView;
            fontSource = settings != null ? settings.GetComponentInChildren<TMP_Text>(true) : null;
            if (fontSource == null || fontSource.font == null)
                fontSource = ui.GetComponentInChildren<TMP_Text>(true);
            inputStyle = settings != null ? settings.GetComponentInChildren<TMP_InputField>(true) : null;
            if (inputStyle == null) inputStyle = ui.GetComponentInChildren<TMP_InputField>(true);
            buttonStyle = settings != null ? settings.GetComponentInChildren<Button>(true) : null;
            if (buttonStyle == null) buttonStyle = ui.GetComponentInChildren<Button>(true);
            BackgroundStyle = settings != null ? settings.GetComponent<Image>() : null;
            return fontSource != null && fontSource.font != null;
        }

        internal void RefreshText(GameObject root)
        {
            foreach (var entry in localized)
                entry.Key.text = Localization.Text(entry.Value);
            if (fontSource != null)
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font = fontSource.font;
                    text.fontSharedMaterial = fontSource.fontSharedMaterial;
                }
        }

        internal TMP_InputField BuildInput(RectTransform parent)
        {
            var fieldRect = Rect("Value", parent);
            var size = fieldRect.gameObject.AddComponent<LayoutElement>();
            size.minWidth = size.preferredWidth = 160f;
            size.minHeight = size.preferredHeight = 42f;
            var image = fieldRect.gameObject.AddComponent<Image>();
            StyleImage(image, inputStyle != null ? inputStyle.targetGraphic as Image : null, new Color(0.18f, 0.2f, 0.22f, 1f));
            var field = fieldRect.gameObject.AddComponent<TMP_InputField>();
            BuildInputText(field, fieldRect);
            field.targetGraphic = image;
            field.contentType = TMP_InputField.ContentType.Custom;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterValidation = TMP_InputField.CharacterValidation.None;
            field.richText = false;
            field.restoreOriginalTextOnEscape = false;
            if (inputStyle != null)
            {
                field.colors = inputStyle.colors;
                field.selectionColor = inputStyle.selectionColor;
                field.caretColor = inputStyle.caretColor;
                field.customCaretColor = inputStyle.customCaretColor;
            }
            return field;
        }

        private void BuildInputText(TMP_InputField field, RectTransform parent)
        {
            var textArea = Rect("TextArea", parent);
            Stretch(textArea);
            InsetText(textArea);
            textArea.gameObject.AddComponent<RectMask2D>();
            var text = Text(textArea, "Text", 22f);
            Stretch(text.rectTransform);
            text.enableWordWrapping = false;
            text.alignment = TextAlignmentOptions.MidlineRight;
            if (inputStyle != null && inputStyle.textComponent != null)
                text.color = inputStyle.textComponent.color;
            field.textViewport = textArea;
            field.textComponent = text;
        }

        internal void AddButton(RectTransform parent, int textId, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect("Action" + textId, parent);
            var size = rect.gameObject.AddComponent<LayoutElement>();
            size.minWidth = 0f;
            size.preferredWidth = 240f;
            size.flexibleWidth = 1f;
            size.minHeight = size.preferredHeight = 48f;
            var image = rect.gameObject.AddComponent<Image>();
            StyleImage(image, buttonStyle != null ? buttonStyle.targetGraphic as Image : null, new Color(0.22f, 0.25f, 0.28f, 1f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if (buttonStyle != null) button.colors = buttonStyle.colors;
            button.onClick.AddListener(action);
            var text = Label(rect, textId, 22f);
            Stretch(text.rectTransform);
            InsetText(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14f;
            text.fontSizeMax = 22f;
        }

        internal TMP_Text Label(Transform parent, int id, float size)
        {
            var text = Text(parent, "Text" + id, size);
            localized.Add(new KeyValuePair<TMP_Text, int>(text, id));
            return text;
        }

        internal TMP_Text Text(Transform parent, string name, float size)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = fontSource.font;
            text.fontSharedMaterial = fontSource.fontSharedMaterial;
            text.color = fontSource.color;
            text.fontSize = size;
            text.enableWordWrapping = true;
            text.richText = false;
            text.raycastTarget = false;
            return text;
        }

        internal static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        internal static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void InsetText(RectTransform rect)
        {
            const float horizontalInset = 10f;
            const float verticalInset = 4f;
            rect.offsetMin = new Vector2(horizontalInset, verticalInset);
            rect.offsetMax = new Vector2(-horizontalInset, -verticalInset);
        }

        internal static void StyleImage(Image image, Image source, Color fallback)
        {
            image.color = source != null ? source.color : fallback;
            if (source == null) return;
            image.sprite = source.sprite;
            image.type = source.type;
            image.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        }
    }
}
