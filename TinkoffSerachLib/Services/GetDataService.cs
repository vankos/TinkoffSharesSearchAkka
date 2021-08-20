using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffSerachLib.Models;

namespace TinkoffSerachLib.Services
{
    public static class GetDataService
    {
        private static async Task<List<Security>> GetCandlesForAllSharesOnDate(Context context,DateTime startDate, DateTime endDate, Currency currency)
        {
            if (startDate > endDate)
                throw new ArgumentException("Первая дата больше второй");

            CandleInterval interval = CandleInterval.Week;
            if ((endDate - startDate).TotalDays <= 7) interval = CandleInterval.Hour;
            else if ((endDate - startDate).TotalDays <= 90) interval = CandleInterval.Day;
            try
            {
                MarketInstrumentList markertlist = await context.MarketStocksAsync();
                Portfolio portfolio = await context.PortfolioAsync();
                PortfolioCurrencies portfolioCurrencies = await context.PortfolioCurrenciesAsync();

                decimal rubles = portfolioCurrencies.Currencies.Find(balanceCurr => balanceCurr.Currency == Currency.Rub).Balance;
                decimal usdPrice = portfolio.Positions.First(pos => pos.Ticker == "USD000UTSTOM").AveragePositionPrice.Value;
                decimal portfolioCost = portfolio.Positions.Select(pos =>
                {
                    if (pos.AveragePositionPrice.Currency == Currency.Usd)
                        return pos.AveragePositionPrice.Value * pos.Balance;
                    else
                        return (pos.AveragePositionPrice.Value * pos.Balance) / usdPrice;
                }).Sum() + rubles / usdPrice;

                if (currency == Currency.Rub)
                    portfolioCost *= usdPrice;

                int failedInstrumentsCounter = 0;
                List<Security> securities = new List<Security>();
                foreach (var instrument in markertlist.Instruments.Where((i) => i.Currency == currency))
                {
                    try
                    {
                        Thread.Sleep(250);
                        List<CandlePayload> candles = (await context.MarketCandlesAsync(instrument.Figi, DateTime.SpecifyKind(startDate, DateTimeKind.Local), DateTime.SpecifyKind(endDate, DateTimeKind.Local), interval)).Candles;
                        if (candles.Count>0)
                        {
                            securities.Add(new Security()
                            {
                                Name = instrument.Name,
                                Growth = Math.Round((candles[0].Open - candles.Last().Close) / candles.Last().Close * 100, 2) * -1,
                                Linearity = GetMadeUpCoeff(candles)

                            });
                        }
                    }
                    catch (Exception e)
                    {
                        failedInstrumentsCounter++;
                    }
                }
                string message = $"Всего {securities.Count + failedInstrumentsCounter} акций \nУспешно {securities.Count} акций\nЗафейлилось {failedInstrumentsCounter} акций";
                MessageService.SendMessage(message, true);
                return securities;


            }
            catch (Exception e)
            {
                MessageService.SendMessage(e.Message, false);
                return null;
            }
        }

        private static decimal GetMadeUpCoeff(List<CandlePayload> candles)
        {
            decimal k = (candles.Last().Close - candles.First().Close) / candles.Count;
            decimal b = candles.First().Close;
            List<decimal> diffs = new List<decimal>();
            for (int x = 1; x <= candles.Count; x++)
            {
                diffs.Add(Math.Abs(candles[x - 1].Close - (k * x + b)) / (k * x + b));
            }
            return diffs.Sum();
        }
    }
}
