using Akka.Actor;
using System;
using System.IO;
using System.Text.Json;
using TinkoffSearchLib.Messages;
using TinkoffSearchLib.Models;

namespace TinkoffSearchLib.Services
{
    public class SaveService:ReceiveActor
    {
        public SaveService()
        {
            Receive<SaveUserDataMessage>(msg => 
            {
                SaveData(msg.UserData, msg.FilePath);
                Sender.Tell(SimpleMessages.Saved);
            });

            Receive<LoadUserDataMessage>(msg =>Sender.Tell(LoadData(msg.FilePath)));
        }

        private static void SaveData(UserData userData, string filelocation)
        {
           File.WriteAllText(filelocation,JsonSerializer.Serialize<UserData>(userData));
        }

        private static UserData LoadData(string filelocation)
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

    #region Messages
    public class SaveUserDataMessage
    {
        public SaveUserDataMessage(UserData userData, string fileName)
        {
            UserData = userData;
            FilePath = fileName;
        }
        public UserData UserData { get; set; }
        public string FilePath { get; set; }
    }

    public class LoadUserDataMessage
    {
        public LoadUserDataMessage(string fileName)
        {
            FilePath = fileName;
        }
        public string FilePath { get; set; }
    }

    #endregion
}
