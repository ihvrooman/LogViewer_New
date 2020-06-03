using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using DynamicData.Binding;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LogViewer.ViewModels
{
    public class LogViewManagementViewModel : BaseViewModel
    {
        #region Fields
        #region Commands
        private ICommand _openOpenLogFileViewCommand;
        private ICommand _openOpenDatabaseViewCommand;
        private ICommand _removeLogSourceFromLogViewCommand;
        #endregion
        #region Readonly observable collections
        private ReadOnlyObservableCollection<LogFile> _logFiles;
        private ReadOnlyObservableCollection<Database> _databases;
        #endregion
        private LogView _selectedLogView;
        private int _numUnfilteredErrors;
        #endregion

        #region Properties
        #region Commands
        public ICommand OpenOpenLogFileViewCommand
        {
            get
            {
                if (_openOpenLogFileViewCommand == null)
                {
                    _openOpenLogFileViewCommand = new RelayCommand(OpenOpenLogFileView);
                }
                return _openOpenLogFileViewCommand;
            }
        }
        public ICommand OpenOpenDatabaseViewCommand
        {
            get
            {
                if (_openOpenDatabaseViewCommand == null)
                {
                    _openOpenDatabaseViewCommand = new RelayCommand(OpenOpenDatabaseView);
                }
                return _openOpenDatabaseViewCommand;
            }
        }
        public ICommand RemoveLogSourceFromLogViewCommand
        {
            get
            {
                if (_removeLogSourceFromLogViewCommand == null)
                {
                    _removeLogSourceFromLogViewCommand = new RelayCommand<ILogEntriesSource>(RemoveLogSourceFromLogView);
                }
                return _removeLogSourceFromLogViewCommand;
            }
        }
        #endregion
        #region Source caches
        public SourceCache<LogEntry, string> LogEntriesSourceCache { get; set; }
        public SourceCache<LogFile, string> LogFilesSourceCache { get; set; }
        public SourceCache<Database, string> DatabasesSourceCache { get; set; }
        #endregion
        #region Readonly observable collections
        public ReadOnlyObservableCollection<LogFile> LogFiles { get { return _logFiles; } private set { _logFiles = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<Database> Databases { get { return _databases; } private set { _databases = value; RaisePropertyChangedEvent(); } }
        #endregion
        public int NumUnfilteredErrors { get { return _numUnfilteredErrors; } set { _numUnfilteredErrors = value; RaisePropertyChangedEvent(); } }
        #endregion

        #region Public methods
        public void Initialize()
        {
            Mediator.Register(MediatorMessages.SelectedLogViewChanged, SelectedLogViewChanged_1);
            Mediator.Register(MediatorMessages.AutoRefreshSettingChanged, UpdateLogSourceAutoRefreshPropertiesAsync);
            InitializeBindings();
        }
        #endregion

        #region Private methods
        private void InitializeBindings()
        {
            LogFilesSourceCache.Connect()
                    .AutoRefresh(logFile => logFile.IsOpen)
                    .AutoRefresh(logFile => logFile.RefreshIndicator)
                    .Sort(SortExpressionComparer<LogFile>.Descending(logFile => logFile.DateLastOpened))
                    .Filter(logFile => logFile.IsOpen && _selectedLogView != null && _selectedLogView.LogFilePaths.Contains(logFile.NetworkFile?.FullName))
                    .ObserveOnDispatcher()
                    .Bind(out _logFiles)
                    .DisposeMany()
                    .Subscribe();

            DatabasesSourceCache.Connect()
                    .AutoRefresh(database => database.IsOpen)
                    .AutoRefresh(database => database.RefreshIndicator)
                    .Sort(SortExpressionComparer<Database>.Descending(database => database.DateLastOpened))
                    .Filter(database => database.IsOpen && _selectedLogView != null && _selectedLogView.DatabaseNames.Contains(database.Name))
                    .ObserveOnDispatcher()
                    .Bind(out _databases)
                    .DisposeMany()
                    .Subscribe();

            //FileSizeText = fileWatcher.Latest.Select(fn => fn.Size)
            //    .Select(size => size.FormatWithAbbreviation())
            //    .DistinctUntilChanged()
            //    .ForBinding();

            //NumUnfilteredErrors = LogEntriesSourceCache.Connect().ToListObservable()
            //    .DistinctUntilChanged()
            //    .ForBinding();

            //LogEntriesSourceCache.Items
            //    .Where(l => (_selectedLogView.LogFilePaths.Contains(l.Identifier) || _selectedLogView.DatabaseNames.Contains(l.Identifier)) && l.Type == LogMessageType.Error)
            //    .Subscribe(
            //    NumUnfilteredErrors = 5,
            //    ex =>
            //    {
            //        NumUnfilteredErrors = 0;
            //    },
            //    () => { NumUnfilteredErrors = 0; });
        }

        private void OpenOpenLogFileView()
        {
            Mediator.NotifyColleagues(MediatorMessages.RequestOpenOpenLogFileView, _selectedLogView);
            Mediator.NotifyColleagues(MediatorMessages.RequestToggleLaunchViewIsOpen, LaunchViewDisplayOptions.ToolbarWidth);
        }

        private void RemoveLogSourceFromLogView(ILogEntriesSource logEntriesSource)
        {
            if (logEntriesSource is LogFile logFile)
            {
                _selectedLogView.LogFilePaths.Remove(logFile.NetworkFile.FullName);
                LogFilesSourceCache.Refresh(logFile);
            }
            else if (logEntriesSource is Database database)
            {
                _selectedLogView.DatabaseNames.Remove(database.Name);
                DatabasesSourceCache.Refresh(database);
            }
            Mediator.NotifyColleagues(MediatorMessages.LogSourceRemovedFromSelectedLogView, logEntriesSource);
        }

        private void OpenOpenDatabaseView()
        {
            Mediator.NotifyColleagues(MediatorMessages.RequestOpenOpenDatabaseView, _selectedLogView);
            Mediator.NotifyColleagues(MediatorMessages.RequestToggleLaunchViewIsOpen, LaunchViewDisplayOptions.ToolbarWidth);
        }

        private void SelectedLogViewChanged_1(object obj)
        {
            var e = (SelectedLogViewChangedEventArgs)obj;
            _selectedLogView = e.NewLogView;
            RefreshLogFilesSourceCacheAsync(e.OldLogView, e.NewLogView);
            RefreshDatabasesSourceCacheAsync(e.OldLogView, e.NewLogView);
        }

        private void RefreshLogFilesSourceCacheAsync(LogView oldLogView, LogView newLogView)
        {
            Task.Run(() =>
            {
                var logFilesToRefresh = new List<LogFile>();
                if (oldLogView != null)
                {
                    foreach (var identifier in oldLogView.AllLogFilePathsAndDatabaseNames)
                    {
                        var logFile = LogFilesSourceCache.Lookup(identifier);
                        if (logFile.HasValue)
                        {
                            logFilesToRefresh.Add(logFile.Value);
                        }
                    }
                }
                if (newLogView != null)
                {
                    foreach (var identifier in newLogView.AllLogFilePathsAndDatabaseNames)
                    {
                        var logFile = LogFilesSourceCache.Lookup(identifier);
                        if (logFile.HasValue)
                        {
                            logFilesToRefresh.Add(logFile.Value);
                        }
                    }
                }
                LogFilesSourceCache.Refresh(logFilesToRefresh);
                RaisePropertyChangedEvent("LogFiles");
            });
        }

        private void RefreshDatabasesSourceCacheAsync(LogView oldLogView, LogView newLogView)
        {
            Task.Run(() =>
            {
                var databasesToRefresh = new List<Database>();
                if (oldLogView != null)
                {
                    foreach (var identifier in oldLogView.AllLogFilePathsAndDatabaseNames)
                    {
                        var database = DatabasesSourceCache.Lookup(identifier);
                        if (database.HasValue)
                        {
                            databasesToRefresh.Add(database.Value);
                        }
                    }
                }
                if (newLogView != null)
                {
                    foreach (var identifier in newLogView.AllLogFilePathsAndDatabaseNames)
                    {
                        var database = DatabasesSourceCache.Lookup(identifier);
                        if (database.HasValue)
                        {
                            databasesToRefresh.Add(database.Value);
                        }
                    }
                }
                DatabasesSourceCache.Refresh(databasesToRefresh);
                RaisePropertyChangedEvent("Databases");
            });
        }

        private void UpdateLogSourceAutoRefreshPropertiesAsync(object obj)
        {
            Task.Run(() =>
            {
                if (obj is bool autoRefresh)
                {
                    foreach (var logFile in LogFiles)
                    {
                        logFile.AutoRefresh = autoRefresh;
                    }

                    foreach (var database in Databases)
                    {
                        database.AutoRefresh = autoRefresh;
                    }
                }
            });
        }
        #endregion
    }
}
