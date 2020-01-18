using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tinkoff.Trading.OpenApi.Network;
using Tinkoff.Trading.OpenApi.Models;
using System.Collections.ObjectModel;

namespace Tinkoff
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string saveFilePath = "savedData.dat";
        Context context = null;
        SaveData savedata= new SaveData();
        bool IsInitializing=true;

        public MainWindow()
        {
            InitializeComponent();
            savedata= savedata.GetSaveData();
            TokenTextEdit.Text = savedata.Token+"";
            MoneyLimit.Text = savedata.MoneyLimitValue + "";
            USDRadioButton.IsChecked = savedata.Currency == Currency.Usd;
            RubRadioButton.IsChecked = savedata.Currency == Currency.Rub;
            StartDate.SelectedDate = savedata.StartDate;
            EndDate.SelectedDate = savedata.EndDate;
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
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitializing) return;
            try
            {
                savedata.MoneyLimitValue = decimal.Parse(MoneyLimit.Text);
                savedata.Save();
                ErrorTextBlock.Text = "";
            }
            catch (Exception)
            {
               
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (IsInitializing) return;
            try
            {
                savedata.Token = TokenTextEdit.Text;
                savedata.Save();
                context = GetContext(savedata.Token);
                ErrorTextBlock.Text = "";
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Неправильный токен";
            }
        }

        Context GetContext(string token)
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
            if ((endDate - startDate).TotalDays <=7) interval = CandleInterval.Hour;
                else  if ((endDate - startDate).TotalDays <= 30) interval = CandleInterval.Day;
                        else  interval = CandleInterval.Week;
            try
            {
                ErrorTextBlock.Text = "Идет загрузка";
                decimal priceLimit = savedata.MoneyLimitValue;
                MarketInstrumentList markertlist = await context.MarketStocksAsync();
                Dictionary<string, decimal> dif = new Dictionary<string, decimal>();
                foreach (var instrument in markertlist.Instruments.Where((i) => i.Currency == curr))
                {
                   
                    try
                    {
                        List<CandlePayload> candles= ( await context.MarketCandlesAsync(instrument.Figi, 
                            DateTime.SpecifyKind( startDate,DateTimeKind.Local), DateTime.SpecifyKind(endDate,DateTimeKind.Local), 
                            interval)).Candles;
                       // List<CandlePayload> candles= ( await context.MarketCandlesAsync(instrument.Figi, DateTime.Now.AddDays(-2), DateTime.Now, CandleInterval.Hour)).Candles;
                        if (candles.Count > 0 && candles.Last().Close < priceLimit)
                            dif[instrument.Name] = Math.Round((candles[0].Open - candles.Last().Close) / candles.Last().Close * 100,2)*-1;
                        candles= (await context.MarketCandlesAsync(instrument.Figi, DateTime.Today, DateTime.Today.AddDays(0.99),
                            CandleInterval.Minute)).Candles;
                        File.WriteAllText("data.txt", $"{instrument.Name} - H={Hurst.CalculateHurst(candles)}");
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
                var listofDifs = dif.ToList();
                listofDifs.Sort((el1, el2) => el2.Value.CompareTo(el1.Value));
                //foreach (var item in listofDifs)
               // {
                    DataDataGrid.ItemsSource= listofDifs;
                    //listListBox.Items.Add($"{item.Key}  {(item.Value * -1).ToString("n2")}%");
               // }
                   
                ErrorTextBlock.Text = "";
            }
            catch (Exception e)
            {
                ErrorTextBlock.Text=e.Message;
            }
            
        }

        [Serializable]
        class SaveData
        {
            public string Token { get; set; }
            public decimal MoneyLimitValue { get; set; }
            public Currency Currency { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public SaveData GetSaveData()
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
            public void Save()
            {
                using (FileStream fs = File.OpenWrite(saveFilePath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, this);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            if (context != null)
                await GetPriceChange(context, savedata.Currency,savedata.StartDate, savedata.EndDate);
        }

        private void RubRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            savedata.Currency = RubRadioButton?.IsChecked == true ? Currency.Rub : Currency.Usd;
            savedata.Save();
        }

        private void USDRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            savedata.Currency = RubRadioButton?.IsChecked == true ? Currency.Rub : Currency.Usd;
            savedata.Save();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitializing) return;
            savedata.StartDate = StartDate.SelectedDate.GetValueOrDefault();
            savedata.Save();

        }

        private void EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitializing) return;
            savedata.EndDate = ((DateTime)e.AddedItems[0]).AddDays(1) > DateTime.Now ? DateTime.Now : ((DateTime)e.AddedItems[0]).AddDays(1);
            savedata.Save();

        }
    }
}
