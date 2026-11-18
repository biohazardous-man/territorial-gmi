using System;
using System.Text;
using BepInEx;
using HarmonyLib;

namespace FrontlineLogistics.TerritorialGmi
{
    [BepInPlugin(Id, "Territorial GMI", "1.2.1")]
    [BepInProcess("FrontlineLogistics.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Id = "local.frontlinelogistics.territorial-gmi";
        internal static Plugin Instance;
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            CampaignSettings.Initialize();
            harmony = new Harmony(Id);
            harmony.PatchAll(typeof(Plugin).Assembly);
            gameObject.AddComponent<SettingsPanel>();
            Logger.LogInfo("Territorial GMI loaded with campaign-local settings.");
        }

        internal static Rates DefaultRates => new Rates
        {
            Minimum = 10000, Tile = 5, Road = 20, Rail = 40, Bridge = 150, Building = 75
        };


        internal string PayDaily()
        {
            if (!CampaignSettings.CurrentEnabled || !DataManager.GameReady || !DataManager.MainBaseInitialized)
                return string.Empty;

            DateTime date = TimeManager.currentDate.Date;
            if (!Income.IsDue(date, Persistence.PaidThrough))
                return string.Empty;

            Holdings holdings = Territory.Count();
            Rates rates = CampaignSettings.CurrentRates;
            int calculated = Income.Calculate(holdings, rates);
            var wallet = GrossMilitaryIndexSystem.grossMilitaryIndexSystemInstance;
            int credited = Income.CreditAmount(wallet.GMINum, calculated);
            if (credited > 0 && !wallet.ChangeGMIValue(credited, false))
                throw new InvalidOperationException("Territorial GMI payment was rejected.");
            Persistence.PaidThrough = date.Ticks;

            var culture = Localization.Culture;
            var text = new StringBuilder(512);
            text.Append('\n').Append(Localization.Text(Localization.Title)).Append(" — ").Append(date.ToString("d", culture)).Append('\n');
            AppendCategory(text, Localization.ControlledTiles, holdings.Tiles, rates.Tile);
            AppendCategory(text, Localization.Roads, holdings.Roads, rates.Road);
            AppendCategory(text, Localization.Rails, holdings.Rails, rates.Rail);
            AppendCategory(text, Localization.Bridges, holdings.Bridges, rates.Bridge);
            AppendCategory(text, Localization.Buildings, holdings.Buildings, rates.Building);
            text.Append(Localization.Text(Localization.LandMinimum)).Append(": ").Append(rates.Minimum.ToString("N0", culture)).Append('\n');
            text.Append(Localization.Text(Localization.Credited)).Append(": +").Append(credited.ToString("N0", culture)).Append(" GMI\n");
            if (credited < calculated) text.Append(Localization.Text(Localization.WalletLimit)).Append('\n');
            string report = text.ToString();
            Logger.LogInfo($"Territorial income {date:yyyy-MM-dd}: credited={credited} GMI; tiles={holdings.Tiles}; roads={holdings.Roads}; rails={holdings.Rails}; bridges={holdings.Bridges}; buildings={holdings.Buildings}.");
            return report;
        }

        private static void AppendCategory(StringBuilder report, int label, int count, int rate)
        {
            var culture = Localization.Culture;
            report.Append(Localization.Text(label)).Append(": ").Append(count.ToString("N0", culture))
                .Append(" × ").Append(rate.ToString("N0", culture)).Append('\n');
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Instance = null;
        }

        [HarmonyPatch(typeof(TimeManager), nameof(TimeManager.FacilityRating))]
        private static class DailyIncome
        {
            private static bool Prefix(ref string __result)
            {
                if (!CampaignSettings.CurrentEnabled) return true;
                __result = Instance.PayDaily();
                return false;
            }
        }
    }
}
