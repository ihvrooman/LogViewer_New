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

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for FilterView.xaml
    /// </summary>
    public partial class FilterView : UserControl
    {
        #region Properties
        public FilterOptions FilterOptions
        {
            get { return (FilterOptions)GetValue(FilterOptionsProperty); }
            set { SetValue(FilterOptionsProperty, value); }
        }
        public ReadOnlyObservableCollection<string> AvailableUsernames
        {
            get { return (ReadOnlyObservableCollection<string>)GetValue(AvailableUsernamesProperty); }
            set { SetValue(AvailableUsernamesProperty, value); }
        }
        public ReadOnlyObservableCollection<string> AvailableComputernames
        {
            get { return (ReadOnlyObservableCollection<string>)GetValue(AvailableComputernamesProperty); }
            set { SetValue(AvailableComputernamesProperty, value); }
        }
        #endregion

        #region Dependency properties
        private static readonly DependencyProperty FilterOptionsProperty = DependencyProperty.Register("FilterOptions", typeof(FilterOptions), typeof(FilterView), new FrameworkPropertyMetadata(new FilterOptions()));
        private static readonly DependencyProperty AvailableUsernamesProperty = DependencyProperty.Register("AvailableUsernames", typeof(ReadOnlyObservableCollection<string>), typeof(FilterView));
        private static readonly DependencyProperty AvailableComputernamesProperty = DependencyProperty.Register("AvailableComputernames", typeof(ReadOnlyObservableCollection<string>), typeof(FilterView));
        #endregion

        #region Constructor
        public FilterView()
        {
            InitializeComponent();
        }
        #endregion
    }
}
