using System;
using HarmonyLib;

namespace FrontlineLogistics.TerritorialGmi
{
    internal static class Persistence
    {
        internal const string SaveKey = "local.frontlinelogistics.territorial-gmi.paid-through-v1";
        internal static long PaidThrough;

        internal static void Save(string path)
        {
            // Use the game's exact save path: autosaves, copies and manual slots stay independent.
            CampaignSettings.Save(path);
            ES3.Save(SaveKey, PaidThrough, path);
        }

        internal static void Load(string path)
        {
            // Old saves start earning on their next date, never on loading the save itself.
            long baseline = SaveManager.saveManagerInstance.timeManagerSaveData.currentDate.Date.Ticks;
            long paidThrough = ES3.Load<long>(SaveKey, path, baseline);
            if (paidThrough < 0 || paidThrough > DateTime.MaxValue.Ticks)
                throw new InvalidOperationException("Invalid territorial GMI payment date in save.");
            CampaignSettings.Load(path);
            PaidThrough = paidThrough;
        }

        [HarmonyPatch(typeof(DataManager), "Awake")]
        private static class NewSession
        {
            private static void Postfix(DataManager __instance)
            {
                if (!__instance.isOpeningSence)
                {
                    CampaignSettings.BeginSession();
                    PaidThrough = GameGlobalVariables.DefaultInitialDate.Date.Ticks;
                }
            }
        }

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.Save))]
        private static class SaveHook
        {
            private static void Postfix(string saveFolderPath)
            {
                Save(saveFolderPath);
            }
        }

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.Load))]
        private static class LoadHook
        {
            private static void Postfix(string saveFolderPath)
            {
                Load(saveFolderPath);
            }
        }
    }
}
