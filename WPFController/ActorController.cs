using Akka.Actor;
using System;
using System.Collections.Generic;
using TinkoffSearchLib.Models;
using TinkoffSearchLib.Services;
using TinkoffSearchLib.Messages;
using System.Linq;

namespace WPFController
{
    public class ActorController : ReceiveActor
    {
        private const string SAVEFILEPATH = "savedData.dat";

        private UserData userData;
        public UserData UserData
        {
            get => userData;
            set
            {
                userData = value;
                userData.OnLinearityChanged += (_, _) => analytycalService.Tell(new LinearityMessage(UnflteredData));
                userData.OnMoneyLimitValueChanged += (_, _) => analytycalService.Tell(new GrowthMessage(UnflteredData));
            }
        }
        public List<Security> UnflteredData { get; set; } = new();
        private IActorRef getDataService;
        private readonly IActorRef analytycalService;
        private readonly IActorRef saveService;

        public ActorController()
        {
            analytycalService = Context.ActorOf(Props.Create(() => new AnalyticalService()));
            saveService = Context.ActorOf(Props.Create(() => new SaveService()));

            Receive<string>(message =>
            {
                switch (message)
                {
                    case SimpleMessages.SaveUserData:
                        Sender.Tell(saveService.Ask(new SaveUserDataMessage(UserData, SAVEFILEPATH)));
                        break;
                    case SimpleMessages.GetData:
                        GetData();
                        break;
                    case SimpleMessages.ContextRequest:
                        UserData = saveService.Ask(new LoadUserDataMessage(SAVEFILEPATH)).Result as UserData;
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

            Receive<LinearityMessage>(msg => Context.Parent.Tell(msg.Securities.Where(s => s.Linearity <= UserData.Linearity && s.Candles.Last().Close <= UserData.MoneyLimit).ToList()));
        }

        private void GetData()
        {
            if (UserData.Token == null && getDataService is null)
                Context.Parent.Tell(new TextMessage("Нет токена", false));
            if(getDataService==null)
                getDataService = Context.ActorOf(Props.Create(() => new GetDataService(UserData.Token)));
            try
            {
                getDataService.Tell(new GetDataMessage(UserData));
            }
            catch (Exception ex)
            {
                Context.Parent.Tell(new TextMessage($"Ошибка получения данных:{ex.Message}", false));
            }
        }
    }
}
