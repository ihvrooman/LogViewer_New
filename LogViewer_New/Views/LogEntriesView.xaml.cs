using DynamicData;
using LogViewer.Models;
using LogViewer.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for LogViewView.xaml
    /// </summary>
    public partial class LogEntriesView : UserControl
    {
        #region Properties
        private LogEntriesViewModel _viewModel
        {
            get
            {
                return (LogEntriesViewModel)DataContext;
            }
        }
        public SourceCache<LogEntry, string> LogEntriesSourceCache
        {
            get { return (SourceCache<LogEntry, string>)GetValue(LogEntriesSourceCacheProperty); }
            set { SetValue(LogEntriesSourceCacheProperty, value); }
        }
        #endregion

        #region Dependency properties
        private static readonly DependencyProperty LogEntriesSourceCacheProperty = DependencyProperty.Register("LogEntriesSourceCache", typeof(SourceCache<LogEntry, string>), typeof(LogEntriesView));
        #endregion

        #region Constructor
        public LogEntriesView()
        {
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void LogEntriesView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LogEntriesSourceCache = LogEntriesSourceCache;
            _viewModel.DialogCoordinator = DialogCoordinator.Instance;
            _viewModel.Initialize();
        }
        #endregion

        //TODO: When log entries are added, the datagrid sorting isn't updating.
    }
}
