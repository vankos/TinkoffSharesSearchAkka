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


        public WPFController()
        {
            UserData =  SaveService.LoadData();
            
        }

        public void SaveUserData()
        {
            SaveService.SaveData(UserData);
        }

        async public Task<List<Security>> GetData()
        {
             MessageService.SendMessage("", false);
            if (UserData.Token == null && getDataService is null)
            {
                MessageService.SendMessage("Нет токена", false);
                throw new ApplicationException("Нет токена");
            }
            try
            {
                if (getDataService is null)
                    getDataService = new GetDataService(UserData.Token);
                return await getDataService.GetCandlesForAllSharesOnDate(UserData.StartDate, UserData.EndDate, UserData.Currency);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
