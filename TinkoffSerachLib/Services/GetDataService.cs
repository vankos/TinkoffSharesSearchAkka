using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffSearchLib.Messages;
using TinkoffSearchLib.Models;

namespace TinkoffSearchLib.Services
{
    public class GetDataService : ReceiveActor
    {
        private readonly Context context;
        public GetDataService(string token)
        {
            try
            {
                var connection = ConnectionFactory.GetConnection(token);
                context = connection.Context;
            }
            catch (Exception)
            {
                Context.Parent.Tell(new TextMessage("Не удалось получить контекст", false));
            }

            Receive<GetDataMessage>(msg => Sender.Tell(new UnfilteredDataMessage(GetCandlesForAllSharesOnDate(msg.StartDate, msg.EndDate, msg.Currency))));
        }

        private List<Security> GetCandlesForAllSharesOnDate(DateTime startDate, DateTime endDate, Currency currency)
        {
            if (startDate > endDate)
            {
                Context.Parent.Tell(new TextMessage("Первая дата больше второй", false));
                return new List<Security>();
            }

            CandleInterval interval = CandleInterval.Week;
            if ((endDate - startDate).TotalDays <= 7) interval = CandleInterval.Hour;
            else if ((endDate - startDate).TotalDays <= 90) interval = CandleInterval.Day;
            try
            {
                MarketInstrumentList markertlist = context.MarketStocksAsync().Result;

                int failedInstrumentsCounter = 0;
                List<Security> securities = new();
                foreach (var instrument in markertlist.Instruments.Where((i) => i.Currency == currency))
                {
                    try
                    {
                        Thread.Sleep(250);
                        List<CandlePayload> candles = (context.MarketCandlesAsync(instrument.Figi, DateTime.SpecifyKind(startDate, DateTimeKind.Local), DateTime.SpecifyKind(endDate, DateTimeKind.Local), interval)).Result.Candles;
                        if (candles.Count > 0)
                        {
                            securities.Add(new Security()
                            {
                                Name = instrument.Name,
                                Candles = candles
                            });
                        }
                    }
                    catch (Exception)
                    {
                        failedInstrumentsCounter++;
                    }
                }
                string message = $"Всего {securities.Count + failedInstrumentsCounter} акций \nУспешно {securities.Count} акций\nЗафейлилось {failedInstrumentsCounter} акций";
                Context.Parent.Tell(new TextMessage(message, true));
                return securities;
            }
            catch (Exception e)
            {
                Context.Parent.Tell(new TextMessage(e.Message, false));
                return new List<Security>();
            }
        }
    }

    #region Messages
    public class GetDataMessage
    {
        public GetDataMessage(DateTime startDate, DateTime endDate, Currency currency)
        {
            StartDate = startDate;
            EndDate = endDate;
            Currency = currency;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Currency Currency { get; set; }
    }

    public class UnfilteredDataMessage
    {
        public UnfilteredDataMessage(List<Security> unfilteredData)
        {
            Securities = unfilteredData;
        }

        public List<Security> Securities { get; set; }
    }
    #endregion
}
