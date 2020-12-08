using LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DynamicData;
using LogViewer.ViewModels;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for LogViewManagementView.xaml
    /// </summary>
    public partial class LogViewManagementView : UserControl
    {
        #region Properties
        private LogViewManagementViewModel _viewModel
        {
            get
            {
                return (LogViewManagementViewModel)DataContext;
            }
        }
        public SourceCache<LogEntry, string> LogEntriesSourceCache
        {
            get { return (SourceCache<LogEntry, string>)GetValue(LogEntriesSourceCacheProperty); }
            set { SetValue(LogEntriesSourceCacheProperty, value); }
        }
        public SourceCache<LogFile, string> LogFilesSourceCache
        {
            get { return (SourceCache<LogFile, string>)GetValue(LogFilesSourceCacheProperty); }
            set { SetValue(LogFilesSourceCacheProperty, value); }
        }
        public SourceCache<Database, string> DatabasesSourceCache
        {
            get { return (SourceCache<Database, string>)GetValue(DatabasesSourceCacheProperty); }
            set { SetValue(DatabasesSourceCacheProperty, value); }
        }
        #endregion

        #region Dependency properties
        private static readonly DependencyProperty LogEntriesSourceCacheProperty = DependencyProperty.Register("LogEntriesSourceCache", typeof(SourceCache<LogEntry, string>), typeof(LogViewManagementView));
        private static readonly DependencyProperty LogFilesSourceCacheProperty = DependencyProperty.Register("LogFilesSourceCache", typeof(SourceCache<LogFile, string>), typeof(LogViewManagementView));
        private static readonly DependencyProperty DatabasesSourceCacheProperty = DependencyProperty.Register("DatabasesSourceCache", typeof(SourceCache<Database, string>), typeof(LogViewManagementView));
        #endregion

        #region Constructor
        public LogViewManagementView()
        {
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void LogViewManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LogEntriesSourceCache = LogEntriesSourceCache;
            _viewModel.LogFilesSourceCache = LogFilesSourceCache;
            _viewModel.DatabasesSourceCache = DatabasesSourceCache;
            _viewModel.Initialize();
        }
        #endregion
    }
}
