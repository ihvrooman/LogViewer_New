using AppStandards.Logging;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class DatabaseService
    {
        #region Constants
        public const string DatabaseAndTableName = "EventLogConnectX or EventLogJetX"; //"[SOADB].[dbo].[Local_SSI_ErrorLogDetail]";
        private const string _databaseNameFormat = "'{SQLInstanceName}.{DatabaseName}.{SQLUsername}.{SQLPassword}'";
        private const string _databaseNameFormatNote = "Note: The username and password are only required if using SQL authentication.";
        private const string sqlCommandText = @"SELECT [Code]
      ,[Message]
      ,[MessageDetails]
      ,[EventLevel]
      ,[Timestamp]
      ,[DeviceId]
      ,[Username]
      ,[EventId]
  FROM [JetExApp].[dbo].[EventLogConnectX]
  UNION
  SELECT [Code]
      ,[Message]
      ,[MessageDetails]
      ,[EventLevel]
      ,[Timestamp]
      ,[DeviceId]
      ,[Username]
      ,[EventId]
  FROM [JetExApp].[dbo].[EventLogJetX]
  ORDER BY [Timestamp], [EventId]";
        /*@"
SELECT 
[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[OBJECT_NAME],
[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[Error_Section],
[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[ERROR_MESSAGE],
[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[TimeStamp],
[SOADB].[dbo].[Local_SSI_ErrorSeverityLevel].[Severity_Level_Desc]
FROM[SOADB].[dbo].[Local_SSI_ErrorLogDetail]
WITH (NOLOCK)
INNER JOIN [SOADB].[dbo].[Local_SSI_ErrorSeverityLevel]
ON[SOADB].[dbo].[Local_SSI_ErrorSeverityLevel].Severity_Level_Id = [SOADB].[dbo].[Local_SSI_ErrorLogDetail].Error_Severity_Level
ORDER BY [SOADB].[dbo].[Local_SSI_ErrorLogDetail].[TimeStamp]";*/
        #endregion

        #region Public methods
        public static async Task<ServiceOperationResult> LoadLogEntriesIntoSourceCache(Database database, SourceCache<LogEntry, string> logEntriesSourceCache)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogEntry), Plurality.Plural, ServiceOperation.Load, $"{typeof(Database).Name} {database.Name}");
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                    var numberOfOldLogEntries = logEntriesSourceCache.Keys.Where(k => k.Contains(database.Identifier)).Count();
                    var sqlConnection = new SqlConnection(new AddDatabaseInfo(database.Name).ToConnectionString());
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand(sqlCommandText, sqlConnection))
                    {
                        using (var sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            long logEntryIndex = 0;
                            logEntriesSourceCache.Edit(innerCache =>
                            {
                                while (sqlDataReader.Read())
                                {
                                    if (++logEntryIndex > numberOfOldLogEntries)
                                    {
                                        //If the log entry is new
                                        var newLogEntry = ParseDatabaseLogEntry(sqlDataReader, database.Name, database, LogEntry.GetIdentifier(database.Identifier, logEntryIndex));
                                        newLogEntry.IsNew = true;
                                        innerCache.AddOrUpdate(newLogEntry);
                                    }
                                }
                            });
                        }
                    }
                    sqlConnection.Close();
                    sqlConnection.Dispose();

                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }

        public static async Task RemoveDatabaseRoutine(IDialogCoordinator dialogCoordinator, object dialogContext, SourceCache<Database, string> databasesSourceCache, Database database)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(Database), Plurality.Single, ServiceOperation.Remove, "databases source cache", database.Name);
            var dialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                DefaultButtonFocus = MessageDialogResult.Negative,
            };
            var dialogResult = await dialogCoordinator.ShowMessageAsync(dialogContext, "Remove Database", $"Are you sure that you would like to remove database \"{database.Name}\"?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                serviceOperationHelper.ServiceOperationResult = await SettingsService.RemoveDatabaseByName(database.Name, true);
                if (serviceOperationHelper.ServiceOperationResult.OperationSuceeded)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                            databasesSourceCache.Remove(database);

                            serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                        }
                        catch (Exception ex)
                        {
                            serviceOperationHelper.LogServiceOperation(ex.Message);
                        }
                    });
                }
            }

            if (serviceOperationHelper.ServiceOperationResult.OperationFailed)
            {
                await serviceOperationHelper.ServiceOperationResult.ShowUserErrorMessage(dialogCoordinator, dialogContext);
            }
        }

        public static async Task AddDatabaseRoutine(IDialogCoordinator dialogCoordinator, object dialogContext, SourceCache<Database, string> databasesSourceCache)
        {
            var maxCharLength = 100;
            var keepPrompting = true;
            var dialogSettings1 = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Add",
            };

            while (keepPrompting)
            {
                var newDatabaseName = dialogCoordinator.ShowModalInputExternal(dialogContext, "Add Database", $"Enter the database name in the format {_databaseNameFormat}.{Environment.NewLine + _databaseNameFormatNote}", dialogSettings1);
                dialogSettings1.DefaultText = newDatabaseName;
                if (newDatabaseName == null)
                {
                    keepPrompting = false;
                }
                else if (string.IsNullOrWhiteSpace(newDatabaseName))
                {
                    await dialogCoordinator.ShowMessageAsync(dialogContext, "Invalid Database Name", "The new database name cannot be empty or whitespace.");
                }
                else if (newDatabaseName.Length > maxCharLength)
                {
                    await dialogCoordinator.ShowMessageAsync(dialogContext, "Invalid Database Name", $"The new database name cannot be greater than {maxCharLength} characters.");
                    dialogSettings1.DefaultText = newDatabaseName.Substring(0, maxCharLength);
                }
                else if (!newDatabaseName.Contains(".") || newDatabaseName.Contains("{") || newDatabaseName.Contains("}"))
                {
                    await dialogCoordinator.ShowMessageAsync(dialogContext, "Invalid Database Name", $"The new database name must be in the format {_databaseNameFormat}.{Environment.NewLine + _databaseNameFormatNote}" + Environment.NewLine + "Example: MainServer\\SQLEXPRESS.CustomerInfo");
                }
                else
                {
                    var addDatabaseInfo = new AddDatabaseInfo(newDatabaseName);
                    if (SettingsService.SettingsContainsDatabaseName(addDatabaseInfo.ToFormattedString()))
                    {
                        await dialogCoordinator.ShowMessageAsync(dialogContext, "Database Already Added", $"Database \"{newDatabaseName}\" has already been added.");
                        keepPrompting = false;
                    }
                    else
                    {
                        //Show progress dialog while searching for new database
                        var progressController = await dialogCoordinator.ShowProgressAsync(dialogContext, "Looking for Database", $"Trying to find database \"{newDatabaseName}\"...");
                        progressController.SetIndeterminate();
                        var databaseExists = false;
                        await Task.Run(() =>
                        {
                            databaseExists = TestDatabaseConnection(addDatabaseInfo.ToConnectionString());
                        });
                        await progressController.CloseAsync();

                        //If database can't be found, ask user if they want to continue adding database
                        var dialogSettings = new MetroDialogSettings()
                        {
                            AffirmativeButtonText = "Yes",
                            NegativeButtonText = "No",
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                        };
                        if (!databaseExists && await dialogCoordinator.ShowMessageAsync(dialogContext, "Database Not Found", $"The database \"{newDatabaseName}\" could not be found. Would you still like to add it?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings) != MessageDialogResult.Affirmative)
                        {
                            return;
                        }

                        var newDatabase = new Database(addDatabaseInfo.ToFormattedString());
                        var serviceOperationHelper = new ServiceOperationHelper(typeof(Database), Plurality.Single, ServiceOperation.Add, "databases source cache", addDatabaseInfo.ToFormattedString());

                        await Task.Run(() =>
                        {
                            try
                            {
                                serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                                databasesSourceCache.AddOrUpdate(newDatabase);
                                serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                            }
                            catch (Exception ex)
                            {
                                serviceOperationHelper.LogServiceOperation(ex.Message);
                            }
                        });

                        keepPrompting = false;

                        if (serviceOperationHelper.ServiceOperationResult.OperationSuceeded)
                        {
                            serviceOperationHelper.ServiceOperationResult = await SettingsService.AddOrUpdateDatabase(newDatabase, true);
                        }

                        if (serviceOperationHelper.ServiceOperationResult.OperationFailed)
                        {
                            await serviceOperationHelper.ServiceOperationResult.ShowUserErrorMessage(dialogCoordinator, dialogContext);
                        }
                    }
                }
            }
        }
        #endregion

        #region Private methods
        private static bool TestDatabaseConnection(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static LogEntry ParseDatabaseLogEntry(SqlDataReader sqlDataReader, string computerName, ILogEntriesSource logEntriesSource, string logEntryIdentifier)
        {
            //TODO: Join in device table to get device info
            var logMessageType = ParseDatabaseLogEntryType(sqlDataReader.GetInt32(3).ToString());
            var timeStamp = sqlDataReader.GetDateTime(4).ToUniversalTime();
            var logMessage = $"Code: \"{sqlDataReader["Code"]}\" | Message: \"{sqlDataReader["Message"]}\" | Details: \"{sqlDataReader["MessageDetails"]}\" | DeviceId: \"{sqlDataReader["DeviceId"]}\" | UserId: \"{sqlDataReader["UserId"]}\"";
            /* $"Object: \"{sqlDataReader["OBJECT_NAME"]}\" | Section: \"{sqlDataReader["Error_Section"]}\" | Message: \"{sqlDataReader["ERROR_MESSAGE"]}\"";*/
            return new LogEntry(logMessageType, timeStamp, logMessage, logEntriesSource, logEntryIdentifier, computername: computerName);
        }

        private static LogMessageType ParseDatabaseLogEntryType(string databaseLogEntryType)
        {
            //switch (databaseLogEntryType.ToUpper())
            //{
            //    case ("CRITICAL"):
            //        return LogMessageType.Error;
            //    case ("WARNING"):
            //        return LogMessageType.Warning;
            //    case ("INFORMATIONAL"):
            //        return LogMessageType.Information;
            //    default:
            //        return LogMessageType.Error;
            //}

            switch (databaseLogEntryType)
            {
                case "0":
                    return LogMessageType.Error;
                case "1":
                    return LogMessageType.Error;
                case "2":
                    return LogMessageType.Warning;
                case "3":
                    return LogMessageType.Information;
                case "4":
                    return LogMessageType.Debug;
                default:
                    return LogMessageType.Unknown;
            }
        }
        #endregion
    }
}
