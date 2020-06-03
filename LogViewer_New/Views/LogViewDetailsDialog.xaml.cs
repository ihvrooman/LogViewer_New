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
using System.Windows.Shapes;
using MahApps.Metro.SimpleChildWindow;
using LogViewer.Models;
using AppStandards.MVVM;
using LogViewer.Helpers;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for LogViewDetailsDialog.xaml
    /// </summary>
    public partial class LogViewDetailsDialog : ChildWindow
    {
        #region Properties
        public LogViewDetails LogViewDetails { get; set; }
        #endregion

        #region Constructor
        public LogViewDetailsDialog(LogViewDetails logViewDetails)
        {
            LogViewDetails = logViewDetails;
            DataContext = LogViewDetails;
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void OpenLogViewButton_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
        #endregion
    }
}
