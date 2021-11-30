using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            Receive<GetDataMessage>(msg => GetCandlesForAllSharesOnDate(msg.UserData, Sender).PipeTo(Context.Parent));
        }

        private async Task<UnfilteredDataMessage> GetCandlesForAllSharesOnDate(UserData userData, IActorRef sender)
        {
            if (userData.StartDate > userData.EndDate)
            {
                Context.Parent.Tell(new TextMessage("Первая дата больше второй", false));
                return new UnfilteredDataMessage(new List<Security>());
            }

            CandleInterval interval = CandleInterval.Week;
            if ((userData.EndDate - userData.StartDate).TotalDays <= 7) interval = CandleInterval.Hour;
            else if ((userData.EndDate - userData.StartDate).TotalDays <= 90) interval = CandleInterval.Day;
            try
            {
                List<MarketInstrument> marketInstruments = new();
                if (userData.IsShares)
                    marketInstruments.AddRange((await context.MarketStocksAsync().ConfigureAwait(false)).Instruments);
                if (userData.IsETF)
                    marketInstruments.AddRange((await context.MarketEtfsAsync().ConfigureAwait(false)).Instruments);

                if (!userData.IsUSD)
                    marketInstruments = marketInstruments.Where(instr => instr.Currency == Currency.Rub).ToList();
                if (!userData.IsRUR)
                    marketInstruments = marketInstruments.Where(instr => instr.Currency == Currency.Eur).ToList();

                int failedInstrumentsCounter = 0;
                List<Security> securities = new();
                foreach (var instrument in marketInstruments)
                {
                    try
                    {
                        Thread.Sleep(250);
                        List<CandlePayload> candles = (await context.MarketCandlesAsync(instrument.Figi, DateTime.SpecifyKind(userData.StartDate, DateTimeKind.Local), DateTime.SpecifyKind(userData.EndDate, DateTimeKind.Local), interval).ConfigureAwait(false)).Candles;
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
                sender.Tell(new TextMessage(message, true));
                return new UnfilteredDataMessage(securities);
            }
            catch (Exception e)
            {
                sender.Tell(new TextMessage(e.Message, false));
                return new UnfilteredDataMessage(new List<Security>());
            }
        }
    }

    #region Messages
    public class GetDataMessage
    {
        public GetDataMessage(UserData userData)
        {
            UserData = userData;
        }

        public UserData UserData { get; set; }
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
