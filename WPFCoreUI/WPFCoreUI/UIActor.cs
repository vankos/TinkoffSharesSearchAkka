using Akka.Actor;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffSearchLib.Messages;
using TinkoffSearchLib.Models;
using WPFController;
using static WPFController.ActorController;

namespace WPFCoreUI
{
    public class UIActor : ReceiveActor
    {
        private readonly IActorRef controller;

        public UIActor(MainWindow mainWindow)
        {
            controller = Context.ActorOf(Props.Create(() =>new ActorController()));
            
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.DataContext = mainWindow.UserData =  controller.Ask(SimpleMessages.ContextRequest).Result as UserData;
                mainWindow.USDRadioButton.IsChecked = mainWindow.UserData.Currency == Currency.Usd;
                mainWindow.RubRadioButton.IsChecked = mainWindow.UserData.Currency == Currency.Rub;
            });

            Receive<TextMessage>(message => mainWindow.Dispatcher.Invoke(()=>
            {
                mainWindow.Dispatcher.Invoke(() => mainWindow.ErrorTextBlock.Text = message.MessageText);
                if (message.ShowNotification)
                {
                    var notificationManager = new NotificationManager();
                    notificationManager.ShowAsync(new NotificationContent
                    {
                        Title = "Tinkoff shares",
                        Message = message.MessageText,
                        Type = NotificationType.Success
                    });
                }
            }));

            Receive<List<Security>>(secs => mainWindow.Dispatcher.Invoke(() =>
            mainWindow.DataDataGrid.ItemsSource = secs));
            
            Receive<UserData>(msg=>
            {
                if(Sender.Path == Context.Parent.Path)
                    controller.Tell(msg);
                else
                    mainWindow.Dispatcher.Invoke(() => mainWindow.DataContext = mainWindow.UserData = msg);

            });

            Receive<string>(message =>
            {
                switch (message)
                {
                    case SimpleMessages.SaveUserData:
                        Sender.Tell(controller.Ask(message).Result);
                        break;
                    case SimpleMessages.GetData:
                        controller.Tell(message);
                        break;
                }
            });

        }
    }
}
