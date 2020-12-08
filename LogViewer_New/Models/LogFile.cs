using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.Services;
using LogViewer.ViewModels;

namespace LogViewer.Models
{
    public class LogFile : OpenableObject, ILogEntriesSource
    {
        #region Constants
        private const string logFileXmlNodeName = "LogFile";
        private const string logFileXmlNetworkPathAttrName = "NetworkPath";
        #endregion

        #region Fields
        private volatile int _uniqueId = LogFileIdManager.GetNextAvailableId();
        private bool _refreshIndicator;
        private FileSystemWatcher _fileSystemWatcher;
        private DateTime _lastWriteTime = DateTime.MinValue;
        private long _sizeInBytes = -1;
        private bool _autoRefresh;
        private volatile bool _refreshing;
        private volatile bool _refreshPending;
        private FileInfo _networkFile;
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="FileInfo"/> which represents the network copy of the log file.
        /// </summary>
        public FileInfo NetworkFile { get { return _networkFile; } set { _networkFile = value; RaisePropertyChangedEvent(); Identifier = NetworkFile?.FullName; } }
        public string Source { get { return NetworkFile?.FullName; } }
        /// <summary>
        /// Used to force a refresh of the <see cref="LogViewManagementViewModel.DatabasesSourceCache"/> and <see cref="LogViewManagementViewModel.LogFilesSourceCache"/> collections.
        /// </summary>
        public bool RefreshIndicator { get { return _refreshIndicator; } private set { _refreshIndicator = value; RaisePropertyChangedEvent(); } }
        public string ComputerName { get; set; }
        public TimeSpan ComputerOffsetFromLocalTime { get; private set; } = new TimeSpan(0, 0, 0);
        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            set
            {
                _autoRefresh = value;
                if (AutoRefresh)
                {
                    File_Changed(null, null);
                }
            }
        }
        public DateTime LastWriteTime { get { return _lastWriteTime; } set { _lastWriteTime = value; RaisePropertyChangedEvent(); } }
        public long SizeInBytes { get { return _sizeInBytes; } set { _sizeInBytes = value; RaisePropertyChangedEvent(); } }
        public bool Refreshing { get { return _refreshing; } set { _refreshing = value; } }
        public bool RefreshPending { get { return _refreshPending; } set { _refreshPending = value; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="LogFile"/> with the specified path.
        /// </summary>
        /// <param name="path">The path of the <see cref="LogFile"/>.</param>
        public LogFile(string path)
        {
            NetworkFile = new FileInfo(path);
            DateLastOpened = DateTime.Now;
            InitializeAsync();
        }

        public LogFile(string xmlLogFile, bool isNew = false)
        {
            BuildLogViewFromXml(xmlLogFile);
            InitializeAsync();
        }
        #endregion

        #region Public methods
        public void Open()
        {
            RefreshIndicator = !RefreshIndicator;
            if (!IsOpen)
            {
                //Could be opened more than once.
                IsOpen = true;
                DateLastOpened = DateTime.Now;
            }
        }

        /// <summary>
        /// Exports the <see cref="LogFile"/> to xml format so that it can be saved.
        /// </summary>
        /// <returns>The xml representation of the <see cref="LogFile"/>.</returns>
        public string ExportToXml()
        {
            var xmlDocument = new XmlDocument();

            var xmlLogFileNode = ExportToXmlNode(xmlDocument);
            xmlDocument.AppendChild(xmlLogFileNode);

            return xmlDocument.OuterXml;
        }

        /// <summary>
        /// Exports the <see cref="LogFile"/> to an xml node so that it can be saved.
        /// </summary>
        /// <returns>The xml node representation of the <see cref="LogFile"/>.</returns>
        public XmlNode ExportToXmlNode(XmlDocument xmlDocument)
        {
            //Create log file node, add it's attributes, and add it to the xml doc
            var xmlLogFileNode = xmlDocument.CreateElement(logFileXmlNodeName);
            xmlLogFileNode.SetAttribute(logFileXmlNetworkPathAttrName, NetworkFile.FullName);
            xmlLogFileNode.SetAttribute(CommonXmlAttributeNames.DateLastOpened, DateLastOpenedString);
            xmlLogFileNode.SetAttribute(CommonXmlAttributeNames.IsPinned, IsPinned.ToString());

            return xmlLogFileNode;
        }

        public static XmlNode CreateXmlNodeFromPath(string path, XmlDocument xmlDocument)
        {
            //Create log file node, add it's attributes, and add it to the xml doc
            var xmlLogFileNode = xmlDocument.CreateElement(logFileXmlNodeName);
            xmlLogFileNode.SetAttribute(logFileXmlNetworkPathAttrName, path);

            return xmlLogFileNode;
        }

        public void RequestRefresh()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Refreshing)
            {
                RefreshPending = true;
                return;
            }

            Refreshing = true;
            Mediator.NotifyColleagues(MediatorMessages.RequestRefreshLogFile, this);
        }
        #endregion

