using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static FrontlineLogistics.TerritorialGmi.SettingsPanelWidgets;

namespace FrontlineLogistics.TerritorialGmi
{
    internal sealed class SettingsPanelView
    {
        private static readonly int[] RateLabels =
        {
            Localization.MinimumLabel, Localization.TileLabel, Localization.RoadLabel,
            Localization.RailLabel, Localization.BridgeLabel, Localization.BuildingLabel
        };
        private readonly SettingsPanelWidgets widgets = new SettingsPanelWidgets();
        private ScrollRect scroll;
        private TMP_Text status;
        private int statusId;

        internal GameObject Root { get; private set; }
        internal SettingsPanelDraft Draft { get; }
        internal int Language { get; private set; } = -1;

        internal SettingsPanelView()
        {
            Draft = new SettingsPanelDraft(SetStatus);
        }

        internal bool FindNativeStyle() => widgets.FindNativeStyle();

        internal void Build(Transform parent, UnityAction apply, UnityAction close)
        {
            var canvas = BuildCanvas(parent);
            var window = BuildWindow(canvas);
            widgets.Label(window, Localization.SettingsTitle, 28f);
            var viewport = BuildViewport(window);
            var content = BuildRateContent(viewport);
            for (int i = 0; i < Rates.FieldCount; i++) BuildField(content, i);
            BuildScrollbar(viewport);
            status = widgets.Text(window, "Status", 18f);
            status.gameObject.AddComponent<LayoutElement>().minHeight = 46f;
            BuildActions(window, apply, close);
        }

        internal void Show()
        {
            Root.SetActive(true);
            scroll.verticalNormalizedPosition = 1f;
        }

        internal void Hide()
        {
            if (Root != null) Root.SetActive(false);
        }

        internal void SetStatus(int id)
        {
            statusId = id;
            if (status != null) status.text = id == 0 ? string.Empty : Localization.Text(id);
        }

        internal void RefreshText()
        {
            Language = GameSettingsManager.LanguageID;
            widgets.RefreshText(Root);
            SetStatus(statusId);
        }

        private RectTransform BuildCanvas(Transform parent)
        {
            var rect = Rect("TerritorialGmiSettings", parent);
            Root = rect.gameObject;
            Root.SetActive(false);
            var canvas = Root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var scaler = Root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 800f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            Root.AddComponent<GraphicRaycaster>();
            return rect;
        }

        private RectTransform BuildWindow(RectTransform canvas)
        {
            var blocker = Rect("ModalBlocker", canvas);
            Stretch(blocker);
            blocker.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            var window = Rect("Window", blocker);
            window.anchorMin = new Vector2(0.04f, 0.04f);
            window.anchorMax = new Vector2(0.96f, 0.96f);
            window.offsetMin = window.offsetMax = Vector2.zero;
            StyleImage(window.gameObject.AddComponent<Image>(), widgets.BackgroundStyle, new Color(0.1f, 0.12f, 0.14f, 1f));
            var layout = window.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 10f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return window;
        }

        private RectTransform BuildViewport(RectTransform window)
        {
            var viewport = Rect("ScrollViewport", window);
            var size = viewport.gameObject.AddComponent<LayoutElement>();
            size.minHeight = 60f;
            size.flexibleHeight = 1f;
            viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;
            return viewport;
        }

        private RectTransform BuildRateContent(RectTransform viewport)
        {
            var content = Rect("Rates", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var rows = content.gameObject.AddComponent<VerticalLayoutGroup>();
            rows.padding = new RectOffset(12, 32, 6, 6);
            rows.spacing = 12f;
            rows.childControlWidth = rows.childControlHeight = true;
            rows.childForceExpandWidth = true;
            rows.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            return content;
        }

        private void BuildScrollbar(RectTransform viewport)
        {
            var bar = Rect("Scrollbar", viewport);
            bar.anchorMin = new Vector2(1f, 0f);
            bar.anchorMax = Vector2.one;
            bar.pivot = Vector2.one;
            bar.offsetMin = new Vector2(-18f, 4f);
            bar.offsetMax = new Vector2(-4f, -4f);
            bar.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.27f, 0.29f, 1f);
            var handle = Rect("Handle", bar);
            Stretch(handle);
            var image = handle.gameObject.AddComponent<Image>();
            image.color = new Color(0.65f, 0.68f, 0.7f, 1f);
            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = image;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
        }

        private void BuildField(RectTransform parent, int index)
        {
            var row = Rect("Rate" + index, parent);
            var column = row.gameObject.AddComponent<VerticalLayoutGroup>();
            column.spacing = 5f;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            row.gameObject.AddComponent<LayoutElement>().minHeight = 48f;
            var heading = Rect("Heading", row);
            var line = heading.gameObject.AddComponent<HorizontalLayoutGroup>();
            line.spacing = 16f;
            line.childControlWidth = line.childControlHeight = true;
            line.childForceExpandWidth = false;
            line.childForceExpandHeight = false;
            var label = widgets.Label(heading, RateLabels[index], 22f);
            var size = label.gameObject.AddComponent<LayoutElement>();
            size.flexibleWidth = 1f;
            size.minWidth = 0f;
            Draft.Bind(index, widgets.BuildInput(heading));
        }

        private void BuildActions(RectTransform window, UnityAction apply, UnityAction close)
        {
            var buttons = Rect("Actions", window);
            var actions = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            actions.spacing = 16f;
            actions.childControlWidth = actions.childControlHeight = true;
            actions.childForceExpandWidth = true;
            actions.childForceExpandHeight = false;
            widgets.AddButton(buttons, Localization.Apply, apply);
            widgets.AddButton(buttons, Localization.Cancel, close);
            widgets.AddButton(buttons, Localization.Defaults, Draft.RestoreDefaults);
        }
    }
}
