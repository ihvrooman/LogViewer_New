using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using DynamicData.Binding;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LogViewer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields
        #region Commands
        private ICommand _requestOpenHomeViewCommand;
        private ICommand _renameLogViewCommand;
        private ICommand _closeLogViewCommand;
        private ICommand _closeAllLogViewsCommand;
        private ICommand _closeAllLogViewsButThisCommand;
        private ICommand _deleteLogViewCommand;
        private ICommand _saveLogViewCommand;
        private ICommand _saveAllLogViewsCommand;
        private ICommand _refreshLogViewCommand;
        #endregion
        #region Readonly observable collections
        private ReadOnlyObservableCollection<LogView> _openLogViews;
        private ReadOnlyObservableCollection<LogView> _unsavedLogViews;
        private static ReadOnlyObservableCollection<string> _availableUsernames;
        private static ReadOnlyObservableCollection<string> _availableComputernames;
        #endregion
        private LogView _selectedLogView;
        private IDialogCoordinator _dialogCoordinator;
        private bool _launchViewIsOpen;
        private HorizontalAlignment _launchViewHorizantalContentAlignment = HorizontalAlignment.Stretch;
        private int _launchViewWidth;
        private GridLength _logViewManagementColumnWidth;
        private GridLength _logEntryMessageViewRowHeight;
        private string _selectedLogMessage;
        private bool _canCloseLaunchView;
        private volatile bool _ignoreToggleLaunchViewIsOpenRequests;
        #endregion

        #region Properties
        #region Commands
        public ICommand RequestOpenHomeViewCommand
        {
            get
            {
                if (_requestOpenHomeViewCommand == null)
                {
                    _requestOpenHomeViewCommand = new RelayCommand(RequestOpenHomeView);
                }
                return _requestOpenHomeViewCommand;
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
        public ICommand CloseLogViewCommand
        {
            get
            {
                if (_closeLogViewCommand == null)
                {
                    _closeLogViewCommand = new RelayCommand<object>(CloseLogView);
                }
                return _closeLogViewCommand;
            }
        }
        public ICommand CloseAllLogViewsCommand
        {
            get
            {
                if (_closeAllLogViewsCommand == null)
                {
                    _closeAllLogViewsCommand = new RelayCommand(CloseAllLogViews);
                }
                return _closeAllLogViewsCommand;
            }
        }
        public ICommand CloseAllLogViewsButThisCommand
        {
            get
            {
                if (_closeAllLogViewsButThisCommand == null)
                {
                    _closeAllLogViewsButThisCommand = new RelayCommand<LogView>(CloseAllLogViewsButThis);
                }
                return _closeAllLogViewsButThisCommand;
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
        public ICommand SaveLogViewCommand
        {
            get
            {
                if (_saveLogViewCommand == null)
                {
                    _saveLogViewCommand = new RelayCommand<object>(SaveLogView);
                }
                return _saveLogViewCommand;
            }
        }
        public ICommand SaveAllLogViewsCommand
        {
            get
            {
                if (_saveAllLogViewsCommand == null)
                {
                    _saveAllLogViewsCommand = new RelayCommand(SaveAllLogViews);
                }
                return _saveAllLogViewsCommand;
            }
        }
        public ICommand RefreshLogViewCommand
        {
            get
            {
                if (_refreshLogViewCommand == null)
                {
                    _refreshLogViewCommand = new RelayCommand<LogView>(RefreshLogView);
                }
                return _refreshLogViewCommand;
            }
        }
        #endregion
        #region Source caches
        public SourceCache<LogView, string> LogViewsSourceCache { get; private set; } = new SourceCache<LogView, string>(logView => logView.Name);
        public SourceCache<Database, string> DatabasesSourceCache { get; private set; } = new SourceCache<Database, string>(database => database.Name);
        public SourceCache<LogFile, string> LogFilesSourceCache { get; private set; } = new SourceCache<LogFile, string>(logFile => logFile.NetworkFile.FullName);
        public SourceCache<LogEntry, string> LogEntriesSourceCache { get; private set; } = new SourceCache<LogEntry, string>(logEntry => logEntry.Identifier);
        public SourceCache<string, string> AvailableUsernamesSourceCache = new SourceCache<string, string>(username => username);
        public SourceCache<string, string> AvailableComputernamesSourceCache = new SourceCache<string, string>(computername => computername);
        #endregion
        #region Readonly observable collections
        public ReadOnlyObservableCollection<LogView> OpenLogViews { get { return _openLogViews; } set { _openLogViews = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogView> UnsavedLogViews { get { return _unsavedLogViews; } private set { _unsavedLogViews = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableUsernames { get { return _availableUsernames; } set { _availableUsernames = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableComputernames { get { return _availableComputernames; } set { _availableComputernames = value; RaisePropertyChangedEvent(); } }
        #endregion
        public LogView SelectedLogView
        {
            get { return _selectedLogView; }
            set
            {
                Mediator.NotifyColleagues(MediatorMessages.SelectedLogViewChanged, new SelectedLogViewChangedEventArgs(SelectedLogView, value));
                _selectedLogView = value;
                RaisePropertyChangedEvent();
                if (SelectedLogView == null)
                {
                    Mediator.NotifyColleagues(MediatorMessages.SelectedLogMessageChanged, string.Empty);
                }
            }
        }
        public bool LaunchViewIsOpen
        {
            get { return _launchViewIsOpen; }
            set
            {
                if (!_ignoreToggleLaunchViewIsOpenRequests)
                {
                    _launchViewIsOpen = value;
                    RaisePropertyChangedEvent();
                }
            }
        }
        public HorizontalAlignment LaunchViewHorizantalContentAlignment { get { return _launchViewHorizantalContentAlignment; } set { _launchViewHorizantalContentAlignment = value; RaisePropertyChangedEvent(); } }
        public int LaunchViewWidth { get { return _launchViewWidth; } set { _launchViewWidth = value; RaisePropertyChangedEvent(); } }
        public GridLength LogViewManagementColumnWidth { get { return _logViewManagementColumnWidth; } set { _logViewManagementColumnWidth = value; RaisePropertyChangedEvent(); } }
        public GridLength LogEntryMessageViewRowHeight { get { return _logEntryMessageViewRowHeight; } set { _logEntryMessageViewRowHeight = value; RaisePropertyChangedEvent(); } }
        public string SelectedLogMessage { get { return _selectedLogMessage; } set { _selectedLogMessage = value; RaisePropertyChangedEvent(); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            RegisterEventHandlersWithMediator();

            SettingsService.InitializeCollections();
            InitializeBindings();
            InitializeSourceCaches();
            ToggleLaunchViewIsOpen(LaunchViewDisplayOptions.FullWindowWidth);
        }
        #endregion

        #region Public/internal methods
        public async Task<bool> SaveAllUnsavedLogViews(bool showConfAndWaitDialogs = true)
        {
            return await SaveLogViews(UnsavedLogViews.ToList(), showConfAndWaitDialogs);
        }

        public void SaveLayout()
        {
            SettingsService.SetLogViewManagementColumnWidth(LogViewManagementColumnWidth.Value);
            SettingsService.SetLogEntryMessageViewRowHeight(LogEntryMessageViewRowHeight.Value);
        }
        #endregion

        #region Private methods
        private void RegisterEventHandlersWithMediator()
        {
            Mediator.Register(MediatorMessages.RequestToggleLaunchViewIsOpen, ToggleLaunchViewIsOpen);
            Mediator.Register(MediatorMessages.RequestSetSelectedLogView, SetSelectedLogView);
            Mediator.Register(MediatorMessages.AddAvailableUsername, AddAvailableUsernameAsync);
            Mediator.Register(MediatorMessages.AddAvailableComputername, AddAvailableComputernameAsync);
            Mediator.Register(MediatorMessages.SelectedLogMessageChanged, SelectedLogMessageChanged);
            Mediator.Register(MediatorMessages.CanCloseLaunchViewChanged, CanCloseLaunchViewChanged);
            Mediator.Register(MediatorMessages.UpdateIgnoreToggleLaunchViewIsOpenRequests, UpdateIgnoreToggleLaunchViewIsOpenRequests);
        }

        private void InitializeBindings()
        {
            LogViewsSourceCache.Connect()
                    .AutoRefresh(logView => logView.IsOpen)
                    .Filter(logView => logView.IsOpen)
                    .ObserveOnDispatcher()
                    .Bind(out _openLogViews)
                    .DisposeMany()
                    .Subscribe();
            LogViewsSourceCache.Connect()
                    .AutoRefresh(logView => logView.IsEdited)
                    .Filter(logView => logView.IsEdited)
                    .ObserveOnDispatcher()
                    .Bind(out _unsavedLogViews)
                    .DisposeMany()
                    .Subscribe();

            AvailableUsernamesSourceCache.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableUsernames)
                    .DisposeMany()
                    .Subscribe();
            AvailableComputernamesSourceCache.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableComputernames)
                    .DisposeMany()
                    .Subscribe();
        }

        private async void InitializeSourceCaches()
        {
            InitializeAvailableUsernamesAndComputernamesAsync();
            var results = await Task.WhenAll(
                SettingsService.LoadLogViewsIntoSourceCache(LogViewsSourceCache),
                SettingsService.LoadLogFilesIntoSourceCache(LogFilesSourceCache),
                SettingsService.LoadDatabasesIntoSourceCache(DatabasesSourceCache));
            foreach (var result in results)
            {
                if (result.OperationFailed)
                {
                    //TODO: Notify user. It's already been logged.
                }
            }
        }

        private void InitializeAvailableUsernamesAndComputernamesAsync()
        {
            Task.Run(() =>
            {
                AvailableUsernamesSourceCache.Edit(innerCache =>
                {
                    innerCache.Clear();
                    innerCache.AddOrUpdate(string.Empty);
                });
                AvailableComputernamesSourceCache.Edit(innerCache =>
                {
                    innerCache.Clear();
                    innerCache.AddOrUpdate(string.Empty);
                });
            });
        }

        private async Task<bool> SaveLogViews(IEnumerable<LogView> logViewsToSave, bool showConfAndWaitDialogs = true)
        {
            var editedLogViewNames = string.Empty;
            foreach (var logView in logViewsToSave)
            {
                editedLogViewNames += $"{logView.Name},{Environment.NewLine}";
            }

            if (!string.IsNullOrWhiteSpace(editedLogViewNames))
            {
                var dialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    FirstAuxiliaryButtonText = "Cancel",
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                };

                editedLogViewNames = editedLogViewNames.Substring(0, editedLogViewNames.Length - 3);

                var dialogResult = !showConfAndWaitDialogs ? MessageDialogResult.Affirmative : await _dialogCoordinator.ShowMessageAsync(this, "Save Log Views", $"Would you like to save changes to the following log views?{Environment.NewLine + Environment.NewLine + editedLogViewNames}", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, dialogSettings);

                if (dialogResult == MessageDialogResult.Affirmative)
                {
                    var minWaitTime = !showConfAndWaitDialogs ? 0 : 300;
                    var progressController = !showConfAndWaitDialogs ? null : await _dialogCoordinator.ShowProgressAsync(this, "Saving Log Views", "Preparing...");
                    progressController?.SetIndeterminate();

                    var failedLogViews = string.Empty;
                    await Task.Run(async () =>
                    {
                        foreach (var logView in logViewsToSave)
                        {
                            try
                            {
                                progressController?.SetMessage($"Saving log view \"{logView.Name}\"...");
                                await Task.WhenAll(SettingsService.AddOrUpdateLogView(logView), Task.Delay(minWaitTime));
                                logView.IsEdited = false;
                            }
                            catch
                            {
                                failedLogViews += logView.Name + $",{Environment.NewLine}";
                            }
                        }

                        SettingsService.SaveSettings();
                    });

                    if (progressController != null)
                    {
                        await progressController.CloseAsync();
                    }

                    if (!string.IsNullOrWhiteSpace(failedLogViews))
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Save Failed", $"{AppInfo.BaseAppInfo.AppName} failed to save the following log views:{Environment.NewLine + Environment.NewLine + failedLogViews}");
                    }
                }
                else if (dialogResult == MessageDialogResult.Negative)
                {
                    //If user declines to save log views, reload them to discard in-memory changes
                    var tasks = new List<Task<ServiceOperationResult>>();
                    foreach (var logView in logViewsToSave)
                    {
                        tasks.Add(SettingsService.ReloadLogView(logView));
                    }
                    await Task.WhenAll(tasks);
                }
                else if (dialogResult == MessageDialogResult.FirstAuxiliary)
                {
                    return false;
                }
            }
            return true;
        }

        private void ToggleLaunchViewIsOpen(object obj)
        {
            if (LaunchViewIsOpen && !_canCloseLaunchView)
            {
                return;
            }

            if (obj != null)
            {
                var displayOptions = (LaunchViewDisplayOptions)obj;
                switch (displayOptions)
                {
                    case LaunchViewDisplayOptions.FullWindowWidth:
                        LaunchViewHorizantalContentAlignment = HorizontalAlignment.Stretch;
                        LaunchViewWidth = -1;
                        break;
                    case LaunchViewDisplayOptions.ToolbarWidth:
                        LaunchViewHorizantalContentAlignment = HorizontalAlignment.Left;
                        LaunchViewWidth = Convert.ToInt32(Application.Current.MainWindow.Width - LogViewManagementColumnWidth.Value - 5);
                        break;
                    default:
                        LaunchViewHorizantalContentAlignment = HorizontalAlignment.Stretch;
                        LaunchViewWidth = -1;
                        break;
                }
            }
            LaunchViewIsOpen = !LaunchViewIsOpen;
        }

        private void RequestOpenHomeView()
        {
            Mediator.NotifyColleagues(MediatorMessages.RequestOpenHomeView, null);
            ToggleLaunchViewIsOpen(LaunchViewDisplayOptions.FullWindowWidth);
        }

        private async void RenameLogView(LogView logView)
        {
            if (logView == null)
            {
                if (SelectedLogView == null)
                {
                    return;
                }
                logView = SelectedLogView;
            }

            await LogViewService.RenameLogViewRoutine(_dialogCoordinator, this, LogViewsSourceCache, logView);
        }

        private async void CloseLogView(object logViewObj)
        {
            if (logViewObj == null)
            {
                logViewObj = SelectedLogView;
            }

            if (logViewObj is LogView logView && (!logView.IsEdited || await SaveLogViews(new List<LogView>() { logView })))
            {
                RemoveNewAndUnsavedLogViewsFromSourceCache(new List<LogView>() { logView });
                logView.Close();
                ProcessLogViewClosed();
            }
        }

        private async void CloseAllLogViews()
        {
            var unsavedLogViews = UnsavedLogViews.ToList();
            unsavedLogViews.Remove(unsavedLogViews.Where(lv => !lv.IsOpen).ToList());
            if (await SaveLogViews(unsavedLogViews))
            {
                RemoveNewAndUnsavedLogViewsFromSourceCache(unsavedLogViews);
                foreach (var openLogView in OpenLogViews)
                {
                    openLogView.Close();
                    ProcessLogViewClosed();
                }
            }
        }

        private async void CloseAllLogViewsButThis(LogView logView)
        {
            //TODO: If rename on open view, it shows up as prompting to save here and close all
            var unsavedLogViews = UnsavedLogViews.ToList();
            unsavedLogViews.Remove(unsavedLogViews.Where(lv => !lv.IsOpen || lv == logView).ToList());
            if (await SaveLogViews(unsavedLogViews))
            {
                RemoveNewAndUnsavedLogViewsFromSourceCache(unsavedLogViews);
                foreach (var openLogView in OpenLogViews)
                {
                    if (openLogView != logView)
                    {
                        openLogView.Close();
                        ProcessLogViewClosed();
                    }
                }
            }
        }

        private void RemoveNewAndUnsavedLogViewsFromSourceCache(IEnumerable<LogView> logViews)
        {
            LogViewsSourceCache.Edit(innerCache =>
            {
                var newAndUnsavedLogViews = logViews.Where(lv => lv.IsOpen && lv.IsNew && lv.IsEdited);
                innerCache.Remove(newAndUnsavedLogViews);
            });
        }

        private void SetSelectedLogView(object obj)
        {
            if (obj is LogView)
            {
                SelectedLogView = (LogView)obj;
            }
        }

        private void AddAvailableUsernameAsync(object obj)
        {
            Task.Run(() =>
            {
                var username = obj?.ToString();
                var operationHelper = new ServiceOperationHelper("Username", Plurality.Single, ServiceOperation.AddOrUpdate, $"{typeof(FilterOptions).Name} available usernames", username);
                try
                {
                    if (!string.IsNullOrWhiteSpace(username) && !AvailableUsernamesSourceCache.Keys.Contains(username))
                    {
                        operationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                        AvailableUsernamesSourceCache.Edit(innerCache =>
                        {
                            innerCache.AddOrUpdate(username);
                        });

                        operationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    }
                }
                catch (Exception ex)
                {
                    operationHelper.LogServiceOperation(ex.Message);
                }
            });
        }

        private void AddAvailableComputernameAsync(object obj)
        {
            Task.Run(() =>
            {
                var computername = obj?.ToString();
                var operationHelper = new ServiceOperationHelper("Computername", Plurality.Single, ServiceOperation.AddOrUpdate, $"{typeof(FilterOptions).Name} available computernames", computername);
                try
                {
                    if (!string.IsNullOrWhiteSpace(computername) && !AvailableComputernamesSourceCache.Keys.Contains(computername))
                    {
                        operationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                        AvailableComputernamesSourceCache.Edit(innerCache =>
                        {
                            innerCache.AddOrUpdate(computername);
                        });

                        operationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    }
                }
                catch (Exception ex)
                {
                    operationHelper.LogServiceOperation(ex.Message);
                }
            });
        }

        private async void DeleteLogView(LogView logView)
        {
            await LogViewService.DeleteLogViewRoutine(_dialogCoordinator, this, LogViewsSourceCache, logView);
        }

        private void SelectedLogMessageChanged(object obj)
        {
            if (obj is string logMessage)
            {
                SelectedLogMessage = logMessage;
            }
        }

        private void CanCloseLaunchViewChanged(object obj)
        {
            if (obj is bool canCloseLaunchView)
            {
                _canCloseLaunchView = canCloseLaunchView;
            }
        }

        private void ProcessLogViewClosed()
        {
            Mediator.NotifyColleagues(MediatorMessages.AdjustOpenLogViewCount, -1);
            if (!_canCloseLaunchView)
            {
                RequestOpenHomeView();
                ToggleLaunchViewIsOpen(LaunchViewDisplayOptions.FullWindowWidth);
            }
        }

        private async void SaveLogView(object logViewObj)
        {
            if (logViewObj == null)
            {
                if (SelectedLogView == null)
                {
                    return;
                }

                logViewObj = SelectedLogView;
            }
            
            if (logViewObj is LogView logView && logView.IsEdited)
            {
                await SaveLogViews(new List<LogView> { logView }, false);
            }
        }

        private async void SaveAllLogViews()
        {
            await SaveAllUnsavedLogViews(false);
        }

        private void UpdateIgnoreToggleLaunchViewIsOpenRequests(object obj)
        {
            if (obj is bool ignoreToggleLaunchViewIsOpenRequests)
            {
                _ignoreToggleLaunchViewIsOpenRequests = ignoreToggleLaunchViewIsOpenRequests;
            }
        }

        private void RefreshLogView(LogView logView)
        {
            if (logView == null)
            {
                if (SelectedLogView == null)
                {
                    return;
                }

                logView = SelectedLogView;
            }

            if (logView.LogFilePaths == null || logView.DatabaseNames == null)
            {
                return;
            }
            
            LogFilesSourceCache.Items
                .Where(l => logView.LogFilePaths.Contains(l.Identifier))
                .ToList()
                .ForEach(l => l.RequestRefresh());
            DatabasesSourceCache.Items
                .Where(d => logView.DatabaseNames.Contains(d.Identifier))
                .ToList()
                .ForEach(d => d.RequestRefresh());
        }
        #endregion
    }
}
