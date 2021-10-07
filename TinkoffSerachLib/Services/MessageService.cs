using System;

namespace TinkoffSearchLib.Services
{
    public static class MessageService
    {
        public static void SendMessage(string message, bool isNotification)
        {
            OnMessageRecived?.Invoke(null, message);
            if (isNotification)
                OnNotificationMessageRecived?.Invoke(null, message);
        }

        public static event EventHandler<string> OnMessageRecived;
        public static event EventHandler<string> OnNotificationMessageRecived;
    }
}
