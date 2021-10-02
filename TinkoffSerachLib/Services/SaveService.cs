using System;
using System.IO;
using System.Text.Json;
using TinkoffSerachLib.Models;

namespace TinkoffSerachLib.Services
{
    static public class SaveService
    {
        public const string SAVEFILEPATH = "savedData.dat";

        static public void SaveData(UserData userData, string filelocation = SAVEFILEPATH)
        {
           File.WriteAllText(filelocation,JsonSerializer.Serialize<UserData>(userData));
        }

        public static UserData LoadData(string filelocation = SAVEFILEPATH)
        {
            try
            {
                    return JsonSerializer.Deserialize<UserData>(File.ReadAllText(filelocation));
            }
            catch (Exception)
            {
                return new UserData();
            }
        }
    }
}
