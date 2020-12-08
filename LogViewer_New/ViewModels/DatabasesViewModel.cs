using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LogViewer.ViewModels
{
    public class DatabasesViewModel : BaseViewModel
    {
        #region Fields
        private int _selectedIndex;
        private ICommand _addDatabaseCommand;
        private ICommand _removeDatabaseCommand;
        private ReadOnlyObservableCollection<Database> _databases;
        #endregion

        #region Properties
        public IDialogCoordinator DialogCoordinator { get; set; }
        public ReadOnlyObservableCollection<Database> Databases { get { return _databases; } set { _databases = value; RaisePropertyChangedEvent(); } }
        public int SelectedIndex { get { return _selectedIndex; } set { _selectedIndex = value; RaisePropertyChangedEvent(); } }
        public string SelectedDatabase
        {
            get
            {
                try
                {
                    return Settings.Default.XmlDatabases != null ? Settings.Default.XmlDatabases[SelectedIndex] : string.Empty;
                }
                catch (Exception ex)
                {
                    return $"Error-{ex.Message}";
                }
            }
        }
        public ICommand AddDatabaseCommand
        {
            get
            {
                if (_addDatabaseCommand == null)
                {
                    _addDatabaseCommand = new RelayCommand(AddDatabase);
                }
                return _addDatabaseCommand;
            }
        }
        public ICommand RemoveDatabaseCommand
        {
            get
            {
                if (_removeDatabaseCommand == null)
                {
                    _removeDatabaseCommand = new RelayCommand(RemoveDatabase, CanRemoveDatabase);
                }
                return _removeDatabaseCommand;
            }
        }
        public SourceCache<Database, string> DatabasesSourceCache { get; set; }
        #endregion

        #region Private methods
        private async void AddDatabase()
        {
            await DatabaseService.AddDatabaseRoutine(DialogCoordinator, this, DatabasesSourceCache);
        }

        private bool CanRemoveDatabase()
        {
            return Databases?.Count > 0 && SelectedIndex < Databases?.Count;
        }

        private async void RemoveDatabase()
        {
            await DatabaseService.RemoveDatabaseRoutine(DialogCoordinator, this, DatabasesSourceCache, Databases[SelectedIndex]);
            SelectedIndex = 0;
        }
        #endregion
    }
}
