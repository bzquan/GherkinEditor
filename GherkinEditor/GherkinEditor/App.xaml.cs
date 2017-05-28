using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Practices.Unity;
using System.Windows.Markup;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gherkin.Util;

namespace Gherkin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string m_FeatureFilePath;

        IUnityContainer m_Container;
        bool IsStartingUp { get; set; } = true;

        public App(string featureFilePath)
        {
            m_FeatureFilePath = featureFilePath;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (IsAlreadyRunning())
            {
                Application.Current.Shutdown(0);
                return;
            }

            SplashScreen splashScreen = new SplashScreen("View/Images/BDDSplash.jpg");
            splashScreen.Show(true);

            // WPF has an interesting bug (or "feature" as you want it):
            // YOU CAN`T SHOW ANY MESSAGE BOXES UNTIL YOU OPEN AT LEAST ONE WPF WINDOW.
            // As a workaround we need to open one hidden window at the beginning of application startup.
            // And if our application starts successfully, than we close this "helper" window.
            // WPF Bug Workaround: while we have no WPF window open we can`t show MessageBox.
            Window dummyWindow = SetupDummyWindow();

            try
            {
                m_Container = new UnityContainer();
                ContainerBootstrapper.Resiger(m_Container);

                ConfigLocalizations();

                var mainWindow = m_Container.Resolve<View.MainWindow>();

                Application.Current.MainWindow = mainWindow;
                Application.Current.MainWindow.Show();
                mainWindow.OpenFeatureFile = m_FeatureFilePath;

                TeardownDummyWindow(dummyWindow);
            }
            catch (Exception ex)
            {
                ShowExceptionMessages(ex);
                dummyWindow.Close();
                Shutdown(1);
            }
        }

        private bool IsAlreadyRunning()
        {
            AppSettings setting = new AppSettings();
            if (setting.IsAllowRunningMultiApps ||
                Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() <= 1)
            {
                return false;
            }

            return ProcessInterop.SendFilePathToApp(m_FeatureFilePath);
        }

        private void ConfigLocalizations()
        {
            var appSetting = m_Container.Resolve<Util.IAppSettings>();
            Util.Util.SetLanguage(appSetting.Language);
            Util.EnumUtil.CurrentLanguage = appSetting.Language;
        }

        Window DummyWindow => new Window()
        {
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            WindowStyle = WindowStyle.None,
            Top = 0,
            Left = 0,
            Width = 1,
            Height = 1,
            ShowInTaskbar = false
        };

        Window SetupDummyWindow()
        {
            Window dummyWindow = DummyWindow;
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            dummyWindow.Show();
            return dummyWindow;
        }

        void TeardownDummyWindow(Window dummyWindow)
        {
            IsStartingUp = false;
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            dummyWindow.Close();
        }

        void ShowExceptionMessages(Exception ex)
        {
            string error_msg = String.Format
                ("{0} Error:  {1}\r\n\r\n{2}",
                   ex.Source, ex.Message, ex.StackTrace);
            MessageBox.Show(error_msg, "UnhandledException", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                if (!IsStartingUp) ShowExceptionMessages(e.Exception);
                e.Handled = true;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }

        /// <summary>
        /// Application Entry Point.
        /// 
        /// To add Main function in App class
        /// 1.Right-click App.xaml in the solution explorer
        /// 2.Select Properties Change 'Build Action' to 'Page'
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Gherkin.App app = new Gherkin.App(ExtractFilePath(args));

            app.InitializeComponent();
            app.Run();
        }

        private static string ExtractFilePath(string[] args)
        {
            if ((args.Length > 0) && File.Exists(args[0]))
                return args[0];
            else
                return null;
        }
    }
}
