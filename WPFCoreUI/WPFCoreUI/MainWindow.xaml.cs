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
using Microsoft.Toolkit.Uwp.Notifications;
using System.ComponentModel;
using System.Windows.Input;
using WPFController;
using System.Xml;
using Notifications.Wpf.Core;

namespace Tinkoff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand ShowCommand { get; set; } = new RoutedCommand();
        private const string saveFilePath = "savedData.dat";
        private Context context;
        private readonly SaveData savedata;
        private readonly bool IsInitializing;
        private WPFController.WPFController controller;
        public MainWindow()
        {
            IsInitializing = true;
            controller = new WPFController.WPFController();
            InitializeComponent();
            
            ShowCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.None));

            savedata = SaveData.GetSaveData(saveFilePath);
            DataContext = savedata;
            TokenTextEdit.Text = savedata.Token + "";
            USDRadioButton.IsChecked = savedata.Currency == Currency.Usd;
            RubRadioButton.IsChecked = savedata.Currency == Currency.Rub;
            try
            {
                context = GetContext(savedata.Token);
                ErrorTextBlock.Text = "";
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Неправильный токен";
            }
            IsInitializing = false;
            StartDate.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDate.SelectedDate = DateTime.Now;
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                context = GetContext(savedata.Token);
                ErrorTextBlock.Text = "";
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Неправильный токен";
            }
        }

        private Context GetContext(string token)
        {
            try
            {
                var connection = ConnectionFactory.GetConnection(token);
                ErrorTextBlock.Text = "";
                return connection.Context;
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Не удалось получить контекст";
                throw;
            }
        }

        private async Task GetPriceChange(Context context, Currency curr, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            { ErrorTextBlock.Text = "Первая дата больше второй"; return; }

            CandleInterval interval;
            if ((endDate - startDate).TotalDays <= 7) interval = CandleInterval.Hour;
            else if ((endDate - startDate).TotalDays <= 90) interval = CandleInterval.Day;
            else interval = CandleInterval.Week;
            try
            {
                ErrorTextBlock.Text = "Идет загрузка";
                decimal priceLimit = savedata.MoneyLimitValue;
                MarketInstrumentList markertlist = await context.MarketStocksAsync();
                DataTable diff = new DataTable();
                diff.Columns.Add("Name");
                diff.Columns.Add("Change", typeof(decimal));
                diff.Columns.Add("Линейность");
                diff.Columns.Add("Стоп-лосс");
                diff.Columns.Add("Zacks рейтинг");
                int failedInstrumentCount = 0;

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

                if (curr == Currency.Rub)
                    portfolioCost *= usdPrice;

                foreach (var instrument in markertlist.Instruments.Where((i) => i.Currency == curr))
                {
                    try
                    {
                        Thread.Sleep(250);
                        List<CandlePayload> candles = (await context.MarketCandlesAsync(instrument.Figi, DateTime.SpecifyKind(startDate, DateTimeKind.Local), DateTime.SpecifyKind(endDate, DateTimeKind.Local), interval)).Candles;
                        if (candles.Count > 0 && candles.Last().Close < priceLimit)
                        {

                            string name = instrument.Name;
                            decimal growth = Math.Round((candles[0].Open - candles.Last().Close) / candles.Last().Close * 100, 2) * -1;
                            decimal linearity = GetMadeUpCoeff(candles);
                            decimal stoploss = CalcStopLoss(portfolioCost, candles.Last().Close, priceLimit);
                            string zacksScore = "";//linearity<(decimal)0.5 && curr!=Currency.Rub && growth>0? await GetZacksScore(instrument.Ticker):"";
                            if ((!filterNonLinear.IsChecked ?? false) || linearity < savedata.Linearity)
                                diff.Rows.Add(name, growth, linearity.ToString("n2"), stoploss, zacksScore);
                        }
                    }
                    catch (Exception e)
                    {
                        failedInstrumentCount++;
                    }
                }
                DataDataGrid.ItemsSource = diff.DefaultView;
                string resultText = $"Всего {diff.Rows.Count + failedInstrumentCount} акций \nПоказано {diff.Rows.Count} акций\nЗафейлилось {failedInstrumentCount} акций";
                ErrorTextBlock.Text = resultText;

                var notificationManager = new NotificationManager();
                await notificationManager.ShowAsync(new NotificationContent
                {
                    Title = "Загрузка завершена",
                    Message = resultText,
                    Type = NotificationType.Success
                });
            }
            catch (Exception e)
            {
                ErrorTextBlock.Text = e.Message;
            }
        }

        private decimal CalcStopLoss(decimal portfolioCost, decimal close, decimal priceLimit)
        {
            decimal acceptableLoseForShare = (portfolioCost * (decimal)0.015) / (decimal.Round(priceLimit / close));
            return close - acceptableLoseForShare;
        }

        private decimal GetMadeUpCoeff(List<CandlePayload> candles)
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

        private async Task<string> GetZacksScore(string secCode)
        {
            MatchCollection matches;
            using (WebClient client = new WebClient())
            {
                string htmlCode = await client.DownloadStringTaskAsync($"https://zacks.com/stock/quote/{secCode}");
                matches = Regex.Matches(htmlCode, " <p class=\"rank_view\">\\n\\s*(\\d.*?)<span");
            }

            if (matches.Count > 0)
            {
                if (matches[0].Groups.Count > 1)
                    return matches[0].Groups[1].Value;

            }
            return string.Empty;



        }

        private void RubRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            savedata.Currency = RubRadioButton?.IsChecked == true ? Currency.Rub : Currency.Usd;
            savedata.Save(saveFilePath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            savedata.Save(saveFilePath);
        }

        private async void ShowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            if (context != null)
                await GetPriceChange(context, savedata.Currency, savedata.StartDate, savedata.EndDate);
        }

    }

    [Serializable]
    public class SaveData
    {
        public string Token { get; set; }
        public decimal MoneyLimitValue { get; set; }
        public Currency Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Linearity { get; set; }
        public static SaveData GetSaveData(string saveFilePath)
        {
            try
            {
                using (FileStream fs = File.OpenRead(saveFilePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    return (SaveData)bf.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                return new SaveData();
            }
        }

        public void Save(string saveFilePath)
        {
            using (FileStream fs = File.OpenWrite(saveFilePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }
    }
}
