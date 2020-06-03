using AppStandards.MVVM;
using DynamicData;
using DynamicData.Binding;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.Services;
using LogViewer.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LogViewer.ViewModels
{
    public class LaunchViewModel : BaseViewModel
    {
        #region Fields
        #region ViewModels
        private List<BaseViewModel> _viewModels = new List<BaseViewModel>()
        {
            new HomeViewModel(),
            new OpenLogViewViewModel(),
            new OpenLogFileViewModel(),
            new OpenDatabaseViewModel(),
            new DatabasesViewModel(),
        };
        private BaseViewModel _selectedViewModel;
        #endregion
        #region Commands
        private ICommand _requestToggleLaunchViewIsOpenCommand;
        private ICommand _openOpenableObjectCommand;
        private ICommand _openLogViewDetailsCommand;
        private ICommand _renameLogViewCommand;
        private ICommand _toggleOpenableObjectIsPinnedCommand;
        private ICommand _deleteLogViewCommand;
        private ICommand _removeDatabaseCommand;
        private ICommand _openOpenLogViewViewCommand;
        private ICommand _openOpenLogFileViewCommand;
        private ICommand _openOpenDatabaseViewCommand;
        #endregion
        #region Readonly observable collections
        private ReadOnlyObservableCollection<LogView> _logViews;
        private ReadOnlyObservableCollection<LogView> _recentlyUsedLogViews;
        private ReadOnlyObservableCollection<LogView> _pinnedLogViews;

        private ReadOnlyObservableCollection<LogFile> _logFiles;
        private ReadOnlyObservableCollection<LogFile> _recentlyUsedLogFiles;
        private ReadOnlyObservableCollection<LogFile> _pinnedLogFiles;

        private ReadOnlyObservableCollection<Database> _databases;
        private ReadOnlyObservableCollection<Database> _recentlyUsedDatabases;
        private ReadOnlyObservableCollection<Database> _pinnedDatabases;

        private ReadOnlyObservableCollection<OpenableObject> _searchableOpenableObjects;
        #endregion
        private int _selectedMenuItemIndex;
        private LogView _logViewToOpenDocumentIn;
        private const int _numberInRecents = 6;
        private int _openLogViewCount;
        private bool _canCloseLaunchView;
        private bool _showSearchPopup;
        private string _searchTerm;
        #endregion

        #region Properties
        #region Commands
        public ICommand RequestToggleLaunchViewIsOpenCommand
        {
            get
            {
                if (_requestToggleLaunchViewIsOpenCommand == null)
                {
                    _requestToggleLaunchViewIsOpenCommand = new RelayCommand(RequestToggleLaunchViewIsOpen);
                }
                return _requestToggleLaunchViewIsOpenCommand;
            }
        }
        public ICommand OpenOpenableObjectCommand
        {
            get
            {
                if (_openOpenableObjectCommand == null)
                {
                    _openOpenableObjectCommand = new RelayCommand<OpenableObject>(OpenOpenableObject);
                }
                return _openOpenableObjectCommand;
            }
        }
        public ICommand OpenLogViewDetailsCommand
        {
            get
            {
                if (_openLogViewDetailsCommand == null)
                {
                    _openLogViewDetailsCommand = new RelayCommand<LogView>(OpenLogViewDetails);
                }
                return _openLogViewDetailsCommand;
            }
        }
        public ICommand RenameLogViewCommand
        {
            get
            {
                if (_renameLogViewCommand == null)
                {
                    _renameLogViewCommand = new RelayCommand<LogView>(RenameLogView);
                }
                return _renameLogViewCommand;
            }
        }
        public ICommand ToggleOpenableObjectIsPinnedCommand
        {
            get
            {
                if (_toggleOpenableObjectIsPinnedCommand == null)
                {
                    _toggleOpenableObjectIsPinnedCommand = new RelayCommand<OpenableObject>(ToggleOpenableObjectIsPinned);
                }
                return _toggleOpenableObjectIsPinnedCommand;
            }
        }
        public ICommand DeleteLogViewCommand
        {
            get
            {
                if (_deleteLogViewCommand == null)
                {
                    _deleteLogViewCommand = new RelayCommand<LogView>(DeleteLogView);
                }
                return _deleteLogViewCommand;
            }
        }
        public ICommand RemoveDatabaseCommand
        {
            get
            {
                if (_removeDatabaseCommand == null)
                {
                    _removeDatabaseCommand = new RelayCommand<Database>(RemoveDatabase);
                }
                return _removeDatabaseCommand;
            }
        }
        public ICommand OpenOpenLogViewViewCommand
        {
            get
            {
                if (_openOpenLogViewViewCommand == null)
                {
                    _openOpenLogViewViewCommand = new RelayCommand<object>(OpenOpenLogViewView);
                }
                return _openOpenLogViewViewCommand;
            }
        }
        public ICommand OpenOpenLogFileViewCommand
        {
            get
            {
                if (_openOpenLogFileViewCommand == null)
                {
                    _openOpenLogFileViewCommand = new RelayCommand<object>(OpenOpenLogFileView);
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
                    _openOpenDatabaseViewCommand = new RelayCommand<object>(OpenOpenDatabaseView);
                }
                return _openOpenDatabaseViewCommand;
            }
        }
        #endregion
        #region Source caches
        public SourceCache<LogView, string> LogViewsSourceCache { get; set; }
        public SourceCache<LogFile, string> LogFilesSourceCache { get; set; }
        public SourceCache<Database, string> DatabasesSourceCache { get; set; }
        public SourceCache<LogEntry, string> LogEntriesSourceCache { get; set; }
        public SourceCache<OpenableObject, string> AllOpenableObjectsSourceCache { get; set; } = new SourceCache<OpenableObject, string>(o => o.Identifier);
        #endregion
        #region Readonly observable collections
        public ReadOnlyObservableCollection<LogView> LogViews { get { return _logViews; } private set { _logViews = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogView> RecentlyUsedLogViews { get { return _recentlyUsedLogViews; } private set { _recentlyUsedLogViews = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogView> PinnedLogViews { get { return _pinnedLogViews; } private set { _pinnedLogViews = value; RaisePropertyChangedEvent(); } }

        public ReadOnlyObservableCollection<LogFile> LogFiles { get { return _logFiles; } private set { _logFiles = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogFile> RecentlyUsedLogFiles { get { return _recentlyUsedLogFiles; } private set { _recentlyUsedLogFiles = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogFile> PinnedLogFiles { get { return _pinnedLogFiles; } private set { _pinnedLogFiles = value; RaisePropertyChangedEvent(); } }

        public ReadOnlyObservableCollection<Database> Databases { get { return _databases; } private set { _databases = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<Database> RecentlyUsedDatabases { get { return _recentlyUsedDatabases; } private set { _recentlyUsedDatabases = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<Database> PinnedDatabases { get { return _pinnedDatabases; } private set { _pinnedDatabases = value; RaisePropertyChangedEvent(); } }

        public ReadOnlyObservableCollection<OpenableObject> SearchableOpenableObjects { get { return _searchableOpenableObjects; } private set { _searchableOpenableObjects = value; RaisePropertyChangedEvent(); } }
        #endregion
        public IDialogCoordinator DialogCoordinator { get; set; }
        public BaseViewModel SelectedViewModel { get { return _selectedViewModel; } set { _selectedViewModel = value; RaisePropertyChangedEvent(); } }
        public int SelectedMenuItemIndex
        {
            get { return _selectedMenuItemIndex; }
            set
            {
                _selectedMenuItemIndex = value;
                RaisePropertyChangedEvent();
                if (SelectedMenuItemIndex > 0)
                {
                    SelectedViewModel = _viewModels[SelectedMenuItemIndex - 1];
                }
            }
        }
        public LogFile SelectedLogFile { get; set; }
        public LogView LogViewToOpenDocumentIn { get { return _logViewToOpenDocumentIn; } set { _logViewToOpenDocumentIn = value; RaisePropertyChangedEvent(); } }
        public bool CanCloseLaunchView { get { return _canCloseLaunchView; } set { _canCloseLaunchView = value; RaisePropertyChangedEvent(); Mediator.NotifyColleagues(MediatorMessages.CanCloseLaunchViewChanged, CanCloseLaunchView); } }
        public bool ShowSearchPopup { get { return _showSearchPopup; } set { _showSearchPopup = value; RaisePropertyChangedEvent(); } }
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                RaisePropertyChangedEvent();
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    RefreshSearchableOpenableObjects();
                }
                ShowSearchPopup = !string.IsNullOrEmpty(SearchTerm);
            }
        }
        #endregion

        #region Public methods
        public void Initialize()
        {
            Mediator.Register(MediatorMessages.RequestOpenHomeView, OpenHomeView);
            Mediator.Register(MediatorMessages.RequestOpenOpenLogViewView, OpenOpenLogViewView);
            Mediator.Register(MediatorMessages.RequestOpenOpenLogFileView, OpenOpenLogFileView);
            Mediator.Register(MediatorMessages.RequestOpenOpenDatabaseView, OpenOpenDatabaseView);
            Mediator.Register(MediatorMessages.AdjustOpenLogViewCount, AdjustOpenLogViewCount);
            Mediator.Register(MediatorMessages.RequestOpenLogView, ProcessOpenLogViewRequest);
            InitializeBindings();
            SelectedMenuItemIndex = 1;
        }

        public void OpenLogFilePath(string logFilePath)
        {
            var newLogFile = new LogFile(logFilePath);
            SettingsService.AddOrUpdateLogFile(newLogFile, true);
            LogFilesSourceCache.AddOrUpdate(newLogFile);
            SelectedLogFile = newLogFile;
            OpenSelectedLogFile();
        }
        #endregion

        #region Private methods
        private void InitializeBindings()
        {
            #region Log views
            var logViewsSharedSource = LogViewsSourceCache.Connect();
            logViewsSharedSource
                    .AutoRefresh(l => l.DateLastOpened)
                    .Sort(SortExpressionComparer<LogView>.Descending(l => l.DateLastOpened))
                    .ObserveOnDispatcher()
                    .Bind(out var logViews)
                    .Subscribe();
            logViewsSharedSource
                    .AutoRefresh(l => l.DateLastOpened)
                    .Sort(SortExpressionComparer<LogView>.Descending(l => l.DateLastOpened))
                    .Top(_numberInRecents)
                    .ObserveOnDispatcher()
                    .Bind(out var recentlyUsedLogViews)
                    .Subscribe();
            logViewsSharedSource
                    .AutoRefresh(l => l.DateLastOpened)
                    .AutoRefresh(l => l.IsPinned)
                    .Filter(l => l.IsPinned)
                    .Sort(SortExpressionComparer<LogView>.Descending(l => l.DateLastOpened))
                    .ObserveOnDispatcher()
                    .Bind(out var pinnedLogViews)
                    .Subscribe();

            LogViews = logViews;
            RecentlyUsedLogViews = recentlyUsedLogViews;
            PinnedLogViews = pinnedLogViews;
            #endregion

            #region Log files
            var logFilesSharedSource = LogFilesSourceCache.Connect();

            logFilesSharedSource
                    .AutoRefresh(l => l.DateLastOpened)
                    .Sort(SortExpressionComparer<LogFile>.Descending(l => l.DateLastOpened))
                    .ObserveOnDispatcher()
                    .Bind(out var allLogFiles)
                    .Subscribe();
            logFilesSharedSource
                    .AutoRefresh(l => l.DateLastOpened)
                    .Sort(SortExpressionComparer<LogFile>.Descending(l => l.DateLastOpened))
                    .Top(_numberInRecents)
                    .ObserveOnDispatcher()
                    .Bind(out var recentlyUsedLogFiles)
                    .Subscribe();
            logFilesSharedSource
                    .AutoRefresh(l => l.IsPinned)
                    .AutoRefresh(l => l.DateLastOpened)
                    .Filter(l => l.IsPinned)
                    .Sort(SortExpressionComparer<LogFile>.Descending(l => l.DateLastOpened))
                    .ObserveOnDispatcher()
                    .Bind(out var pinnedLogFiles)
                    .Subscribe();

            LogFiles = allLogFiles;
            RecentlyUsedLogFiles = recentlyUsedLogFiles;
            PinnedLogFiles = pinnedLogFiles;
            #endregion

            #region Databases
            var databasesSharedSource = DatabasesSourceCache.Connect();

            databasesSharedSource
                    .ObserveOnDispatcher()
                    .Bind(out var databases)
                    .Subscribe();
            databasesSharedSource
                    .AutoRefresh(d => d.DateLastOpened)
                    .Sort(SortExpressionComparer<Database>.Descending(d => d.DateLastOpened))
                    .Top(_numberInRecents)
                    .ObserveOnDispatcher()
                    .Bind(out var recentlyUsedDatabases)
                    .Subscribe();
            databasesSharedSource
                    .AutoRefresh(l => l.IsPinned)
                    .AutoRefresh(d => d.DateLastOpened)
                    .Filter(l => l.IsPinned)
                    .Sort(SortExpressionComparer<Database>.Descending(d => d.DateLastOpened))
                    .ObserveOnDispatcher()
                    .Bind(out var pinnedDatabases)
                    .Subscribe();

            Databases = databases;
            RecentlyUsedDatabases = recentlyUsedDatabases;
            PinnedDatabases = pinnedDatabases;
            #endregion

            #region Searchable openable objects
            //TODO: Right now if item added to source caches, this source cache won't see it. Maybe do a join?
            //TODO: Need to open search popup when user clicks on search box if there's text in the search box.
            AllOpenableObjectsSourceCache.AddOrUpdate(LogViewsSourceCache.Items);
            AllOpenableObjectsSourceCache.AddOrUpdate(LogFilesSourceCache.Items);
            AllOpenableObjectsSourceCache.AddOrUpdate(DatabasesSourceCache.Items);


            AllOpenableObjectsSourceCache.Connect()
                    .AutoRefresh(d => SearchTerm)
                    .Filter(o => string.IsNullOrEmpty(SearchTerm) || o.Identifier.ToLower().Contains(SearchTerm.ToLower()))
                    .ObserveOnDispatcher()
                    .Bind(out var searchableOpenableObjects)
                    .Subscribe();
            SearchableOpenableObjects = searchableOpenableObjects;
            #endregion
        }

        private void RefreshSearchableOpenableObjects()
        {
            AllOpenableObjectsSourceCache.Refresh(AllOpenableObjectsSourceCache.Items);
        }

        private void RequestToggleLaunchViewIsOpen()
        {
            LogViewToOpenDocumentIn = null;
            Mediator.NotifyColleagues(MediatorMessages.RequestToggleLaunchViewIsOpen, null);
        }

        private void OpenHomeView(object obj = null)
        {
            if (obj is LogView)
            {
                LogViewToOpenDocumentIn = (LogView)obj;
            }
            SelectedMenuItemIndex = 1;
        }

        private void OpenDatabasesView(object obj = null)
        {
            SelectedMenuItemIndex = 5;
        }

        private void OpenOpenableObject(OpenableObject obj)
        {
            ResetSearchbox();
            if (obj is LogView logView)
            {
                OpenLogView(logView);
            }
            else if (obj is LogFile logFile)
            {
                SelectedLogFile = logFile;
                OpenSelectedLogFile();
            }
            else if (obj is Database database)
            {
                OpenDatabase(database);
            }
        }

        private void ResetSearchbox()
        {
            SearchTerm = string.Empty;
            ShowSearchPopup = false;
        }

        private void ProcessOpenLogViewRequest(object obj)
        {
            if (obj is LogView logView)
            {
                OpenLogView(logView);
            }
        }

        private async void OpenLogView(LogView logView)
        {
            logView.Open();
            AdjustOpenLogViewCount(1);
            RequestSetSelectedLogView(logView);
            logView.IsLoading = true;
            RequestToggleLaunchViewIsOpen();

            await Task.Delay(500);
            if (!logView.IsNew && LogViewToOpenDocumentIn == null)
            {
                OpenableObjectService.SaveOpenableObject(logView);
            }

            var tasks = new List<Task<ServiceOperationResult>>();

            foreach (var logFilePath in logView.LogFilePaths)
            {
                var logFile = LogFilesSourceCache.Lookup(logFilePath);
                if (logFile.HasValue)
                {
                    logFile.Value.Open();
                    OpenableObjectService.SaveOpenableObject(logFile.Value);
                    tasks.Add(LogFileService.LoadLogEntriesIntoSourceCacheAsync(logFile.Value, LogEntriesSourceCache));
                }
            }

            foreach (var databaseName in logView.DatabaseNames)
            {
                var database = DatabasesSourceCache.Lookup(databaseName);
                if (database.HasValue)
                {
                    database.Value.Open();
                    OpenableObjectService.SaveOpenableObject(database.Value);
                    tasks.Add(DatabaseService.LoadLogEntriesIntoSourceCache(database.Value, LogEntriesSourceCache));
                }
            }

            var results = await Task.WhenAll(tasks);

            var failedLogFilesString = string.Empty;
            var failedDatabasesString = string.Empty;
            var errorMessage = string.Empty;
            await Task.Run(() =>
            {
                for (int i = 0; i < results.Length - logView.DatabaseNames.Count; i++)
                {
                    var result = results[i];
                    if (result.OperationFailed && logView.LogFilePaths.Count > i)
                    {
                        failedLogFilesString += logView.LogFilePaths[i] + "," + Environment.NewLine;
                    }
                }
                if (failedLogFilesString.Length > 2)
                {
                    failedLogFilesString = failedLogFilesString.Substring(0, failedLogFilesString.Length - 3);
                }

                for (int i = logView.LogFilePaths.Count; i < results.Length; i++)
                {
                    var result = results[i];
                    if (result.OperationFailed)
                    {
                        failedDatabasesString += logView.DatabaseNames[i - logView.LogFilePaths.Count] + "," + Environment.NewLine;
                    }
                }
                if (failedDatabasesString.Length > 2)
                {
                    failedDatabasesString = failedDatabasesString.Substring(0, failedDatabasesString.Length - 3);
                }

                if (!string.IsNullOrWhiteSpace(failedLogFilesString))
                {
                    errorMessage += $"Failed to load log entries for the following log files:{Environment.NewLine + failedLogFilesString}";
                    if (!string.IsNullOrWhiteSpace(failedDatabasesString))
                    {
                        errorMessage += $"{Environment.NewLine + Environment.NewLine}Failed to load log entries for the following databases:{Environment.NewLine + failedDatabasesString}";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(failedDatabasesString))
                {
                    errorMessage += $"Failed to load log entries for the following databases:{Environment.NewLine + failedDatabasesString}";
                }
            });

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                //TODO: I'm getting this summary error message and individual error messages. Have to disable the error message for each individual log source.
                await DialogCoordinator.ShowMessageAsync(this, "Failed to Load Log Entries", errorMessage);
            }

            //Notify the LogViewManagementViewModel that the AutoRefreshSetting changed so that it can tell databases and log files
            Mediator.NotifyColleagues(MediatorMessages.AutoRefreshSettingChanged, logView.Settings.AutoRefresh);

            logView.IsLoading = false;
        }

        private async void OpenLogViewDetails(LogView logView)
        {
            ResetSearchbox();
            var logViewDetails = new LogViewDetails() { LogView = logView };
            await Task.Run(() =>
            {
                foreach (var logFilePath in LogFilesSourceCache.Keys.Where(l => logView.LogFilePaths.Contains(l)))
                {
                    logViewDetails.LogFilePaths.Add(logFilePath);
                }

                foreach (var databaseName in DatabasesSourceCache.Keys.Where(d => logView.DatabaseNames.Contains(d)))
                {
                    logViewDetails.DatabaseNames.Add(databaseName);
                }
            });
            Mediator.NotifyColleagues(MediatorMessages.RequestShowLogViewDetailsDialog, logViewDetails);
        }

        private void OpenSelectedLogFile()
        {
            if (SelectedLogFile == null)
            {
                return;
            }
            LogView logView = null;
            if (LogViewToOpenDocumentIn != null)
            {
                logView = LogViewToOpenDocumentIn;
                logView.AddLogFile(SelectedLogFile);
            }
            else
            {
                logView = new LogView(SelectedLogFile);
                LogViewsSourceCache.AddOrUpdate(logView);
            }
            OpenLogView(logView);
        }

        private void OpenDatabase(Database database)
        {
            LogView logView = null;
            if (LogViewToOpenDocumentIn != null)
            {
                logView = LogViewToOpenDocumentIn;
                logView.AddDatabase(database);
            }
            else
            {
                logView = new LogView(database);
                LogViewsSourceCache.AddOrUpdate(logView);
            }
            OpenLogView(logView);
        }

        private async void RenameLogView(LogView logView)
        {
            await LogViewService.RenameLogViewRoutine(DialogCoordinator, this, LogViewsSourceCache, logView);
        }

        private void RequestSetSelectedLogView(LogView logView)
        {
            Mediator.NotifyColleagues(MediatorMessages.RequestSetSelectedLogView, logView);
        }

        private void ToggleOpenableObjectIsPinned(OpenableObject openableObject)
        {
            openableObject.IsPinned = !openableObject.IsPinned;
            OpenableObjectService.SaveOpenableObject(openableObject);
        }

        private async void DeleteLogView(LogView logView)
        {
            var logViewIsOpen = logView.IsOpen;
            await LogViewService.DeleteLogViewRoutine(DialogCoordinator, this, LogViewsSourceCache, logView);
            if (logViewIsOpen)
            {
                AdjustOpenLogViewCount(-1);
            }
        }

        private async void RemoveDatabase(Database database)
        {
            await DatabaseService.RemoveDatabaseRoutine(DialogCoordinator, this, DatabasesSourceCache, database);
        }

        private void OpenOpenLogViewView(object obj = null)
        {
            if (obj is LogView)
            {
                LogViewToOpenDocumentIn = (LogView)obj;
            }
            SelectedMenuItemIndex = 2;
        }

        private void OpenOpenLogFileView(object obj = null)
        {
            if (obj is LogView)
            {
                LogViewToOpenDocumentIn = (LogView)obj;
            }
            SelectedMenuItemIndex = 3;
        }

        private void OpenOpenDatabaseView(object obj = null)
        {
            if (obj is LogView)
            {
                LogViewToOpenDocumentIn = (LogView)obj;
            }
            SelectedMenuItemIndex = 4;
        }

        private void AdjustOpenLogViewCount(object obj)
        {
            if (obj is int adjustment)
            {
                _openLogViewCount += adjustment;
                CanCloseLaunchView = _openLogViewCount > 0;
            }
        }
        #endregion
    }
}
