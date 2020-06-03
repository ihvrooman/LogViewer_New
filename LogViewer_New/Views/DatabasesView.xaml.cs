using DynamicData;
using LogViewer.Models;
using LogViewer.ViewModels;
using MahApps.Metro.Controls.Dialogs;
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

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for DatabasesView.xaml
    /// </summary>
    public partial class DatabasesView : UserControl
    {
        #region Properties
        private DatabasesViewModel _viewModel
        {
            get
            {
                return (DatabasesViewModel)DataContext;
            }
        }
        public ReadOnlyObservableCollection<Database> Databases
        {
            get { return (ReadOnlyObservableCollection<Database>)GetValue(DatabasesProperty); }
            set { SetValue(DatabasesProperty, value); }
        }
        public SourceCache<Database, string> DatabasesSourceCache
        {
            get { return (SourceCache<Database, string>)GetValue(DatabasesSourceCacheProperty); }
            set { SetValue(DatabasesSourceCacheProperty, value); }
        }
        #endregion

        #region Dependency properties
        private static readonly DependencyProperty DatabasesProperty = DependencyProperty.Register("Databases", typeof(ReadOnlyObservableCollection<Database>), typeof(DatabasesView));
        private static readonly DependencyProperty DatabasesSourceCacheProperty = DependencyProperty.Register("DatabasesSourceCache", typeof(SourceCache<Database, string>), typeof(DatabasesView));
        #endregion

        #region Constructor
        public DatabasesView()
        {
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void DatabasesView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DialogCoordinator = DialogCoordinator.Instance;
            _viewModel.Databases = Databases;
            _viewModel.DatabasesSourceCache = DatabasesSourceCache;
        }
        #endregion
    }
}
