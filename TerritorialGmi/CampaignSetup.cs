using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FrontlineLogistics.TerritorialGmi
{
    internal sealed class CampaignSetup : MonoBehaviour
    {
        private const float RowWidth = 473f;
        private const float RowHeight = 26f;
        private const float ConfigureWidth = 140f;
        private const float ControlSpacing = 12f;
        private const float CheckboxSize = 20f;
        private const float TitleInset = 26f;
        private const float ButtonTextInset = 6f;
        private const float MinimumFontSize = 8f;

        private static CampaignSetup current;
        private OpeningUIManager owner;
        private RectTransform row;
        private Toggle enabledToggle;
        private Button configureButton;
        private TMP_Text titleLabel;
        private TMP_Text configureLabel;
        private TMP_Text toggleTextStyle;
        private TMP_Text buttonTextStyle;
        private int language = -1;

        internal static bool IsVisible => current != null && current.owner != null
            && current.owner.openingSettingUI != null
            && current.owner.openingSettingUI.gameObject.activeInHierarchy;

        private void Initialize(OpeningUIManager opening, Toggle tutorial)
        {
            owner = opening;
            current = this;
            var pane = opening.openingSettingUI;
            var buttonStyle = pane.Find("NewGame").GetComponent<Button>();
            CaptureTextStyles(tutorial, buttonStyle);
            BuildRow(pane);
            BuildToggle(tutorial);
            BuildConfigureButton(buttonStyle);
            WireEvents();
            RefreshText();
            row.gameObject.SetActive(true);
            LayoutRebuilder.MarkLayoutForRebuild(pane);
        }

        private void CaptureTextStyles(Toggle tutorial, Button buttonStyle)
        {
            toggleTextStyle = tutorial.GetComponentInChildren<TMP_Text>(true);
            foreach (var text in buttonStyle.GetComponentsInChildren<TMP_Text>(true))
                if (text.gameObject.activeSelf) buttonTextStyle = text;
        }

        private void BuildRow(RectTransform pane)
        {
            row = Rect("TerritorialGmiCampaignSetup", pane);
            row.gameObject.SetActive(false);
            var size = row.gameObject.AddComponent<LayoutElement>();
            size.minWidth = size.preferredWidth = RowWidth;
            size.minHeight = size.preferredHeight = RowHeight;
            row.sizeDelta = new Vector2(RowWidth, RowHeight);

            // The native footer has fixed anchors. Reserve this row's height in the existing scroll area.
            var description = pane.GetComponentInChildren<ScrollRect>(true);
            var descriptionRect = (RectTransform)description.transform;
            float descriptionHeight = descriptionRect.rect.height - RowHeight
                - pane.GetComponent<VerticalLayoutGroup>().spacing;
            var descriptionSize = description.GetComponent<LayoutElement>();
            descriptionSize.minHeight = descriptionSize.preferredHeight = descriptionHeight;
            descriptionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionHeight);
        }

        private void BuildToggle(Toggle tutorial)
        {
            var toggleRect = Rect("Enabled", row);
            Stretch(toggleRect);
            toggleRect.offsetMax = new Vector2(-(ConfigureWidth + ControlSpacing), 0f);
            toggleRect.gameObject.AddComponent<Image>().color = Color.clear;
            enabledToggle = toggleRect.gameObject.AddComponent<Toggle>();
            enabledToggle.onValueChanged = new Toggle.ToggleEvent();
            enabledToggle.group = null;
            enabledToggle.colors = tutorial.colors;
            enabledToggle.spriteState = tutorial.spriteState;
            enabledToggle.transition = tutorial.transition;
            enabledToggle.toggleTransition = tutorial.toggleTransition;

            var background = Rect("Background", toggleRect);
            background.anchorMin = background.anchorMax = new Vector2(0f, 0.5f);
            background.pivot = new Vector2(0f, 0.5f);
            background.sizeDelta = new Vector2(CheckboxSize, CheckboxSize);
            var backgroundImage = background.gameObject.AddComponent<Image>();
            StyleImage(backgroundImage, (Image)tutorial.targetGraphic);
            enabledToggle.targetGraphic = backgroundImage;
            var checkmark = Rect("Checkmark", background);
            checkmark.anchorMin = checkmark.anchorMax = new Vector2(0.5f, 0.5f);
            checkmark.sizeDelta = tutorial.graphic.rectTransform.rect.size;
            var checkmarkImage = checkmark.gameObject.AddComponent<Image>();
            StyleImage(checkmarkImage, (Image)tutorial.graphic);
            checkmarkImage.raycastTarget = false;
            enabledToggle.graphic = checkmarkImage;
            titleLabel = Text("Title", toggleRect, toggleTextStyle);
            titleLabel.rectTransform.offsetMin = new Vector2(TitleInset, 0f);
            titleLabel.alignment = TextAlignmentOptions.MidlineLeft;
        }

        private void BuildConfigureButton(Button buttonStyle)
        {
            var buttonRect = Rect("Configure", row);
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = Vector2.one;
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.sizeDelta = new Vector2(ConfigureWidth, 0f);
            var buttonImage = buttonRect.gameObject.AddComponent<Image>();
            StyleImage(buttonImage, (Image)buttonStyle.targetGraphic);
            configureButton = buttonRect.gameObject.AddComponent<Button>();
            configureButton.onClick = new Button.ButtonClickedEvent();
            configureButton.targetGraphic = buttonImage;
            configureButton.colors = buttonStyle.colors;
            configureButton.spriteState = buttonStyle.spriteState;
            configureButton.transition = buttonStyle.transition;
            configureLabel = Text("Text", buttonRect, buttonTextStyle);
            configureLabel.rectTransform.offsetMin = new Vector2(ButtonTextInset, 0f);
            configureLabel.rectTransform.offsetMax = new Vector2(-ButtonTextInset, 0f);
            configureLabel.alignment = TextAlignmentOptions.Center;
        }

        private void WireEvents()
        {
            enabledToggle.SetIsOnWithoutNotify(CampaignSettings.DraftEnabled);
            configureButton.interactable = CampaignSettings.DraftEnabled;
            enabledToggle.onValueChanged.AddListener(SetEnabled);
            configureButton.onClick.AddListener(OpenSettings);
        }

        private void SetEnabled(bool value)
        {
            CampaignSettings.DraftEnabled = value;
            configureButton.interactable = value;
        }

        private void OpenSettings()
        {
            if (IsVisible && CampaignSettings.DraftEnabled)
                Plugin.Instance.GetComponent<SettingsPanel>().Open();
        }

        private void Update()
        {
            if (language != GameSettingsManager.LanguageID) RefreshText();
        }

        private void RefreshText()
        {
            language = GameSettingsManager.LanguageID;
            titleLabel.font = toggleTextStyle.font;
            titleLabel.fontSharedMaterial = toggleTextStyle.fontSharedMaterial;
            configureLabel.font = buttonTextStyle.font;
            configureLabel.fontSharedMaterial = buttonTextStyle.fontSharedMaterial;
            titleLabel.text = Localization.Text(Localization.Title);
            configureLabel.text = Localization.Text(Localization.Configure);
        }

        private void OnDestroy()
        {
            if (current == this) current = null;
            if (row != null) Destroy(row.gameObject);
        }

        private static TMP_Text Text(string name, Transform parent, TMP_Text source)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            Stretch(text.rectTransform);
            text.font = source.font;
            text.fontSharedMaterial = source.fontSharedMaterial;
            text.color = source.color;
            text.fontStyle = source.fontStyle;
            text.fontSize = source.fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = MinimumFontSize;
            text.fontSizeMax = source.fontSize;
            text.enableWordWrapping = false;
            text.richText = false;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void StyleImage(Image image, Image source)
        {
            image.color = source.color;
            image.sprite = source.sprite;
            image.type = source.type;
            image.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        }

        [HarmonyPatch(typeof(OpeningUIManager), "Start")]
        private static class OpeningStarted
        {
            private static void Postfix(OpeningUIManager __instance, Toggle ___tutorialToggle)
            {
                if (__instance.GetComponent<CampaignSetup>() != null) return;
                CampaignSettings.ResetDraft();
                __instance.gameObject.AddComponent<CampaignSetup>().Initialize(__instance, ___tutorialToggle);
            }
        }

        [HarmonyPatch(typeof(OpeningUIManager), nameof(OpeningUIManager.OnStartGameButtonClicked))]
        private static class CampaignStarted
        {
            private static void Prefix() => CampaignSettings.CaptureNewCampaign();
        }
    }
}
