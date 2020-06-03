using AppStandards;
using AppStandards.Logging;
using LogViewer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace LogViewer.Helpers
{
    /// <summary>
    /// A static class containing app-specific information.
    /// </summary>
    public static class AppInfo
    {
        /// <summary>
        /// A string containing two new line characters.
        /// </summary>
        private static string _twoNewLines = Environment.NewLine + Environment.NewLine;
        /// <summary>
        /// An instance of the <see cref="BaseAppInfo"/> class which contains app-specific information. This object is used for many <see cref="AppStandards"/> operations.
        /// </summary>
        public static BaseAppInfo BaseAppInfo { get; private set; } = new BaseAppInfo();
        /// <summary>
        /// A description of the application.
        /// </summary>
        public static string Description { get; private set; } = $"An application used to view log files and database log tables.{Environment.NewLine + Environment.NewLine}Currently supported log file types include:{Environment.NewLine}    - Proficy client log files (ProficyClient.log){Environment.NewLine}    - Plant apps sdk log files (PlantApps.SDK.log){Environment.NewLine}    - MES core service log files (MESCore Service.log){Environment.NewLine}    - SOA server log files (SOAServer.log){Environment.NewLine}    - WY custom application log files (LogViewer and CopyDllsToProficy logs){Environment.NewLine + Environment.NewLine}Currently supported database log tables:{Environment.NewLine}    - Local_SSI_ErrorLogDetail";
        //TODO: Update description and credits.
        public const string Credits = "Icons downloaded from: 'https://icons8.com/icons'.";
        /// <summary>
        /// A string containing information displayed in the About dialog.
        /// </summary>
        public static string AboutInformation { get; private set; } = $"Application Name: {BaseAppInfo.AppName + _twoNewLines}Version Number: {BaseAppInfo.VersionNumber + _twoNewLines}Description: {Description + _twoNewLines}Credits: {Credits}";
        public static string LocalLogFileFolderPath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"\\{BaseAppInfo.Company}\\{BaseAppInfo.AppName}\\LocalLogFileCopies\\";
        public const string NAString = "N/A";
        public const string LastOpenedTimeFormatString = @"MM/dd/yyyy HH:mm:ss:ffff";
    }

    /// <summary>
    /// An instance class containing app-specific information and components that inherits from <see cref="IAppInfo"/>.
    /// </summary>
    public class BaseAppInfo : IAppInfo
    {
        #region Fields
        /// <summary>
        /// The application <see cref="AppStandards.Logging.Log"/>.
        /// </summary>
        private Log _log;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the application obtained from the assembly.
        /// </summary>
        public string AppName { get; } = Assembly.GetExecutingAssembly().GetName().Name;
        /// <summary>
        /// The application version number obtained from the assembly.
        /// </summary>
        public string VersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        /// <summary>
        /// The company which developed the application obtained from the assembly.
        /// </summary>
        public string Company { get; } = "Weyerhaeuser";
        /// <summary>
        /// The application <see cref="AppStandards.Logging.Log"/>.
        /// </summary>
        public Log Log
        {
            get
            {
                if (_log == null)
                {
                    _log = new Log(Settings.Default.OnlineLogFolderPath, Settings.Default.OfflineLogFolderPath, AppName);
                }
                return _log;
            }
        }
        #endregion

        #region Constructor
        public BaseAppInfo()
        {
            Routines.Startup(this);
        }
        #endregion
    }
}
