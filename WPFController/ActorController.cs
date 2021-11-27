using Akka.Actor;
using System;
using System.Collections.Generic;
using TinkoffSearchLib.Models;
using TinkoffSearchLib.Services;
using TinkoffSearchLib.Messages;
using System.Linq;

namespace WPFController
{
    public class ActorController:ReceiveActor
    {
        public UserData UserData { get; set; }
        public List<Security> UnflteredData { get; set; } = new();
        private IActorRef getDataService;
        private IActorRef analytycalService;

        public ActorController()
        {
            UserData =  SaveService.LoadData();
            UserData.OnLinearityChanged += (_, _) => analytycalService.Tell(new LinearityMessage(UnflteredData));
            UserData.OnMoneyLimitValueChanged += (_, _) => analytycalService.Tell(new GrowthMessage(UnflteredData));

            getDataService = Context.ActorOf(Props.Create(() => new GetDataService(UserData.Token)));
            analytycalService = Context.ActorOf(Props.Create(() => new AnalyticalService()));

            Receive<string>(message =>
            {
                switch(message)
                {
                    case SimpleMessages.SaveUserData:
                        SaveUserData();
                        Sender.Tell(new object());
                        break;
                    case SimpleMessages.GetData:
                        GetData();
                        break;
                    case SimpleMessages.ContextRequest:
                        Sender.Tell(UserData);
                        break;
                }
            });

            Receive<TextMessage>(msg => Context.Parent.Tell(msg));

            Receive<UnfilteredDataMessage>(msg => 
            {
                UnflteredData = msg.Securities;
                analytycalService.Tell(new GrowthMessage(UnflteredData));
            });

            Receive<GrowthMessage>(msg => analytycalService.Tell(new LinearityMessage(msg.Securities)));
            Receive<LinearityMessage>(msg => Context.Parent.Tell(msg.Securities.Where(s=>s.Linearity<=UserData.Linearity&& s.Candles.Last().Close<=UserData.MoneyLimit).ToList()));
        }

        private void SaveUserData()
        {
            SaveService.SaveData(UserData);
        }

        private void GetData()
        {
            if (UserData.Token == null && getDataService is null)
                Context.Parent.Tell(new TextMessage("Нет токена", false));
            try
            {
                getDataService.Tell(new GetDataMessage(UserData.StartDate, UserData.EndDate, UserData.Currency));
            }
            catch (Exception ex)
            {
                Context.Parent.Tell(new TextMessage($"Ошибка получения данных:{ex.Message}", false));
            }
        }
    }
}
