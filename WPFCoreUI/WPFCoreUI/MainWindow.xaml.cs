using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization.Formatters.Binary;
using Tinkoff.Trading.OpenApi.Network;
using Tinkoff.Trading.OpenApi.Models;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Notifications.Wpf.Core;
using TinkoffSearchLib.Models;
using Akka.Actor;
using WPFCoreUI;
using static WPFCoreUI.UIActor;
using TinkoffSearchLib.Messages;

namespace Tinkoff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand ShowCommand { get; set; } = new RoutedCommand();
        private readonly ActorSystem actorSystem;
        private readonly IActorRef uiActor;
        public UserData UserData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            actorSystem = ActorSystem.Create("actorSystem");
            uiActor = actorSystem.ActorOf(Props.Create(() => new UIActor(this)));
            DataContext = UserData;
            ShowCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.None));

            StartDate.SelectedDate = DateTime.Now.AddYears(-1);
            EndDate.SelectedDate = DateTime.Now;
            actorSystem = ActorSystem.Create("actorSystem");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uiActor.Ask(SimpleMessages.SaveUserData).Wait();
        }

        private void ShowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Зарузить данные?", "Зарузить данные?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ErrorTextBlock.Text = "Загрузка данных";
                uiActor.Tell(SimpleMessages.GetData);
            }
        }
    }
}
