using System;
using FrontlineLogistics.TerritorialGmi;

static class Program
{
    static void Equal<T>(T expected, T actual, string scenario)
    {
        if (!Equals(expected, actual)) throw new Exception($"{scenario}: expected {expected}, got {actual}");
    }

    static void Main()
    {
        var rates = new Rates { Minimum = 10000, Tile = 20, Road = 40, Rail = 60, Bridge = 200, Building = 100 };
        Equal(0, Income.Calculate(default, rates), "No territory does not receive guaranteed income");
        Equal(10000, Income.Calculate(new Holdings { Tiles = 1 }, rates), "Small foothold receives minimum");
        Equal(10200, Income.Calculate(new Holdings { Tiles = 1, Bridges = 1 }, rates), "Infrastructure still pays extra below the land minimum");
        Equal(10400, Income.Calculate(new Holdings { Tiles = 500, Roads = 1, Rails = 1, Bridges = 1, Buildings = 1 }, rates), "Infrastructure bonuses stack once per category, minimum is not added");
        Equal(10200, Income.Calculate(new Holdings { Tiles = 500, Bridges = 1 }, rates), "Bridge is worth more than bare land");
        Equal(0, Income.Calculate(new Holdings { Tiles = 10 }, default), "Zero configured rates disable payment");
        Equal(int.MaxValue, Income.Calculate(new Holdings { Tiles = int.MaxValue, Bridges = int.MaxValue }, rates), "Large map cannot overflow income");
        Equal(5, Income.CreditAmount(int.MaxValue - 5, 10000), "Credit cannot overflow wallet");
        Equal(0, Income.CreditAmount(int.MaxValue, 10000), "Full wallet remains full");
        var day = new DateTime(2026, 11, 17);
        Equal(false, Income.IsDue(day.AddHours(23), day.Ticks), "Repeated daily report cannot repay");
        Equal(true, Income.IsDue(day.AddDays(1), day.Ticks), "Next date pays");
        Equal(false, Income.IsDue(day.AddDays(-1), day.Ticks), "Clock rewind cannot repay");
        Equal(true, Income.IsDue(day.AddDays(10), day.Ticks), "Skipped dates permit one current payment, not catch-up");
        Console.WriteLine("PASS: 13 income and payment-date regression scenarios.");
    }
}
