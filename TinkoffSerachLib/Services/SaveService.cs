using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffSerachLib.Models;

namespace TinkoffSerachLib.Services
{
    static public class SaveService
    {
        public const string SAVEFILEPATH = "savedData.dat";

        static public void SaveData(UserData userData)
        {
            
        }

        public static UserData LoadData(string filelocation = SAVEFILEPATH)
        { 
        }
    }
}
