using System;
using TinkoffSerachLib.Models;
using TinkoffSerachLib.Services;

namespace WPFController
{
    public class WPFController
    {
        public UserData UserData { get; set; }

        public WPFController()
        {
            UserData =  SaveService.LoadData();
        }

        public void SaveUserData()
        {
            SaveService.SaveData(UserData);
        }
    }
}
