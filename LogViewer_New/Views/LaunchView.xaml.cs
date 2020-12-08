using DynamicData;
using LogViewer.Models;
using LogViewer.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
    /// Interaction logic for LaunchView.xaml
    /// </summary>
    public partial class LaunchView : UserControl
    {
        #region Properties
        private LaunchViewModel _viewModel
        {
            get
            {
                return (LaunchViewModel)DataContext;
            }
        }
        public SourceCache<LogView, string> LogViewsSourceCache
        {
            get { return (SourceCache<LogView, string>)GetValue(LogViewsSourceCacheProperty); }
            set { SetValue(LogViewsSourceCacheProperty, value); }
        }
        public SourceCache<Database, string> DatabasesSourceCache
        {
            get { return (SourceCache<Database, string>)GetValue(DatabasesSourceCacheProperty); }
            set { SetValue(DatabasesSourceCacheProperty, value); }
        }
        public SourceCache<LogFile, string> LogFilesSourceCache
        {
            get { return (SourceCache<LogFile, string>)GetValue(LogFilesSourceCacheProperty); }
            set { SetValue(LogFilesSourceCacheProperty, value); }
        }
        public SourceCache<LogEntry, string> LogEntriesSourceCache
        {
            get { return (SourceCache<LogEntry, string>)GetValue(LogEntriesSourceCacheProperty); }
            set { SetValue(LogEntriesSourceCacheProperty, value); }
        }
        #endregion

        #region Dependency properties
        private static readonly DependencyProperty LogViewsSourceCacheProperty = DependencyProperty.Register("LogViewsSourceCache", typeof(SourceCache<LogView, string>), typeof(LaunchView));
        private static readonly DependencyProperty DatabasesSourceCacheProperty = DependencyProperty.Register("DatabasesSourceCache", typeof(SourceCache<Database, string>), typeof(LaunchView));
        private static readonly DependencyProperty LogFilesSourceCacheProperty = DependencyProperty.Register("LogFilesSourceCache", typeof(SourceCache<LogFile, string>), typeof(LaunchView));
        private static readonly DependencyProperty LogEntriesSourceCacheProperty = DependencyProperty.Register("LogEntriesSourceCache", typeof(SourceCache<LogEntry, string>), typeof(LaunchView));
        #endregion

        #region Constructor
        public LaunchView()
        {
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DialogCoordinator = DialogCoordinator.Instance;
            _viewModel.LogViewsSourceCache = LogViewsSourceCache;
            _viewModel.LogFilesSourceCache = LogFilesSourceCache;
            _viewModel.DatabasesSourceCache = DatabasesSourceCache;
            _viewModel.LogEntriesSourceCache = LogEntriesSourceCache;
            _viewModel.Initialize();
        }

        private async void BrowseButton_Clicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.OpenLogFilePath(openFileDialog.FileName);
            }
            var a = (TabItem)e.Source;
            var b = (TabControl)a.Parent;
            await Task.Delay(5);
            b.SelectedIndex = 0;
        }
        #endregion
    }
}
