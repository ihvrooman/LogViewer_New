using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.Services;
using LogViewer.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields
        private volatile bool _initialized;
        #endregion

        #region Properties
        private MainWindowViewModel _viewModel
        {
            get
            {
                return (MainWindowViewModel)DataContext;
            }
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            //TODO: Upgrade user settings to be compatible with new version.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            InitializeComponent();
            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);
            Mediator.Register(MediatorMessages.RequestShowLogViewDetailsDialog, OpenLogViewDetailsAsync);
        }
        #endregion

        #region Private methods
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var errorMessage = $"An unhandled exception occurred. Error message: {ex.Message} Exception stack trace: {ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner exception message: {ex.InnerException.Message} Inner exception stack trace: {ex.InnerException.StackTrace}";
            }
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(errorMessage, LogMessageType.Error);

            //Wait for message queue to be flushed
            Task.Delay(31).Wait();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _viewModel.LogViewManagementColumnWidth = new GridLength(SettingsService.GetLogViewManagementColumnWidth(), GridUnitType.Pixel);
            _viewModel.LogEntryMessageViewRowHeight = new GridLength(SettingsService.GetLogEntryMessageViewRowHeight(), GridUnitType.Pixel);
            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_viewModel.UnsavedLogViews.Count > 0)
            {
                e.Cancel = true;
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Activate();
                await Task.Delay(50);
                if (await _viewModel.SaveAllUnsavedLogViews())
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                //TODO: Implement single instance functionality.
                _viewModel.SaveLayout();
                SaveWindowPlacement();
                _initialized = false;
                Hide();
                //TODO: Uncomment shutdown routine.
                //Routines.Shutdown(AppInfo.BaseAppInfo);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _initialized = true;
        }

        private void SaveWindowPlacement()
        {
            try
            {
                if (_initialized)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Saving main window placement.", LogMessageType.Verbose);
                    Settings.Default.MainWindowPlacement = this.GetPlacement();
                    SettingsService.SaveSettings();
                }
            }
            catch (Exception ex)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to save main window placement. Error message: {ex.Message}", LogMessageType.Warning);
            }
        }

        private async void OpenLogViewDetailsAsync(object args)
        {
            if (args is LogViewDetails logViewDetails)
            {
                Mediator.NotifyColleagues(MediatorMessages.UpdateIgnoreToggleLaunchViewIsOpenRequests, true);
                var openLogView = await this.ShowChildWindowAsync<bool>(new LogViewDetailsDialog(logViewDetails), ChildWindowManager.OverlayFillBehavior.FullWindow);
                Mediator.NotifyColleagues(MediatorMessages.UpdateIgnoreToggleLaunchViewIsOpenRequests, false);

                if (openLogView)
                {
                    Mediator.NotifyColleagues(MediatorMessages.RequestOpenLogView, logViewDetails.LogView);
                }
            }
        }
        #endregion
    }
}
