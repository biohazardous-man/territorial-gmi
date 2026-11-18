using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FrontlineLogistics.TerritorialGmi
{
    internal static class Localization
    {
        internal const int Title = 1;
        internal const int ControlledTiles = 4;
        internal const int Roads = 5;
        internal const int Rails = 6;
        internal const int Bridges = 7;
        internal const int Buildings = 8;
        internal const int LandMinimum = 9;
        internal const int Credited = 10;
        internal const int WalletLimit = 11;
        internal const int SettingsTitle = 20;
        internal const int MinimumLabel = 21;
        internal const int TileLabel = 22;
        internal const int RoadLabel = 23;
        internal const int RailLabel = 24;
        internal const int BridgeLabel = 25;
        internal const int BuildingLabel = 26;
        internal const int Apply = 27;
        internal const int Cancel = 28;
        internal const int Defaults = 29;
        internal const int Saved = 30;
        internal const int InvalidRange = 31;
        internal const int Configure = 42;

        private const string FileName = "TerritorialGmi.csv";
        private static readonly string[] CultureNames =
        {
            "en-US", "zh-CN", "zh-TW", "ja-JP", "ru-RU", "ko-KR",
            "pt-BR", "nl-NL", "fr-FR", "de-DE", "pl-PL", "es-ES",
            "tr-TR", "sv-SE", "fi-FI", "cs-CZ"
        };
        private static Dictionary<int, string> texts;
        private static int cachedLanguage = -1;
        private static bool loaded;

        internal static CultureInfo Culture
        {
            get
            {
                int language = GameSettingsManager.LanguageID;
                return CultureInfo.GetCultureInfo(CultureNames[
                    language >= 0 && language < CultureNames.Length ? language : 0]);
            }
        }

        internal static string Text(int id)
        {
            if (!loaded)
            {
                string path = Path.Combine(Path.GetDirectoryName(typeof(Localization).Assembly.Location),
                    "Localization", FileName);
                if (!File.Exists(path))
                    throw new FileNotFoundException("Territorial GMI localization file is missing.", path);
                CSVReader.LoadCSV(path);
                loaded = true;
            }

            int language = GameSettingsManager.LanguageID;
            if (texts == null || cachedLanguage != language)
            {
                texts = CSVReader.ReadCsvData(FileName);
                cachedLanguage = language;
            }
            return texts[id];
        }
    }
}
