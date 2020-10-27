using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows;

namespace Tinkoff
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<TinkoffNotificationActivator>("Vano.TinkoffApp");
        }
    }
}
