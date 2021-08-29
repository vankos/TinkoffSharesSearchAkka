using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkoffSerachLib.Models;
using TinkoffSerachLib.Services;

namespace WPFController
{
    public class WPFController
    {
        public UserData UserData { get; set; }
        public List<Security> UnflteredData = new List<Security>();
        private GetDataService getDataService;
        public  event EventHandler<string> OnMessageRecived;
        public  event EventHandler<string> OnNotificationMessageRecived;


        public WPFController()
        {
            UserData =  SaveService.LoadData();
            MessageService.OnMessageRecived += (o, e) => OnMessageRecived?.Invoke(o, e);
            MessageService.OnNotificationMessageRecived += (o, e) => OnNotificationMessageRecived?.Invoke(o, e);
        }

        public void SaveUserData()
        {
            SaveService.SaveData(UserData);
        }

        async public Task<List<Security>> GetData()
        {
            if (UserData.Token == null && getDataService is null)
            {
                MessageService.SendMessage("Нет токена", false);
                throw new ApplicationException("Нет токена");
            }
            try
            {
                if (getDataService is null)
                    getDataService = new GetDataService(UserData.Token);
                UnflteredData = await getDataService.GetCandlesForAllSharesOnDate(UserData.StartDate, UserData.EndDate, UserData.Currency);
                return UnflteredData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        async public Task<List<Security>> GetAndFilterData()
        {
            var data = await GetData();
            AnalyticalService.GetGrowth(data);
            AnalyticalService.GetLinearity(data);
            return data;
        }
    }
}
