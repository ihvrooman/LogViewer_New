using AppStandards.Logging;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class SettingsService
    {
        #region Constants
        private const string _targetName = "settings";
        #endregion

        #region Public methods
        #region Load into source cache methods
        public static async Task<ServiceOperationResult> LoadLogViewsIntoSourceCache(SourceCache<LogView, string> sourceCache)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogView), Plurality.Plural, ServiceOperation.Load, _targetName);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    sourceCache.Edit(innerCache =>
                    {
                        foreach (var xmlLogView in Settings.Default.XmlLogViews)
                        {
                            innerCache.AddOrUpdate(new LogView(xmlLogView));
                        }
                    });
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> LoadLogFilesIntoSourceCache(SourceCache<LogFile, string> sourceCache)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogFile), Plurality.Plural, ServiceOperation.Load, _targetName);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    sourceCache.Edit(innerCache =>
                    {
                        foreach (var xmlLogFile in Settings.Default.XmlLogFiles)
                        {
                            innerCache.AddOrUpdate(new LogFile(xmlLogFile, false));
                        }
                    });
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> LoadDatabasesIntoSourceCache(SourceCache<Database, string> sourceCache)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(Database), Plurality.Plural, ServiceOperation.Load, _targetName);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    sourceCache.Edit(innerCache =>
                    {
                        foreach (var xmlDatabase in Settings.Default.XmlDatabases)
                        {
                            innerCache.AddOrUpdate(new Database(xmlDatabase, false));
                        }
                    });
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> ReloadLogView(LogView logView)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogView), Plurality.Single, ServiceOperation.Reload, _targetName, logView.Name);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                    var index = GetLogViewIndexByName(logView.Name);
                    if (index < 0)
                    {
                        throw new ArgumentException($"The {typeof(LogView).Name} \"{logView.Name}\" was not found in the {_targetName}.");
                    }
                    logView.UpdateFromXml(Settings.Default.XmlLogViews[index]);

                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }
        #endregion

        #region Get index by name methods
        /// <summary>
        /// Gets the index of the <see cref="LogView"/> with the specified name in the <see cref="Settings.XmlLogViews"/> collection.
        /// </summary>
        /// <param name="logFilePath">The name of the <see cref="LogView"/> to find.</param>
        /// <returns>The index of the <see cref="LogView"/> with the specified name or -1 if the <see cref="LogView"/> could not be found.</returns>
        public static int GetLogViewIndexByName(string logViewName)
        {
            var index = -1;
            if (logViewName != null && Settings.Default.XmlLogViews != null)
            {
                for (int i = 0; i < Settings.Default.XmlLogViews.Count; i++)
                {
                    var setting = Settings.Default.XmlLogViews[i];
                    if (setting.Contains(logViewName))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Gets the index of the <see cref="Database"/> with the specified name in the <see cref="Settings.XmlDatabases"/> collection.
        /// </summary>
        /// <param name="databaseName">The name of the <see cref="Database"/> to find.</param>
        /// <returns>The index of the <see cref="Database"/> with the specified name or -1 if the database could not be found.</returns>
        public static int GetDatabaseIndexByName(string databaseName)
        {
            var index = -1;
            if (databaseName != null && Settings.Default.XmlDatabases != null)
            {
                for (int i = 0; i < Settings.Default.XmlDatabases.Count; i++)
                {
                    var setting = Settings.Default.XmlDatabases[i];
                    if (setting.Contains(databaseName))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Gets the index of the <see cref="LogFile"/> with the specified path in the <see cref="Settings.XmlLogFiles"/> collection.
        /// </summary>
        /// <param name="logFilePath">The path of the <see cref="LogFile"/> to find.</param>
        /// <returns>The index of the <see cref="LogFile"/> with the specified path or -1 if the <see cref="LogFile"/> could not be found.</returns>
        public static int GetLogFileIndexByPath(string logFilePath)
        {
            var index = -1;
            if (logFilePath != null && Settings.Default.XmlLogFiles != null)
            {
                for (int i = 0; i < Settings.Default.XmlLogFiles.Count; i++)
                {
                    var setting = Settings.Default.XmlLogFiles[i];
                    if (setting.Contains(logFilePath))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }
        #endregion

        #region Contains methods
        public static bool SettingsContainsLogViewName(string logViewName)
        {
            return GetLogViewIndexByName(logViewName) > -1;
        }

        public static bool SettingsContainsDatabaseName(string databaseName)
        {
            return GetDatabaseIndexByName(databaseName) > -1;
        }

        public static bool SettingsContainsLogFilePath(string logFilePath)
        {
            return GetLogFileIndexByPath(logFilePath) > -1;
        }
        #endregion

        #region Add or update methods
        public static async Task<ServiceOperationResult> AddOrUpdateLogView(LogView logView, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogView), Plurality.Single, ServiceOperation.Add, _targetName, logView.Name);
            await Task.Run(() =>
            {
                try
                {
                    var index = GetLogViewIndexByName(logView.Name);
                    if (index < 0)
                    {
                        //If item isn't in settings, add it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlLogViews.Add(logView.ExportToXml());
                    }
                    else
                    {
                        //If item is in settings, update it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlLogViews[index] = logView.ExportToXml();
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);

                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> AddOrUpdateDatabase(Database database, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(Database), Plurality.Single, ServiceOperation.Add, _targetName, database.Name);
            await Task.Run(() =>
            {
                try
                {
                    var index = GetDatabaseIndexByName(database.Name);
                    if (index < 0)
                    {
                        //If item isn't in settings, add it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlDatabases.Add(database.ExportToXml());
                    }
                    else
                    {
                        //If item is in settings, update it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlDatabases[index] = database.ExportToXml();
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> AddOrUpdateLogFile(LogFile logFile, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogFile), Plurality.Single, ServiceOperation.Add, _targetName, logFile.NetworkFile.FullName);
            await Task.Run(() =>
            {
                try
                {
                    var index = GetLogFileIndexByPath(logFile.NetworkFile.FullName);
                    if (index < 0)
                    {
                        //If item isn't in settings, add it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlLogFiles.Add(logFile.ExportToXml());
                    }
                    else
                    {
                        //If item is in settings, update it
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        Settings.Default.XmlLogFiles[index] = logFile.ExportToXml();
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }
        #endregion

        #region Remove methods
        public static async Task<ServiceOperationResult> RemoveLogViewByName(string logViewName, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogView), Plurality.Single, ServiceOperation.Remove, _targetName, logViewName);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    var index = GetLogViewIndexByName(logViewName);
                    if (index >= 0)
                    {
                        Settings.Default.XmlLogViews.RemoveAt(index);
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> RemoveDatabaseByName(string databaseName, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(Database), Plurality.Single, ServiceOperation.Remove, _targetName, databaseName);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    var index = GetDatabaseIndexByName(databaseName);
                    if (index >= 0)
                    {
                        Settings.Default.XmlDatabases.RemoveAt(index);
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task<ServiceOperationResult> RemoveLogFileByPath(string logFilePath, bool saveOnCompletion = false)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogFile), Plurality.Single, ServiceOperation.Remove, _targetName, logFilePath);
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                    var index = GetLogFileIndexByPath(logFilePath);
                    if (index >= 0)
                    {
                        Settings.Default.XmlLogFiles.RemoveAt(index);
                    }
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    if (saveOnCompletion)
                    {
                        serviceOperationHelper.ServiceOperationResult = SaveSettings();
                    }
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }
        #endregion

        public static void InitializeCollections()
        {
            if (Settings.Default.XmlLogViews == null)
            {
                Settings.Default.XmlLogViews = new StringCollection();
            }
            if (Settings.Default.XmlDatabases == null)
            {
                Settings.Default.XmlDatabases = new StringCollection();
            }
            if (Settings.Default.XmlLogFiles == null)
            {
                Settings.Default.XmlLogFiles = new StringCollection();
            }
        }

        public static async Task<ServiceOperationResult> SaveSettingsAsync()
        {
            var result = new ServiceOperationResult();
            await Task.Run(() =>
            {
                result = SaveSettings();
            });
            return result;
        }

        public static ServiceOperationResult SaveSettings()
        {
            var result = new ServiceOperationResult();
            try
            {
                result.Status = ServiceOperationStatus.Attempting;
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to save user settings.", LogMessageType.Verbose);
                Settings.Default.Save();
                result.Status = ServiceOperationStatus.Succeeded;
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully saved user settings.", LogMessageType.Verbose);
            }
            catch (Exception ex)
            {
                result.Status = ServiceOperationStatus.Failed;
                result.UserFriendlyErrorMessage = $"Failed to save user settings.";
                result.ErrorMessage = $"{result.UserFriendlyErrorMessage} Error message: {ex.Message}";
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(result.ErrorMessage, LogMessageType.Warning);
            }
            return result;
        }

        public static double GetLogViewManagementColumnWidth()
        {
            return Settings.Default.LogViewManagementColumnWidth;
        }

        public static ServiceOperationResult SetLogViewManagementColumnWidth(double value, bool saveOnCompletion = false)
        {
            var result = new ServiceOperationResult(ServiceOperationStatus.Attempting);
            Settings.Default.LogViewManagementColumnWidth = value;
            result.Status = ServiceOperationStatus.Succeeded;
            if (saveOnCompletion)
            {
                result = SaveSettings();
            }
            return result;
        }

        public static double GetLogEntryMessageViewRowHeight()
        {
            return Settings.Default.LogEntryMessageViewRowHeight;
        }

        public static ServiceOperationResult SetLogEntryMessageViewRowHeight(double value, bool saveOnCompletion = false)
        {
            var result = new ServiceOperationResult(ServiceOperationStatus.Attempting);
            Settings.Default.LogEntryMessageViewRowHeight = value;
            result.Status = ServiceOperationStatus.Succeeded;
            if (saveOnCompletion)
            {
                result = SaveSettings();
            }
            return result;
        }

        public static void UpgradeDatabaseSettings()
        {
            SourceCache<Database, string> databasesSourceCache = new SourceCache<Database, string>(database => database.Name);
            LoadDatabasesIntoSourceCache(databasesSourceCache).Wait();
            foreach (var database in databasesSourceCache.Items)
            {
                var name = database.Name;
                RemoveDatabaseByName(name).Wait();
                name = name.Replace(".", "*");
                var newDatabase = new Database(name)
                {
                    IsPinned = database.IsPinned,
                    DateLastOpened = database.DateLastOpened
                };
                AddOrUpdateDatabase(newDatabase).Wait();
            }

            SourceCache<LogView, string> logViewsSourceCache = new SourceCache<LogView, string>(logView => logView.Name);
            LoadLogViewsIntoSourceCache(logViewsSourceCache).Wait();
            foreach (var logView in logViewsSourceCache.Items)
            {
                for (int i = 0; i < logView.DatabaseNames.Count; i++)
                {
                    var databaseName = logView.DatabaseNames[i];
                    if (databaseName.Contains("."))
                    {
                        logView.DatabaseNames[i] = databaseName.Replace(".", "*");
                    }
                }
                AddOrUpdateLogView(logView).Wait();
            }

            Settings.Default.UpgradeDatabaseSettings2 = false;
            SaveSettings();
        }
        #endregion
    }
}
