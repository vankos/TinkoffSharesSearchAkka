using System.Windows;

namespace WPFCoreUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Возникла неоработанная ошибка:\n{e.Exception.Message}", "Необработанная ошибка", MessageBoxButton.OK);
            e.Handled = true;
        }
    }
}
