using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class LogViewService
    {
        #region Public methods
        public static async Task RenameLogViewRoutine(IDialogCoordinator dialogCoordinator, object dialogContext, SourceCache<LogView, string> logViewsSourceCache, LogView logView)
        {
            var result = new ServiceOperationResult();
            var maxCharLength = 100;
            var oldLogViewName = logView.Name;
            var keepPrompting = true;
            var dialogSettings1 = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Rename",
                DefaultText = oldLogViewName,
            };

            while (keepPrompting)
            {
                var newLogViewName = await dialogCoordinator.ShowInputAsync(dialogContext, "Rename Log View", "New name:", dialogSettings1);
                if (newLogViewName == null)
                {
                    keepPrompting = false;
                }
                else if (string.IsNullOrWhiteSpace(newLogViewName))
                {
                    await dialogCoordinator.ShowMessageAsync(dialogContext, "Invalid Log View Name", "The new log view name cannot be null or whitespace.");
                }
                else if (newLogViewName.Length > maxCharLength)
                {
                    await dialogCoordinator.ShowMessageAsync(dialogContext, "Invalid Log View Name", $"The new log view name cannot be greater than {maxCharLength} characters.");
                    dialogSettings1.DefaultText = newLogViewName.Substring(0, maxCharLength);
                }
                else if (logViewsSourceCache.Keys.Contains(newLogViewName))
                {
                    var dialogSettings2 = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Overwrite",
                        NegativeButtonText = "Pick a new name",
                        DefaultButtonFocus = MessageDialogResult.Negative,
                    };
                    var dialogResult = await dialogCoordinator.ShowMessageAsync(dialogContext, "Log View Already Exists", $"There is already a log view with the name \"{newLogViewName}\". Would you like to overwrite the existing log view?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings2);
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        //Remove the old log view and the log view which will be replaced, rename the log view, and add it back in
                        await Task.Run(() =>
                        {
                            try
                            {
                                result.Status = ServiceOperationStatus.Attempting;
                                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to rename {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\" which will overwrite existing log view with name \"{newLogViewName}\".", LogMessageType.Verbose);
                                logViewsSourceCache.Edit(innerCache =>
                                {
                                    innerCache.RemoveKey(newLogViewName);
                                    innerCache.RemoveKey(oldLogViewName);
                                    logView.Name = newLogViewName;
                                    logView.IsEdited = true;
                                    innerCache.AddOrUpdate(logView);
                                });
                                result.Status = ServiceOperationStatus.Succeeded;
                                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully renamed {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\".", LogMessageType.Verbose);
                            }
                            catch (Exception ex)
                            {
                                result.Status = ServiceOperationStatus.Failed;
                                result.UserFriendlyErrorMessage = $"{AppInfo.BaseAppInfo.AppName} failed to rename {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\".";
                                result.ErrorMessage = result.UserFriendlyErrorMessage + $" {ex.Message}";
                                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(result.ErrorMessage, LogMessageType.Error);
                            }
                        });

                        if (result.OperationSuceeded)
                        {
                            //Remove old log view from settings
                            result = await SettingsService.RemoveLogViewByName(oldLogViewName);
                        }

                        keepPrompting = false;
                    }
                }
                else
                {
                    //Remove the old log view, rename it, and add it back in
                    await Task.Run(() =>
                    {
                        try
                        {
                            result.Status = ServiceOperationStatus.Attempting;
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to rename {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\".", LogMessageType.Verbose);
                            logViewsSourceCache.Edit(innerCache =>
                            {
                                innerCache.RemoveKey(oldLogViewName);
                                logView.Name = newLogViewName;
                                logView.IsEdited = true;
                                innerCache.AddOrUpdate(logView);
                            });
                            result.Status = ServiceOperationStatus.Succeeded;
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully renamed {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\".", LogMessageType.Verbose);
                        }
                        catch (Exception ex)
                        {
                            result.Status = ServiceOperationStatus.Failed;
                            result.UserFriendlyErrorMessage = $"{AppInfo.BaseAppInfo.AppName} failed to rename {typeof(LogView).Name} from \"{oldLogViewName}\" to \"{newLogViewName}\".";
                            result.ErrorMessage = result.UserFriendlyErrorMessage + $" {ex.Message}";
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(result.ErrorMessage, LogMessageType.Error);
                        }
                    });

                    if (result.OperationSuceeded)
                    {
                        //Remove old log view from settings
                        result = await SettingsService.RemoveLogViewByName(oldLogViewName);
                    }

                    keepPrompting = false;
                }
            }

            if (result.OperationFailed)
            {
                await result.ShowUserErrorMessage(dialogCoordinator, dialogContext);
            }
            else
            {
                await Task.Delay(100);
                Mediator.NotifyColleagues(MediatorMessages.RequestSetSelectedLogView, logView);
            }
        }

        public static async Task DeleteLogViewRoutine(IDialogCoordinator dialogCoordinator, object dialogContext, SourceCache<LogView, string> logViewsSourceCache, LogView logView)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogView), Plurality.Single, ServiceOperation.Remove, "log views source cache", logView.Name);
            var dialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                DefaultButtonFocus = MessageDialogResult.Negative,
            };

            var dialogResult = await dialogCoordinator.ShowMessageAsync(dialogContext, "Permanently Delete Log View", $"Are you sure that you would like to permanently delete log view \"{logView.Name}\"?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
            if (dialogResult == MessageDialogResult.Affirmative)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);
                        logViewsSourceCache.Remove(logView);
                        serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                    }
                    catch (Exception ex)
                    {
                        serviceOperationHelper.LogServiceOperation(ex.Message);
                    }
                });

                if (serviceOperationHelper.ServiceOperationResult.OperationSuceeded)
                {
                    serviceOperationHelper.ServiceOperationResult = await SettingsService.RemoveLogViewByName(logView.Name, true);
                }
            }

            if (serviceOperationHelper.ServiceOperationResult.OperationFailed)
            {
                await serviceOperationHelper.ServiceOperationResult.ShowUserErrorMessage(dialogCoordinator, dialogContext);
            }
        }
        #endregion
    }
}
