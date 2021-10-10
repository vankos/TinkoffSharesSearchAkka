using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Runtime.InteropServices;

namespace Tinkoff
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("a8896c23-0183-414b-8dd2-c1968c9b2e39"), ComVisible(true)]
    internal class TinkoffNotificationActivator: NotificationActivator
    {
        public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
        {
        }
    }
}
