using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TinkoffSerachLib.Models;

namespace TinkoffSerachLib.Services
{
    static public class SaveService
    {
        public const string SAVEFILEPATH = "savedData.dat";

        static public void SaveData(UserData userData, string filelocation = SAVEFILEPATH)
        {
            using (FileStream fs = File.OpenWrite(filelocation))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, userData);
            }
        }

        public static UserData LoadData(string filelocation = SAVEFILEPATH)
        {
            try
            {
                using (FileStream fs = File.OpenRead(filelocation))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    return (UserData)bf.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                return new UserData();
            }
        }
    }
}
