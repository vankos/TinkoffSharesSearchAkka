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
using TinkoffSerachLib.Models;

namespace Tinkoff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand ShowCommand { get; set; } = new RoutedCommand();
        private readonly bool IsInitializing;
        private WPFController.WPFController controller;
        public MainWindow()
        {
            controller = new WPFController.WPFController();
            controller.OnMessageRecived += (_, e) =>ErrorTextBlock.Text = e;
            controller.OnMessageRecived += (_, e) =>
            {
                ErrorTextBlock.Text = e;
                var notificationManager = new NotificationManager();
                notificationManager.ShowAsync(new NotificationContent
                {
                    Title = "Tinkoff shares",
                    Message = e,
                    Type = NotificationType.Success
                });
            };
            
            InitializeComponent();
            
            ShowCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.None));

            DataContext = controller.UserData;
            USDRadioButton.IsChecked = controller.UserData.Currency == Currency.Usd;
            RubRadioButton.IsChecked = controller.UserData.Currency == Currency.Rub;

            StartDate.SelectedDate = DateTime.Now.AddYears(-1);
            EndDate.SelectedDate = DateTime.Now;
        }

        private void RubRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            controller.UserData.Currency = RubRadioButton?.IsChecked == true ? Currency.Rub : Currency.Usd;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.SaveUserData();
        }

        private async void ShowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorTextBlock.Text = "Загрузка данных";
            DataDataGrid.ItemsSource = await controller.GetAndFilterData();
        }

    }
}
