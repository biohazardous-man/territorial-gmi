using System;

namespace FrontlineLogistics.TerritorialGmi
{
    internal struct Holdings
    {
        public int Tiles;
        public int Roads;
        public int Rails;
        public int Bridges;
        public int Buildings;
    }

    internal struct Rates
    {
        internal const int MaximumValue = 1000000;
        internal const int MaximumDigits = 7;
        internal const int FieldCount = 6;

        public int Minimum;
        public int Tile;
        public int Road;
        public int Rail;
        public int Bridge;
        public int Building;
    }

    internal static class Income
    {
        internal static int Calculate(Holdings holdings, Rates rates)
        {
            if (holdings.Tiles == 0) return 0;
            long total = Math.Max(rates.Minimum, (long)holdings.Tiles * rates.Tile)
                + (long)holdings.Roads * rates.Road
                + (long)holdings.Rails * rates.Rail
                + (long)holdings.Bridges * rates.Bridge
                + (long)holdings.Buildings * rates.Building;
            return (int)Math.Min(int.MaxValue, total);
        }

        internal static bool IsDue(DateTime date, long paidThrough)
        {
            return date.Date.Ticks > paidThrough;
        }

        internal static int CreditAmount(int balance, int income)
        {
            return (int)Math.Min(income, Math.Max(0L, (long)int.MaxValue - balance));
        }
    }
}
