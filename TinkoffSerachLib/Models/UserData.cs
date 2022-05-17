using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TinkoffSearchLib.Models
{
    [Serializable]
    public class UserData : INotifyPropertyChanged
    {
        private decimal moneyLimitValue;
        private decimal linearity;
        private bool showNew;
        private DateTime startDate;
        private DateTime endDate;
        private List<Security> securities;

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartDate)));
            }
        }
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDate)));
            }
        }
        public string Token { get; set; }
        public decimal MoneyLimit
        {
            get { return moneyLimitValue; }
            set
            {
                moneyLimitValue = value;
                OnFiltersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public decimal Linearity
        {
            get { return linearity; }
            set
            {
                linearity = value;
                OnFiltersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowNew
        {
            get { return showNew; }
            set
            {
                showNew = value;
                OnFiltersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [JsonIgnore]
        public List<Security> Securities
        {
            get => securities;
            set
            {
                securities = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Securities)));
            }
        }

        public bool IsUSD { get; set; }
        public bool IsRUR { get; set; }
        public bool IsShares { get; set; }
        public bool IsETF { get; set; }

        public event EventHandler OnFiltersChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
