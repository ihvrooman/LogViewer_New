using AppStandards.MVVM;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public class LogViewSettings : PropertyChangedHelper
    {
        #region Fields
        private bool _autoRefresh;
        private bool _highlightNewLogEntries;
        #endregion

        #region Properties
        public bool AutoRefresh { get { return _autoRefresh; } set { _autoRefresh = value; RaisePropertyChangedEvent(); RaiseSettingsChangedEvent(); Mediator.NotifyColleagues(MediatorMessages.AutoRefreshSettingChanged, AutoRefresh); } }
        public bool HighlightNewLogEntries { get { return _highlightNewLogEntries; } set { _highlightNewLogEntries = value; RaisePropertyChangedEvent(); RaiseSettingsChangedEvent(); } }
        #endregion

        #region Events/delegates
        public event RefreshEventHandler Changed;
        public delegate void RefreshEventHandler();
        #endregion

        #region Private methods
        private void RaiseSettingsChangedEvent()
        {
            Changed?.Invoke();
        }
        #endregion
    }
}
