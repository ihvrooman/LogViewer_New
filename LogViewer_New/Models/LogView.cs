using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogViewer.Models
{
    public class LogView : OpenableObject
    {
        #region Constants
        #region Log view
        private const string logViewXmlNodeName = "LogView";
        #endregion

        #region Log files
        private const string logFilesXmlNodeName = "LogFiles";
        #endregion

        #region Databases
        private const string databasesXmlNodeName = "Databases";
        #endregion

        #region Filter options
        private const string filterOptionsXmlNodeName = "FilterOptions";

        #region Log message type
        private const string filterOptionsXmlIncludeErrorsAttrName = "IncludeErrors";
        private const string filterOptionsXmlIncludeWarningsAttrName = "IncludeWarnings";
        private const string filterOptionsXmlIncludeInformationAttrName = "IncludeInformation";
        private const string filterOptionsXmlIncludeVerboseAttrName = "IncludeVerbose";
        private const string filterOptionsXmlIncludeDebugAttrName = "IncludeDebug";
        private const string filterOptionsXmlIncludeDebugErrorsAttrName = "IncludeDebugErrors";
        private const string filterOptionsXmlIncludeDebugWarningsAttrName = "IncludeDebugWarnings";
        private const string filterOptionsXmlIncludeDebugInformationAttrName = "IncludeDebugInformation";
        private const string filterOptionsXmlIncludeDebugVerboseAttrName = "IncludeDebugVerbose";
        private const string filterOptionsXmlIncludeUnknownAttrName = "IncludeUnknown";
        #endregion

        #region Date range
        private const string filterOptionsXmlUseQuickDateRangeAtrrName = "UseQuickDateRange";
        private const string filterOptionsXmlSelectedQuickDateRangeNameAttrName = "SelectedQuickDateRangeName";

        private const string filterOptionsXmlMinDayAttrName = "MinDay";
        private const string filterOptionsXmlMinHourAttrName = "MinHour";
        private const string filterOptionsXmlMinMinuteAttrName = "MinMinute";
        private const string filterOptionsXmlMinSecondAttrName = "MinSecond";
        private const string filterOptionsXmlMinMilliSecondAttrName = "MinMillisecond";

        private const string filterOptionsXmlMaxDayAttrName = "MaxDay";
        private const string filterOptionsXmlMaxHourAttrName = "MaxHour";
        private const string filterOptionsXmlMaxMinuteAttrName = "MaxMinute";
        private const string filterOptionsXmlMaxSecondAttrName = "MaxSecond";
        private const string filterOptionsXmlMaxMilliSecondAttrName = "MaxMillisecond";
        #endregion

        #region Other filter options
        private const string filterOptionsXmlUsernameAttrName = "Username";
        private const string filterOptionsXmlComputernameAttrName = "Computername";
        private const string filterOptionsXmlSearchTermAttrName = "SearchTerm";
        private const string filterOptionsXmlExclusionTermAttrName = "ExclusionTerm";
        #endregion
        #endregion

        #region Settings
        private const string settingsXmlNodeName = "Settings";
        private const string settingsAutoRefreshAttrName = "AutoRefresh";
        private const string settingsHighlightNewLogEntriesAttrName = "HighlightNewLogEntries";
        #endregion
        #endregion

        #region Fields
        private string _name;
        private static int _suffixInt = 0;
        private bool _isEdited;
        private bool _isLoading;
        #endregion

        #region Properties
        public string Name { get { return _name; } set { _name = value; RaisePropertyChangedEvent(); Identifier = Name; } }
        public bool IsNew { get; private set; }
        public FilterOptions FilterOptions { get; private set; } = new FilterOptions();
        public LogViewSettings Settings { get; set; } = new LogViewSettings();
        public ObservableCollection<string> LogFilePaths { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<string> DatabaseNames { get; private set; } = new ObservableCollection<string>();
        public bool IsEdited { get { return _isEdited; } set { _isEdited = value; RaisePropertyChangedEvent(); } }
        public bool IsLoading { get { return _isLoading; } set { _isLoading = value; RaisePropertyChangedEvent(); } }
        public List<string> AllLogFilePathsAndDatabaseNames { get; private set; } = new List<string>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="LogView"/>.
        /// </summary>
        /// <param name="database">The <see cref="Database"/> being opened in the <see cref="LogView"/>.</param>
        public LogView(Database database)
        {
            IsNew = true;
            IsEdited = true;
            SetName(database.Name);
            SubscribeToEvents();
            DatabaseNames.Add(database.Name);
        }

        /// <summary>
        /// Creates a new <see cref="LogView"/>.
        /// </summary>
        /// <param name="database">The <see cref="LogFile"/> being opened in the <see cref="LogView"/>.</param>
        public LogView(LogFile logFile)
        {
            IsNew = true;
            IsEdited = true;
            SetName(logFile.NetworkFile.FullName);
            SubscribeToEvents();
            LogFilePaths.Add(logFile.NetworkFile?.FullName);
        }

        /// <summary>
        /// Creates a new <see cref="LogView"/> from the xml <see cref="LogView"/> representation.
        /// </summary>
        /// <param name="xmlLogView">The xml representation of the <see cref="LogView"/>.</param>
        /// <param name="resetDateLastOpened">Indicates whether or not the <see cref="DateLastOpened"/> property should be set to the current date. Note: This should only be true if the user is opening the <see cref="LogView"/>.</param>
        /// <param name="isNew">Indicates whether or not this <see cref="LogView"/> is newly created.</param>
        public LogView(string xmlLogView)
        {
            SubscribeToEvents();
            UpdateFromXml(xmlLogView);
            IsEdited = false;
        }
        #endregion

        #region Public methods
        public void UpdateFromXml(string xmlLogView)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlLogView);

                //Set the log view name and last opened date
                Name = xmlDocument.DocumentElement.Attributes[0].Value;
                DateLastOpened = DateTime.ParseExact(xmlDocument.DocumentElement.Attributes[1].Value, AppInfo.LastOpenedTimeFormatString, CultureInfo.InvariantCulture);
                IsPinned = Convert.ToBoolean(xmlDocument.DocumentElement.Attributes[2].Value);

                // Add the log files
                XmlNode logFilesNode = xmlDocument.GetElementsByTagName(logFilesXmlNodeName)[0];
                LogFilePaths.Clear();
                foreach (XmlNode logFileNode in logFilesNode.ChildNodes)
                {
                    var networkFilePath = logFileNode.Attributes[0].Value.ToString();
                    LogFilePaths.Add(networkFilePath);
                }

                // Add the databases
                XmlNode databasesNode = xmlDocument.GetElementsByTagName(databasesXmlNodeName)[0];
                DatabaseNames.Clear();
                foreach (XmlNode databaseNode in databasesNode.ChildNodes)
                {
                    var name = databaseNode.Attributes[0].Value.ToString();
                    DatabaseNames.Add(name);
                }

                //Add the filter options
                XmlNode filterOptionsNode = xmlDocument.GetElementsByTagName(filterOptionsXmlNodeName)[0];
                foreach (XmlAttribute attribute in filterOptionsNode.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case filterOptionsXmlIncludeErrorsAttrName:
                            FilterOptions.IncludeErrors = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeWarningsAttrName:
                            FilterOptions.IncludeWarnings = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeInformationAttrName:
                            FilterOptions.IncludeInformation = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeVerboseAttrName:
                            FilterOptions.IncludeVerbose = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeDebugAttrName:
                            FilterOptions.IncludeDebug = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeDebugErrorsAttrName:
                            FilterOptions.IncludeDebugErrors = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeDebugWarningsAttrName:
                            FilterOptions.IncludeDebugWarnings = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeDebugInformationAttrName:
                            FilterOptions.IncludeDebugInformation = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeDebugVerboseAttrName:
                            FilterOptions.IncludeDebugVerbose = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlIncludeUnknownAttrName:
                            FilterOptions.IncludeUnknown = Convert.ToBoolean(attribute.Value);
                            break;
                        case filterOptionsXmlUseQuickDateRangeAtrrName:
                            FilterOptions.UseQuickDateRange = Convert.ToBoolean(attribute.Value);
                            FilterOptions.SpecifyDateRange = !FilterOptions.UseQuickDateRange;
                            break;
                        case filterOptionsXmlSelectedQuickDateRangeNameAttrName:
                            FilterOptions.SelectedQuickDateRange = FilterOptions.QuickDateRanges.FirstOrDefault(qdr => qdr.Name == attribute.Value);
                            break;
                        case filterOptionsXmlMinDayAttrName:
                            FilterOptions.MinDay = Convert.ToDateTime(attribute.Value);
                            break;
                        case filterOptionsXmlMinHourAttrName:
                            FilterOptions.MinHour = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMinMinuteAttrName:
                            FilterOptions.MinMinute = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMinSecondAttrName:
                            FilterOptions.MinSecond = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMinMilliSecondAttrName:
                            FilterOptions.MinMillisecond = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMaxDayAttrName:
                            FilterOptions.MaxDay = Convert.ToDateTime(attribute.Value);
                            break;
                        case filterOptionsXmlMaxHourAttrName:
                            FilterOptions.MaxHour = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMaxMinuteAttrName:
                            FilterOptions.MaxMinute = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMaxSecondAttrName:
                            FilterOptions.MaxSecond = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlMaxMilliSecondAttrName:
                            FilterOptions.MaxMillisecond = Convert.ToInt32(attribute.Value);
                            break;
                        case filterOptionsXmlUsernameAttrName:
                            FilterOptions.FilterUsername = attribute.Value;
                            break;
                        case filterOptionsXmlComputernameAttrName:
                            FilterOptions.FilterComputername = attribute.Value;
                            break;
                        case filterOptionsXmlSearchTermAttrName:
                            FilterOptions.SearchTerm = attribute.Value;
                            break;
                        case filterOptionsXmlExclusionTermAttrName:
                            FilterOptions.ExclusionTerm = attribute.Value;
                            break;
                        default:
                            break;
                    }
                }

                if (FilterOptions.UseQuickDateRange)
                {
                    FilterOptions.LoadQuickDateRange(FilterOptions.SelectedQuickDateRange);
                }

                //Add the settings
                XmlNode settingsNode = xmlDocument.GetElementsByTagName(settingsXmlNodeName)[0];
                foreach (XmlAttribute attribute in settingsNode.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case settingsAutoRefreshAttrName:
                            Settings.AutoRefresh = Convert.ToBoolean(attribute.Value);
                            break;
                        case settingsHighlightNewLogEntriesAttrName:
                            Settings.HighlightNewLogEntries = Convert.ToBoolean(attribute.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update {GetType().Name} from xml. Xml parameter: \"{xmlLogView}\" | Inner exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the <see cref="LogView"/> to xml format so that it can be saved.
        /// </summary>
        /// <returns>The xml representation of the <see cref="LogView"/>.</returns>
        public string ExportToXml()
        {
            try
            {
                var xmlDocument = new XmlDocument();

                //Create log view node, and add it's attributes
                var xmlLogViewNode = xmlDocument.CreateElement(logViewXmlNodeName);
                xmlLogViewNode.SetAttribute(CommonXmlAttributeNames.Name, Name);
                xmlLogViewNode.SetAttribute(CommonXmlAttributeNames.DateLastOpened, DateLastOpenedString);
                xmlLogViewNode.SetAttribute(CommonXmlAttributeNames.IsPinned, IsPinned.ToString());
                xmlDocument.AppendChild(xmlLogViewNode);

                //Create the xml log files list and add it to the log view node
                var xmlLogFilesList = xmlDocument.CreateElement(logFilesXmlNodeName);
                xmlLogViewNode.AppendChild(xmlLogFilesList);

                //Add the log files to the xml log files list
                foreach (var logFilePath in LogFilePaths)
                {
                    xmlLogFilesList.AppendChild(LogFile.CreateXmlNodeFromPath(logFilePath, xmlDocument));
                }

                //Create the xml databases list and add it to the log view node
                var xmlDatabasesList = xmlDocument.CreateElement(databasesXmlNodeName);
                xmlLogViewNode.AppendChild(xmlDatabasesList);

                //Add the databases to the xml databases list
                foreach (var databaseName in DatabaseNames)
                {
                    xmlDatabasesList.AppendChild(Database.CreateXmlNodeFromName(databaseName, xmlDocument));
                }

                //Create the xml filter options node, set its attributes, and add it to the log view node
                var xmlFilterOptions = xmlDocument.CreateElement(filterOptionsXmlNodeName);
                xmlLogViewNode.AppendChild(xmlFilterOptions);

                #region Log message type attributes
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeErrorsAttrName, FilterOptions.IncludeErrors.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeWarningsAttrName, FilterOptions.IncludeWarnings.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeInformationAttrName, FilterOptions.IncludeInformation.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeVerboseAttrName, FilterOptions.IncludeVerbose.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeDebugAttrName, FilterOptions.IncludeDebug.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeDebugErrorsAttrName, FilterOptions.IncludeDebugErrors.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeDebugWarningsAttrName, FilterOptions.IncludeDebugWarnings.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeDebugInformationAttrName, FilterOptions.IncludeDebugInformation.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeDebugVerboseAttrName, FilterOptions.IncludeDebugVerbose.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlIncludeUnknownAttrName, FilterOptions.IncludeUnknown.ToString());
                #endregion

                #region Date range attributes
                xmlFilterOptions.SetAttribute(filterOptionsXmlUseQuickDateRangeAtrrName, FilterOptions.UseQuickDateRange.ToString());
                var selectedQuickDateRangeName = FilterOptions.SelectedQuickDateRange == null ? "NULL" : FilterOptions.SelectedQuickDateRange.Name;
                xmlFilterOptions.SetAttribute(filterOptionsXmlSelectedQuickDateRangeNameAttrName, selectedQuickDateRangeName);

                xmlFilterOptions.SetAttribute(filterOptionsXmlMinDayAttrName, FilterOptions.MinDay.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMinHourAttrName, FilterOptions.MinHour.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMinMinuteAttrName, FilterOptions.MinMinute.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMinSecondAttrName, FilterOptions.MinSecond.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMinMilliSecondAttrName, FilterOptions.MinMillisecond.ToString());

                xmlFilterOptions.SetAttribute(filterOptionsXmlMaxDayAttrName, FilterOptions.MaxDay.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMaxHourAttrName, FilterOptions.MaxHour.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMaxMinuteAttrName, FilterOptions.MaxMinute.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMaxSecondAttrName, FilterOptions.MaxSecond.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlMaxMilliSecondAttrName, FilterOptions.MaxMillisecond.ToString());
                #endregion

                #region Other filter options attributes
                xmlFilterOptions.SetAttribute(filterOptionsXmlUsernameAttrName, FilterOptions.FilterUsername.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlComputernameAttrName, FilterOptions.FilterComputername.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlSearchTermAttrName, FilterOptions.SearchTerm.ToString());
                xmlFilterOptions.SetAttribute(filterOptionsXmlExclusionTermAttrName, FilterOptions.ExclusionTerm.ToString());
                #endregion

                //Create the settings node, set it's attributes, and add it to the log view node
                var xmlSettingsNode = xmlDocument.CreateElement(settingsXmlNodeName);
                xmlSettingsNode.SetAttribute(settingsAutoRefreshAttrName, Settings.AutoRefresh.ToString());
                xmlSettingsNode.SetAttribute(settingsHighlightNewLogEntriesAttrName, Settings.HighlightNewLogEntries.ToString());
                xmlLogViewNode.AppendChild(xmlSettingsNode);

                return xmlDocument.OuterXml;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export {GetType().Name} to xml. Inner exception message: {ex.Message}");
            }
        }

        public void Open()
        {
            IsOpen = true;
            DateLastOpened = DateTime.Now;
        }

        public void Close()
        {
            IsOpen = false;
            IsEdited = false;
        }

        public void AddLogFile(LogFile logFile)
        {
            LogFilePaths.Add(logFile.NetworkFile.FullName);
        }

        public void AddDatabase(Database database)
        {
            DatabaseNames.Add(database.Name);
        }
        #endregion

        #region Private methods
        private void SubscribeToEvents()
        {
            FilterOptions.Changed += FilterOptionsOrSettings_Changed;
            Settings.Changed += FilterOptionsOrSettings_Changed;
            LogFilePaths.CollectionChanged += LogFilePaths_CollectionChanged;
            DatabaseNames.CollectionChanged += DatabaseNames_CollectionChanged;
        }

        private void FilterOptionsOrSettings_Changed()
        {
            IsEdited = true;
        }

        private void DatabaseNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsEdited = true;
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    AllLogFilePathsAndDatabaseNames.Add(newItem.ToString());
                }
            }
        }

        private void LogFilePaths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsEdited = true;
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    AllLogFilePathsAndDatabaseNames.Add(newItem.ToString());
                }
            }
        }

        private void SetName(string suggestedName)
        {
            //TODO: Come up with better way of getting unique log view name. If user runs multiple instances in single day, could get duplicates with the method below.
            Name = $"{suggestedName}_{++_suffixInt}_{DateTime.Now.ToShortDateString()}";
        }
        #endregion
    }
}
