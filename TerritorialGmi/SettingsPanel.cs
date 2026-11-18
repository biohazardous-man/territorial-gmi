using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using lgBase;

namespace FrontlineLogistics.TerritorialGmi
{
    [DefaultExecutionOrder(-10000)]
    internal sealed class SettingsPanel : MonoBehaviour
    {
        private static SettingsPanel activePanel;
        private SettingsPanelView view;
        private readonly List<BaseRaycaster> blockedRaycasters = new List<BaseRaycaster>();
        private EventSystem eventSystem;
        private GameObject previousSelection;
        private bool previousNavigation;
        private DataManager previousData;
        private bool previousSystemPause;
        private bool closing;
        private int closeFrame;

        internal bool IsOpen { get; private set; }
        private static bool BlocksGameInput => activePanel != null && (activePanel.IsOpen || activePanel.closing);

        private void Awake()
        {
            activePanel = this;
            SceneManager.activeSceneChanged += OnSceneChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void Update()
        {
            if (IsOpen)
            {
                if (!Ready())
                {
                    Close();
                    return;
                }
                if (view.Language != GameSettingsManager.LanguageID)
                    view.RefreshText();
                if (Input.GetKeyDown(KeyCode.Escape))
                    Close();
            }
        }

        private void LateUpdate()
        {
            // Keep the closing key out of the native input controller's LateUpdate.
            if (closing && Time.frameCount > closeFrame)
                ReleaseModal();
        }

        private static bool Ready()
        {
            var data = DataManager.dataManagerInstance;
            var ui = UIManager.uiManagerInstance;
            return Plugin.Instance != null && data != null && ui != null && EventSystem.current != null
                && (ui.loadingBackground == null || !ui.loadingBackground.activeInHierarchy)
                && data.isOpeningSence && CampaignSetup.IsVisible && CampaignSettings.DraftEnabled;
        }

        internal void Open()
        {
            if (IsOpen || closing || !Ready()) return;
            if (view == null) view = new SettingsPanelView();
            if (!view.FindNativeStyle()) return;
            if (view.Root == null) view.Build(transform, ApplyDraft, Close);
            view.Draft.SetDraft(CampaignSettings.DraftRates);
            view.SetStatus(0);
            view.RefreshText();
            AcquireModal();
            IsOpen = true;
            view.Show();
        }

        private void AcquireModal()
        {
            previousData = DataManager.dataManagerInstance;
            previousSystemPause = InputController.isSystemPause;
            InputController.isSystemPause = true;
            eventSystem = EventSystem.current;
            previousSelection = eventSystem.currentSelectedGameObject;
            previousNavigation = eventSystem.sendNavigationEvents;
            eventSystem.SetSelectedGameObject(null);
            eventSystem.sendNavigationEvents = false;
            blockedRaycasters.Clear();
            foreach (var raycaster in FindObjectsOfType<BaseRaycaster>())
            {
                if (raycaster.enabled && !raycaster.transform.IsChildOf(view.Root.transform))
                {
                    blockedRaycasters.Add(raycaster);
                    raycaster.enabled = false;
                }
            }
        }

        internal void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            closing = true;
            closeFrame = Time.frameCount;
            if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
            view.Hide();
        }

        private void ReleaseModal()
        {
            if (!IsOpen && !closing) return;
            IsOpen = false;
            closing = false;
            if (view != null) view.Hide();
            foreach (var raycaster in blockedRaycasters)
                if (raycaster != null) raycaster.enabled = true;
            blockedRaycasters.Clear();
            if (eventSystem != null)
            {
                eventSystem.sendNavigationEvents = previousNavigation;
                eventSystem.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection : null);
            }
            if (previousData != null && previousData == DataManager.dataManagerInstance)
            {
                InputController.isSystemPause = previousSystemPause;
            }
            previousData = null;
            previousSelection = null;
            eventSystem = null;
        }

        private void OnSceneChanged(Scene previous, Scene next) => ResetScene();
        private void OnSceneUnloaded(Scene scene) => ResetScene();

        private void ResetScene()
        {
            ReleaseModal();
            if (view != null && view.Root != null) Destroy(view.Root);
            view = null;
        }

        private void OnDisable() => ReleaseModal();

        private void OnDestroy()
        {
            ReleaseModal();
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (view != null && view.Root != null) Destroy(view.Root);
            if (activePanel == this) activePanel = null;
        }

        internal void ApplyDraft()
        {
            if (!IsOpen) return;
            Rates rates;
            if (!view.Draft.TryRead(out rates))
            {
                view.SetStatus(Localization.InvalidRange);
                return;
            }
            try
            {
                CampaignSettings.ApplyDraftRates(rates);
                view.SetStatus(Localization.Saved);
            }
            catch (ArgumentOutOfRangeException)
            {
                view.SetStatus(Localization.InvalidRange);
            }
        }

        [HarmonyPatch]
        private static class ModalInput
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(InputController), "LateUpdate");
                yield return AccessTools.Method(typeof(TileMapCameraController), "Update");
                yield return AccessTools.Method(typeof(PlanningLine), "Update");
                yield return AccessTools.Method(typeof(InputFieldView), "LateUpdate");
                yield return AccessTools.Method(typeof(CombatReportWriter), "LateUpdate");
                yield return AccessTools.Method(typeof(OpeningCredits), "Update");
                yield return AccessTools.Method(typeof(VehicleInternalSpaceUI), "Update");
                yield return AccessTools.Method(typeof(EventButton), "Update");
                yield return AccessTools.Method(typeof(OffMapSupportOrderUi), "Update");
                yield return AccessTools.Method(typeof(UnitWorkPriorityCellUI), "Update");
            }

            private static bool Prefix() => !BlocksGameInput;
        }
    }
}
