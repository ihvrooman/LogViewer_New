using AppStandards.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    public class ServiceOperationHelper
    {
        #region Properties
        public string ItemType { get; set; }
        public Plurality Plurality { get; set; }
        public ServiceOperation ServiceOperation { get; set; }
        public string TargetName { get; set; }
        public ServiceOperationResult ServiceOperationResult { get; set; }
        public string ItemIdentifier { get; set; } = "";
        #endregion

        #region Constructors
        public ServiceOperationHelper(Type itemType, Plurality plurality, ServiceOperation serviceOperation, string targetName, ServiceOperationResult serviceOperationResult, string itemIdentifier = "")
        {
            ItemType = itemType.Name;
            Plurality = plurality;
            ServiceOperation = serviceOperation;
            TargetName = targetName;
            ServiceOperationResult = serviceOperationResult;
            ItemIdentifier = itemIdentifier;
        }

        public ServiceOperationHelper(Type itemType, Plurality plurality, ServiceOperation serviceOperation, string targetName, string itemIdentifier = "")
        {
            ItemType = itemType.Name;
            Plurality = plurality;
            ServiceOperation = serviceOperation;
            TargetName = targetName;
            ServiceOperationResult = new ServiceOperationResult();
            ItemIdentifier = itemIdentifier;
        }

        public ServiceOperationHelper(string itemType, Plurality plurality, ServiceOperation serviceOperation, string targetName, string itemIdentifier = "")
        {
            ItemType = itemType;
            Plurality = plurality;
            ServiceOperation = serviceOperation;
            TargetName = targetName;
            ServiceOperationResult = new ServiceOperationResult();
            ItemIdentifier = itemIdentifier;
        }
        #endregion

        #region Public methods
        #region Static methods
        public static string LogServiceOperation(Type itemType, Plurality plurality, ServiceOperation serviceOperation, ServiceOperationStatus serviceOperationStatus, string targetName, string itemIdentifier = "", string exceptionMessage = "")
        {
            var logMessageType = serviceOperationStatus == ServiceOperationStatus.Failed ? LogMessageType.Error : LogMessageType.Verbose;
            var exceptionMessageSuffix = serviceOperationStatus == ServiceOperationStatus.Failed ? $"Error message: {exceptionMessage}" : string.Empty;
            var identifier = plurality == Plurality.Single ? $"{itemType.Name} \"{itemIdentifier}\"" : GetPluralIdentifier(itemType);
            var logMessage = $"{GetActionString(serviceOperation, serviceOperationStatus)} {identifier} {GetPrepositionString(serviceOperation)} the {targetName}. {exceptionMessageSuffix}";
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(logMessage, logMessageType);
            return logMessage;
        }

        public static void LogServiceOperation(Type itemType, Plurality plurality, ServiceOperation serviceOperation, ServiceOperationStatus serviceOperationStatus, string targetName, ServiceOperationResult serviceOperationResult, string itemIdentifier = "", string exceptionMessage = "")
        {
            var logMessageType = serviceOperationStatus == ServiceOperationStatus.Failed ? LogMessageType.Error : LogMessageType.Verbose;
            var exceptionMessageSuffix = serviceOperationStatus == ServiceOperationStatus.Failed ? $"Error message: {exceptionMessage}" : string.Empty;
            var identifier = plurality == Plurality.Single ? $"{itemType.Name} \"{itemIdentifier}\"" : GetPluralIdentifier(itemType);
            var operationMessage = $"{GetActionString(serviceOperation, serviceOperationStatus)} {identifier} {GetPrepositionString(serviceOperation)} the {targetName}.";
            var logMessage = $"{operationMessage} {exceptionMessageSuffix}";
            serviceOperationResult.Status = serviceOperationStatus;
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(logMessage, logMessageType);

            if (serviceOperationStatus == ServiceOperationStatus.Failed)
            {
                serviceOperationResult.ErrorMessage = logMessage;
                serviceOperationResult.UserFriendlyErrorMessage = operationMessage;
            }
        }

        public static void LogServiceOperation(string itemType, Plurality plurality, ServiceOperation serviceOperation, ServiceOperationStatus serviceOperationStatus, string targetName, ServiceOperationResult serviceOperationResult, string itemIdentifier = "", string exceptionMessage = "")
        {
            var logMessageType = serviceOperationStatus == ServiceOperationStatus.Failed ? LogMessageType.Error : LogMessageType.Verbose;
            var exceptionMessageSuffix = serviceOperationStatus == ServiceOperationStatus.Failed ? $"Error message: {exceptionMessage}" : string.Empty;
            var identifier = plurality == Plurality.Single ? $"{itemType} \"{itemIdentifier}\"" : GetPluralIdentifier(itemType);
            var operationMessage = $"{GetActionString(serviceOperation, serviceOperationStatus)} {identifier} {GetPrepositionString(serviceOperation)} the {targetName}.";
            var logMessage = $"{operationMessage} {exceptionMessageSuffix}";
            serviceOperationResult.Status = serviceOperationStatus;
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(logMessage, logMessageType);

            if (serviceOperationStatus == ServiceOperationStatus.Failed)
            {
                serviceOperationResult.ErrorMessage = logMessage;
                serviceOperationResult.UserFriendlyErrorMessage = operationMessage;
            }
        }
        #endregion

        #region Instance methods
        public void LogServiceOperation(ServiceOperationStatus serviceOperationStatus)
        {
            LogServiceOperation(ItemType, Plurality, ServiceOperation, serviceOperationStatus, TargetName, ServiceOperationResult, ItemIdentifier);
        }

        public void LogServiceOperation(string exceptionMessage)
        {
            LogServiceOperation(ItemType, Plurality, ServiceOperation, ServiceOperationStatus.Failed, TargetName, ServiceOperationResult, ItemIdentifier, exceptionMessage);
        }
        #endregion
        #endregion

        #region Private methods
        private static string GetActionString(ServiceOperation serviceOperation, ServiceOperationStatus serviceOperationStatus)
        {
            var actionString = string.Empty;
            var prefixString = "Successfully ";
            if (serviceOperationStatus == ServiceOperationStatus.Attempting || serviceOperationStatus == ServiceOperationStatus.Failed)
            {
                if (serviceOperationStatus == ServiceOperationStatus.Attempting)
                {
                    prefixString = "Attempting to ";
                }
                else if (serviceOperationStatus == ServiceOperationStatus.Failed)
                {
                    prefixString = "Failed to ";
                }

                switch (serviceOperation)
                {
                    case ServiceOperation.Add:
                        actionString = "add";
                        break;
                    case ServiceOperation.Update:
                        actionString = "update";
                        break;
                    case ServiceOperation.AddOrUpdate:
                        actionString = "add or update";
                        break;
                    case ServiceOperation.Remove:
                        actionString = "remove";
                        break;
                    case ServiceOperation.Load:
                        actionString = "load";
                        break;
                    case ServiceOperation.Reload:
                        actionString = "reload";
                        break;
                    default:
                        break;
                }
            }
            else if (serviceOperationStatus == ServiceOperationStatus.Succeeded)
            {

                switch (serviceOperation)
                {
                    case ServiceOperation.Add:
                        actionString = "added";
                        break;
                    case ServiceOperation.Update:
                        actionString = "updated";
                        break;
                    case ServiceOperation.AddOrUpdate:
                        actionString = "added or updated";
                        break;
                    case ServiceOperation.Remove:
                        actionString = "removed";
                        break;
                    case ServiceOperation.Load:
                        actionString = "loaded";
                        break;
                    case ServiceOperation.Reload:
                        actionString = "reloaded";
                        break;
                    default:
                        break;
                }
            }
            return prefixString + actionString;
        }

        private static string GetPrepositionString(ServiceOperation settingsAction)
        {
            switch (settingsAction)
            {
                case ServiceOperation.Add:
                    return "to";
                case ServiceOperation.Update:
                    return "in";
                case ServiceOperation.AddOrUpdate:
                    return "in";
                case ServiceOperation.Remove:
                    return "from";
                case ServiceOperation.Load:
                    return "from";
                case ServiceOperation.Reload:
                    return "from";
                default:
                    return "to";
            }
        }

        private static string GetPluralIdentifier(Type itemType)
        {
            var pluralIdentifier = itemType.Name;
            if (pluralIdentifier.EndsWith("y"))
            {
                pluralIdentifier = pluralIdentifier.Remove(pluralIdentifier.Length - 1, 1) + "ies";
            }
            else
            {
                pluralIdentifier += "s";
            }
            return pluralIdentifier;
        }

        private static string GetPluralIdentifier(string itemType)
        {
            var pluralIdentifier = itemType;
            if (pluralIdentifier.EndsWith("y"))
            {
                pluralIdentifier = pluralIdentifier.Remove(pluralIdentifier.Length - 1, 1) + "ies";
            }
            else
            {
                pluralIdentifier += "s";
            }
            return pluralIdentifier;
        }
        #endregion
    }

    public enum ServiceOperationStatus
    {
        /// <summary>
        /// Indicates that the status of the service operation cannot be determined.
        /// </summary>
        Indeterminate,
        /// <summary>
        /// Indicates service is attempting the operation.
        /// </summary>
        Attempting,
        /// <summary>
        /// Indicates that the service operation completed successfully.
        /// </summary>
        Succeeded,
        /// <summary>
        /// Indicates that the service operation failed.
        /// </summary>
        Failed,
    }

    public enum ServiceOperation
    {
        Add,
        Update,
        AddOrUpdate,
        Remove,
        Load,
        Reload,
    }
}
