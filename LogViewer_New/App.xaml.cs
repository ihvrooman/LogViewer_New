using LogViewer.Helpers;
using System;
using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Shell;
using LogViewer.Properties;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        [STAThread]
        public static void Main()
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Determining if another instance of {AppInfo.BaseAppInfo.AppName} is running.");
            if (SingleInstance<App>.InitializeAsFirstInstance("LogViewer_sag65s4dfjkdlsjk45jk34klj23j4jk53456fy3u"))
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Determined that no other application instances are running. Continuing execution.");
                new SplashScreen("SplashScreen1.png").Show(true);

                //Upgrade user settings
                try
                {
                    if (Settings.Default.UpgradeSettings)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Upgrading user settings.");
                        Settings.Default.Upgrade();
                        Settings.Default.UpgradeSettings = false;
                        Settings.Default.Save();
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"User settings successfully upgraded.");
                    }
                }
                catch (Exception ex)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to upgrade user settings. Error message: {ex.Message}", LogMessageType.Error);
                }

                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
            else
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Determined that another application instance is running. Forwarding command parameters to existing instance and terminating.");
                AppInfo.BaseAppInfo.Log.Dispose();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            try
            {
                Current.Dispatcher.Invoke(() =>
                {
                    WindowHelper.ActivateMainWindow();
                });
            }
            catch
            {
                //Fail silently
            }
            return true;
        }
    }
}
