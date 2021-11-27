using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using TinkoffSearchLib.Models;

namespace TinkoffSearchLib.Services
{
    public class AnalyticalService : ReceiveActor
    {
        public AnalyticalService()
        {
            Receive<GrowthMessage>(msg => Context.Parent.Tell(new GrowthMessage(GetGrowth(msg.Securities))));
            Receive<LinearityMessage>(msg => Context.Parent.Tell(new LinearityMessage(GetLinearity(msg.Securities))));
        }

        private static List<Security> GetGrowth(List<Security> securities)
        {
            if (securities == null) return securities;

            securities = securities.ToList();
            foreach (var security in securities)
            {
               security.Growth = Math.Round((security.Candles.Last().Close - security.Candles[0].Open) / security.Candles[0].Open * 100, 2);
            }
            return securities;
        }

        private static List<Security> GetLinearity(List<Security> securities)
        {
            if (securities == null) return securities;

            securities = securities.ToList();
            foreach (var security in securities)
            {
                decimal k = (security.Candles.Last().Close - security.Candles[0].Open) / security.Candles.Count;
                decimal b = security.Candles[0].Open;
                List<decimal> diffs = new();
                for (int x = 1; x <= security.Candles.Count; x++)
                {
                        if(x==1|| x == security.Candles.Count)
                            diffs.Add(0);
                        else
                            diffs.Add(Math.Abs(((security.Candles[x - 1].Close + security.Candles[x - 1].Open) / 2) - ((k * x) + b)) / ((k * x) + b));
                }
                security.Linearity = diffs.Max()*100;
            }
            return securities;
        }
    }

    #region Messages
    public class GrowthMessage
    {
        public GrowthMessage(List<Security> securities)
        {
            this.Securities = securities;
        }
        public List<Security> Securities { get; set; }
    }

    public class LinearityMessage
    {
        public LinearityMessage(List<Security> securities)
        {
            this.Securities = securities;
        }
        public List<Security> Securities { get; set; }
    }
    #endregion
}
