using System;
using System.Collections.Generic;
using System.Linq;
using TinkoffSearchLib.Models;

namespace TinkoffSearchLib.Services
{
    public static class AnalyticalService
    {
        public static List<Security> GetGrowth(List<Security> securities)
        {
            securities = securities.ToList();
            foreach (var security in securities)
            {
               security.Growth = Math.Round((security.Candles.Last().Close - security.Candles[0].Open) / security.Candles.Last().Close * 100, 2);
            }
            return securities;
        }

        public static List<Security> GetLinearity(List<Security> securities)
        {
            securities = securities.ToList();
            foreach (var security in securities)
            {
                decimal k = (security.Candles.Last().Close - security.Candles[0].Close) / security.Candles.Count;
                decimal b = security.Candles[0].Close;
                List<decimal> diffs = new();
                for (int x = 1; x <= security.Candles.Count; x++)
                {
                    diffs.Add(Math.Abs(security.Candles[x - 1].Close - ((k * x) + b)) / ((k * x) + b));
                }
                security.Linearity = diffs.Sum();
            }
            return securities;
        }
    }
}
