using AppStandards.MVVM;
using AppStandards.Logging;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogViewer.ViewModels;
using AppStandards;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.Xml;
using System.Globalization;
using LogViewer.Services;
using System.Timers;

namespace LogViewer.Models
{
    public class Database : OpenableObject, ILogEntriesSource
    {
        #region Constants
        private const string databaseXmlNodeName = "Database";
        #endregion

        #region Fields
        private bool _refreshIndicator;
        private Timer _refreshTimer = new Timer(100);
        private volatile bool _refreshing;
        private volatile bool _refreshPending;
        private string _name;
        #endregion

        #region Properties
        public string Name { get { return _name; } private set { _name = value; RaisePropertyChangedEvent(); Identifier = Name; } }
        public string Source { get { return Name + $".{DatabaseService.DatabaseAndTableName}"; } }
        /// <summary>
        /// Used to force a refresh of the <see cref="LogViewManagementViewModel.DatabasesSourceCache"/> and <see cref="LogViewManagementViewModel.LogFilesSourceCache"/> collections.
        /// </summary>
        public bool RefreshIndicator { get { return _refreshIndicator; } private set { _refreshIndicator = value; RaisePropertyChangedEvent(); } }
        public bool AutoRefresh { get; set; }
        public bool Refreshing { get { return _refreshing; } set { _refreshing = value; } }
        public bool RefreshPending { get { return _refreshPending; } set { _refreshPending = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Database"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Database"/>.</param>
        public Database(string name)
        {
            Name = name;
            DateLastOpened = DateTime.Now;
            Initialize();
        }

        /// <summary>
        /// Creates a new <see cref="Database"/> from the xml <see cref="Database"/> representation.
        /// </summary>
        /// <param name="xmlDatabase">The xml representation of the <see cref="Database"/>.</param>
        public Database(string xmlDatabase, bool isNew = false)
        {
            BuildDatabaseFromXml(xmlDatabase);
            Initialize();
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
                if (!_refreshTimer.Enabled)
                {
                    _refreshTimer.Start();
                }
            }
        }

        /// <summary>
        /// Exports the <see cref="Database"/> to xml format so that it can be saved.
        /// </summary>
        /// <returns>The xml representation of the <see cref="Database"/>.</returns>
        public string ExportToXml()
        {
            var xmlDocument = new XmlDocument();

            var xmlDatabaseNode = ExportToXmlNode(xmlDocument);
            xmlDocument.AppendChild(xmlDatabaseNode);

            return xmlDocument.OuterXml;
        }

        /// <summary>
        /// Exports the <see cref="Database"/> to an xml node so that it can be saved.
        /// </summary>
        /// <returns>The xml node representation of the <see cref="Database"/>.</returns>
        public XmlNode ExportToXmlNode(XmlDocument xmlDocument)
        {

            //Create database node, add it's attributes, and add it to the xml doc
            var xmlDatabaseNode = xmlDocument.CreateElement(databaseXmlNodeName);
            xmlDatabaseNode.SetAttribute(CommonXmlAttributeNames.Name, Name);
            xmlDatabaseNode.SetAttribute(CommonXmlAttributeNames.DateLastOpened, DateLastOpenedString);
            xmlDatabaseNode.SetAttribute(CommonXmlAttributeNames.IsPinned, IsPinned.ToString());

            return xmlDatabaseNode;
        }

        public static XmlNode CreateXmlNodeFromName(string name, XmlDocument xmlDocument)
        {

            //Create database node, add it's attributes, and add it to the xml doc
            var xmlDatabaseNode = xmlDocument.CreateElement(databaseXmlNodeName);
            xmlDatabaseNode.SetAttribute(CommonXmlAttributeNames.Name, name);

            return xmlDatabaseNode;
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
            Mediator.NotifyColleagues(MediatorMessages.RequestRefreshDatabase, this);
        }
        #endregion

        #region Private methods
        private void BuildDatabaseFromXml(string xmlDatabase)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlDatabase);

                //Set the database name and last opened date
                Name = xmlDocument.DocumentElement.Attributes[0].Value;
                DateLastOpened = DateTime.ParseExact(xmlDocument.DocumentElement.Attributes[1].Value, AppInfo.LastOpenedTimeFormatString, CultureInfo.InvariantCulture);
                IsPinned = Convert.ToBoolean(xmlDocument.DocumentElement.Attributes[2].Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create {GetType().Name} from xml. Xml parameter: \"{xmlDatabase}\" | Inner exception message: {ex.Message}");
            }
        }

        private void Initialize()
        {
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (AutoRefresh)
            {
                RequestRefresh();
            }
        }
        #endregion
    }
}
