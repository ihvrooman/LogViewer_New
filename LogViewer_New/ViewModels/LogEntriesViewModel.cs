using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using DynamicData.Binding;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Services;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogViewer.ViewModels
{
    public class LogEntriesViewModel : BaseViewModel
    {
        #region Fields
        private ReadOnlyObservableCollection<LogEntry> _logEntries;
        private LogEntry _selectedLogEntry;
        private LogView _selectedLogView;
        #endregion

        #region Properties
        public SourceCache<LogEntry, string> LogEntriesSourceCache { get; set; }
        public ReadOnlyObservableCollection<LogEntry> LogEntries { get { return _logEntries; } private set { _logEntries = value; RaisePropertyChangedEvent(); } }
        public LogEntry SelectedLogEntry { get { return _selectedLogEntry; } set { _selectedLogEntry = value; RaisePropertyChangedEvent(); ProcessSelectedLogEntryChanged(); } }
        public IDialogCoordinator DialogCoordinator { get; set; }
        public LogView SelectedLogView { get { return _selectedLogView; } set { _selectedLogView = value; RaisePropertyChangedEvent(); } }
        #endregion

        #region Public methods
        public void Initialize()
        {
            Mediator.Register(MediatorMessages.SelectedLogViewChanged, SelectedLogViewChanged);
            Mediator.Register(MediatorMessages.FilterOptionsChanged, FilterOptionsChanged);
            Mediator.Register(MediatorMessages.LogSourceRemovedFromSelectedLogView, LogSourceRemovedFromSelectedLogView);
            Mediator.Register(MediatorMessages.RequestRefreshLogFile, RefreshLogFile);
            Mediator.Register(MediatorMessages.RequestRefreshDatabase, RefreshDatabase);
            InitializeBindings();
        }
        #endregion

        #region Private methods
        private void InitializeBindings()
        {
            //TODO: Add filtering.
            LogEntriesSourceCache.Connect()
                    .Filter(logEntry => IncludeLogEntry(logEntry))
                    .ObserveOnDispatcher()
                    .Bind(out var logEntries)
                    .Subscribe();
            LogEntries = logEntries;
        }

        private void SelectedLogViewChanged(object obj)
        {
            var e = (SelectedLogViewChangedEventArgs)obj;
            SelectedLogView = e.NewLogView;
            RefreshLogEntriesSourceCacheAsync(e.OldLogView, e.NewLogView);
        }

        private void LogSourceRemovedFromSelectedLogView(object obj)
        {
            Task.Run(() =>
            {
                if (obj is ILogEntriesSource logEntriesSource)
                {
                    var logEntriesToRefresh = new List<LogEntry>();
                    if (logEntriesSource != null)
                    {
                        logEntriesToRefresh.Add(LogEntriesSourceCache.Items.Where(logEntry => logEntry.LogEntriesSource == logEntriesSource));
                    }
                    LogEntriesSourceCache.Refresh(logEntriesToRefresh);
                    RaisePropertyChangedEvent("LogEntries");
                }
            });
        }

        private void RefreshLogEntriesSourceCacheAsync(LogView oldLogView, LogView newLogView)
        {
            Task.Run(() =>
            {
                LogEntriesSourceCache.Refresh(LogEntriesSourceCache.Items.Where(l => (oldLogView != null && (oldLogView.LogFilePaths.Contains(l.LogEntriesSource.Identifier) || oldLogView.DatabaseNames.Contains(l.LogEntriesSource.Identifier))) || (newLogView != null && (newLogView.LogFilePaths.Contains(l.LogEntriesSource.Identifier) || newLogView.DatabaseNames.Contains(l.LogEntriesSource.Identifier)))));
                RaisePropertyChangedEvent("LogEntries");
            });
        }

        private bool IncludeLogEntry(LogEntry logEntry)
        {
            if (SelectedLogView == null || !SelectedLogViewContainsLogEntry(logEntry) || !FilterOptionsIncludeLogEntry(logEntry))
            {
                return false;
            }

            return true;
        }

        private bool SelectedLogViewContainsLogEntry(LogEntry logEntry)
        {
            return SelectedLogView.DatabaseNames.Contains(logEntry.LogEntriesSource.Identifier) || SelectedLogView.LogFilePaths.Contains(logEntry.LogEntriesSource.Identifier);
        }

        private bool FilterOptionsIncludeLogEntry(LogEntry logEntry)
        {
            /* Determine whether or not to include the given log entry based on the following
             * filters:
             *     The log message type filter
             *     The log message timestamp filters
             *     The username filter
             *     The computername filter
             *     The search term filter
             *     The exclusion term filter
             */

            bool include = true;

            //Check log message type filters
            switch (logEntry.Type)
            {
                case LogMessageType.Error:
                    include = include && SelectedLogView.FilterOptions.IncludeErrors;
                    break;
                case LogMessageType.Warning:
                    include = include && SelectedLogView.FilterOptions.IncludeWarnings;
                    break;
                case LogMessageType.Information:
                    include = include && SelectedLogView.FilterOptions.IncludeInformation;
                    break;
                case LogMessageType.Verbose:
                    include = include && SelectedLogView.FilterOptions.IncludeVerbose;
                    break;
                case LogMessageType.Debug:
                    include = include && SelectedLogView.FilterOptions.IncludeDebug;
                    break;
                case LogMessageType.DebugError:
                    include = include && SelectedLogView.FilterOptions.IncludeDebugErrors;
                    break;
                case LogMessageType.DebugWarning:
                    include = include && SelectedLogView.FilterOptions.IncludeDebugWarnings;
                    break;
                case LogMessageType.DebugInformation:
                    include = include && SelectedLogView.FilterOptions.IncludeDebugInformation;
                    break;
                case LogMessageType.DebugVerbose:
                    include = include && SelectedLogView.FilterOptions.IncludeDebugVerbose;
                    break;
                case LogMessageType.Unknown:
                    include = include && SelectedLogView.FilterOptions.IncludeUnknown;
                    break;
                default:
                    break;
            }

            //Check time filters
            include = include && DateFilterIncludesLogEntry(logEntry);

            //Check username filter
            include = include && (string.IsNullOrWhiteSpace(SelectedLogView.FilterOptions.FilterUsername) || SelectedLogView.FilterOptions.FilterUsername == logEntry.Username);

            //Check computername filter
            include = include && (string.IsNullOrWhiteSpace(SelectedLogView.FilterOptions.FilterComputername) || SelectedLogView.FilterOptions.FilterComputername == logEntry.Computername);

            //Check search term filter
            include = include && (string.IsNullOrWhiteSpace(SelectedLogView.FilterOptions.SearchTerm) || logEntry.Message.ToLower().Contains(SelectedLogView.FilterOptions.SearchTerm.ToLower()));

            //Check exclusion term filter
            include = include && (string.IsNullOrWhiteSpace(SelectedLogView.FilterOptions.ExclusionTerm) || !logEntry.Message.ToLower().Contains(SelectedLogView.FilterOptions.ExclusionTerm.ToLower()));

            return include;
        }

        private bool DateFilterIncludesLogEntry(LogEntry logEntry)
        {
            return logEntry.UtcTimeStamp.ToLocalTime() >= SelectedLogView.FilterOptions.MinDate && logEntry.UtcTimeStamp.ToLocalTime() <= SelectedLogView.FilterOptions.MaxDate;
        }

        private void FilterOptionsChanged(object obj)
        {
            RefreshLogEntriesSourceCacheAsync(SelectedLogView, null);
        }

        private void ProcessSelectedLogEntryChanged()
        {
            Mediator.NotifyColleagues(MediatorMessages.SelectedLogMessageChanged, SelectedLogEntry?.Message);
        }

        private async void RefreshLogFile(object obj)
        {
            if (obj is LogFile logFile)
            {
                var result = await LogFileService.LoadLogEntriesIntoSourceCacheAsync(logFile, LogEntriesSourceCache);
                if (result.OperationFailed)
                {
                    await result.ShowUserErrorMessage(DialogCoordinator, this);
                }
                await Task.Delay(1000);
                logFile.Refreshing = false;
                if (logFile.RefreshPending)
                {
                    logFile.RefreshPending = false;
                    if (logFile.AutoRefresh)
                    {
                        logFile.Refreshing = true;
                        RefreshLogFile(logFile);
                    }
                }
            }
        }

        private async void RefreshDatabase(object obj)
        {
            if (obj is Database database)
            {
                var result = await DatabaseService.LoadLogEntriesIntoSourceCache(database, LogEntriesSourceCache);
                if (result.OperationFailed)
                {
                    await result.ShowUserErrorMessage(DialogCoordinator, this);
                }
                await Task.Delay(1000);
                database.Refreshing = false;
                if (database.RefreshPending)
                {
                    database.RefreshPending = false;
                    if (database.AutoRefresh)
                    {
                        database.Refreshing = true;
                        RefreshDatabase(database);
                    }
                }
            }
        }
        #endregion
    }
}