        #region Private methods
        private void BuildLogViewFromXml(string xmlLogFile)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlLogFile);

                //Set the log view name and last opened date
                NetworkFile = new FileInfo(xmlDocument.DocumentElement.Attributes[0].Value);
                DateLastOpened = DateTime.ParseExact(xmlDocument.DocumentElement.Attributes[1].Value, AppInfo.LastOpenedTimeFormatString, CultureInfo.InvariantCulture);
                IsPinned = Convert.ToBoolean(xmlDocument.DocumentElement.Attributes[2].Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create {GetType().Name} from xml. Xml parameter: \"{xmlLogFile}\" | Inner exception message: {ex.Message}");
            }
        }

        private void InitializeAsync()
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Initializing log file with network path \"{NetworkFile.FullName}\".", LogMessageType.Verbose);

                //Get file info attributes needed for displaying info
                if (NetworkFile.Exists)
                {
                    LastWriteTime = NetworkFile.LastWriteTime;
                    SizeInBytes = NetworkFile.Length;
                }
                else
                {
                    LastWriteTime = DateTime.MinValue;
                    SizeInBytes = -1;
                }

                //Extract computer name from log folder path
                var chars = NetworkFile.Directory.FullName.ToCharArray();
                var startingIndex = 0;
                if (chars.Length > 2 && chars[0] == '\\' && chars[1] == '\\')
                {
                    startingIndex = 2;
                    for (int i = startingIndex; i < NetworkFile.Directory.FullName.Length; i++)
                    {
                        if (chars[i] == '\\')
                        {
                            ComputerName = NetworkFile.Directory.FullName.Substring(startingIndex, i - startingIndex);
                            break;
                        }
                    }
                }
                else
                {
                    ComputerName = Environment.MachineName;
                }

                //Get computer offset from local time
                try
                {
                    ComputerOffsetFromLocalTime = ComputerHelper.GetRemoteComputerOffsetFromLocalTime(ComputerName);
                }
                catch (Exception ex)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get computer \"{ComputerName}\"'s offset from local time. Error message: {ex.Message}", LogMessageType.Warning);
                }

                //Initialize filesystem watcher
                if (_fileSystemWatcher == null && NetworkFile.Exists)
                {
                    _fileSystemWatcher = new FileSystemWatcher(NetworkFile.Directory.FullName, NetworkFile.Name) { EnableRaisingEvents = true, };
                    _fileSystemWatcher.Changed += File_Changed;
                }

                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Log file with network path \"{NetworkFile.FullName}\" initialized.", LogMessageType.Verbose);
            });
        }

        private void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (AutoRefresh)
            {
                //TODO: Have to figure out what to do when log view closes or log file is removed from log view. Have to consider what happens if it's removed from one log view but is open in another. Or if auto refresh is enabled one one view that has certain log file but auto refresh is disabled on another log view that has same log file. Logic applies to Database too.
                RequestRefresh();
            }
        }
        #endregion
    }
}