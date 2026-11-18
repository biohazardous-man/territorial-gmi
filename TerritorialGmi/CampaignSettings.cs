using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace FrontlineLogistics.TerritorialGmi
{
    internal static class CampaignSettings
    {
        internal const string SaveKey = "local.frontlinelogistics.territorial-gmi.settings-v1";
        private const int Version = 1;
        private const int VersionIndex = 0;
        private const int EnabledIndex = 1;
        private const int MinimumIndex = 2;
        private const int TileIndex = 3;
        private const int RoadIndex = 4;
        private const int RailIndex = 5;
        private const int BridgeIndex = 6;
        private const int BuildingIndex = 7;
        private const int SaveValueCount = 8;
        private static readonly int[] saveValues = new int[SaveValueCount];
        private static Rates legacyRates = Plugin.DefaultRates;
        private static bool initialized;
        private static bool hasPendingCampaign;
        private static bool pendingEnabled;
        private static Rates pendingRates;

        internal static bool DraftEnabled { get; set; } = true;
        internal static Rates DraftRates { get; private set; } = Plugin.DefaultRates;
        internal static bool CurrentEnabled { get; private set; }
        internal static Rates CurrentRates { get; private set; } = Plugin.DefaultRates;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            string path = Path.Combine(Paths.ConfigPath, Plugin.Id + ".cfg");
            if (!File.Exists(path)) return;
            try
            {
                Rates rates = ReadLegacyRates(path);
                ValidateRates(rates);
                legacyRates = rates;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Territorial GMI could not read legacy rates; defaults will be used. " + exception.Message);
            }
        }

        private static Rates ReadLegacyRates(string path)
        {
            // This separate configuration is read once and is never saved.
            var legacy = new ConfigFile(path, false) { SaveOnConfigSet = false };
            Rates defaults = Plugin.DefaultRates;
            return new Rates
            {
                Minimum = legacy.Bind("Income", "MinimumDailyIncome", defaults.Minimum).Value,
                Tile = legacy.Bind("Income", "PerTile", defaults.Tile).Value,
                Road = legacy.Bind("Income", "RoadBonus", defaults.Road).Value,
                Rail = legacy.Bind("Income", "RailBonus", defaults.Rail).Value,
                Bridge = legacy.Bind("Income", "BridgeBonus", defaults.Bridge).Value,
                Building = legacy.Bind("Income", "BuildingBonus", defaults.Building).Value
            };
        }

        internal static void ApplyDraftRates(Rates rates)
        {
            ValidateRates(rates);
            DraftRates = rates;
        }

        internal static void ResetDraft()
        {
            DraftEnabled = true;
            DraftRates = Plugin.DefaultRates;
        }

        internal static void CaptureNewCampaign()
        {
            pendingEnabled = DraftEnabled;
            pendingRates = DraftRates;
            hasPendingCampaign = true;
        }

        internal static void BeginSession()
        {
            CurrentEnabled = false;
            CurrentRates = Plugin.DefaultRates;
            if (!hasPendingCampaign) return;
            CurrentEnabled = pendingEnabled;
            CurrentRates = pendingRates;
            hasPendingCampaign = false;
            pendingEnabled = false;
            pendingRates = default(Rates);
        }

        internal static void Save(string path)
        {
            Rates rates = CurrentRates;
            saveValues[VersionIndex] = Version;
            saveValues[EnabledIndex] = CurrentEnabled ? 1 : 0;
            saveValues[MinimumIndex] = rates.Minimum;
            saveValues[TileIndex] = rates.Tile;
            saveValues[RoadIndex] = rates.Road;
            saveValues[RailIndex] = rates.Rail;
            saveValues[BridgeIndex] = rates.Bridge;
            saveValues[BuildingIndex] = rates.Building;
            ES3.Save(SaveKey, saveValues, path);
        }

        internal static void Load(string path)
        {
            CurrentEnabled = false;
            CurrentRates = Plugin.DefaultRates;
            if (!ES3.KeyExists(SaveKey, path))
            {
                CurrentRates = legacyRates;
                CurrentEnabled = true;
                return;
            }

            int[] values = ES3.Load<int[]>(SaveKey, path);
            if (values == null || values.Length != SaveValueCount || values[VersionIndex] != Version
                || (values[EnabledIndex] != 0 && values[EnabledIndex] != 1))
                throw new InvalidOperationException("Invalid territorial GMI settings in save.");
            var rates = new Rates
            {
                Minimum = values[MinimumIndex],
                Tile = values[TileIndex],
                Road = values[RoadIndex],
                Rail = values[RailIndex],
                Bridge = values[BridgeIndex],
                Building = values[BuildingIndex]
            };
            ValidateRates(rates);
            CurrentRates = rates;
            CurrentEnabled = values[EnabledIndex] == 1;
        }

        private static void ValidateRates(Rates rates)
        {
            ValidateRate(rates.Minimum);
            ValidateRate(rates.Tile);
            ValidateRate(rates.Road);
            ValidateRate(rates.Rail);
            ValidateRate(rates.Bridge);
            ValidateRate(rates.Building);
        }

        private static void ValidateRate(int value)
        {
            if (value < 0 || value > Rates.MaximumValue)
                throw new ArgumentOutOfRangeException(nameof(value), "Income rates must be between 0 and 1000000.");
        }
    }
}
