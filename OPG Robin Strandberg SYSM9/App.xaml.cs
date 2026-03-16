using System.Windows;
using System.Windows.Threading;
using OPG_Robin_Strandberg_SYSM9.Data;
using OPG_Robin_Strandberg_SYSM9.Managers;
using OPG_Robin_Strandberg_SYSM9.Views;

namespace OPG_Robin_Strandberg_SYSM9
{
    public partial class App : Application
    {
        public static CookMasterDbContext DbContext { get; private set; }
        public static UserManager UserManager { get; private set; }

        // All resources are loaded in the OnStartup method after the app class is compiled,
        // using global resources defined in app.xaml. Runs before any window is opened.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load global theme resource with absolute pack URI
            var theme = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/OPG%20Robin%20Strandberg%20SYSM9;component/Themes/GlobalStyles.xaml",
                    UriKind.Absolute)
            };

            Application.Current.Resources.MergedDictionaries.Add(theme);

            // Initiera databas — skapar cookmaster.db i %LocalAppData%\CookMaster om den inte finns
            DbContext = new CookMasterDbContext();
            DbContext.Database.EnsureCreated();

            UserManager ??= new UserManager(DbContext);

            MainWindow = new Views.MainWindow();
            MainWindow.Closed += OnWindowClosed;
            MainWindow.Show();

            DispatcherUnhandledException +=
                App_DispatcherUnhandledException; // Global handler for exceptions on the UI thread
            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException; // Handler for exceptions on background threads
        }

        // If the user closes all windows, prompt them to reopen the app (app keeps running per ShutdownMode)
        private void OnWindowClosed(object sender, EventArgs e)
        {
            // Dispatcher checks open windows after the UI thread has updated the window list
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Current.Windows.Count == 0)
                {
                    var result = MessageBox.Show(
                        "All windows are closed. Do you want to reopen the app?",
                        "Application still running",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var main = new MainWindow();
                        main.Closed += OnWindowClosed;
                        main.Show();
                    }
                    else
                    {
                        Shutdown();
                    }
                }
            }), DispatcherPriority.Background);
        }


        protected override void OnExit(ExitEventArgs e)
        {
            DbContext?.SaveChanges();
            DbContext?.Dispose();
            base.OnExit(e);
        }

        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("An unexpected error occured with the application. The application will now close." +
                                "Please open the application again.");
            });
        }
    }
}
