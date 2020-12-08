using AppStandards.MVVM;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public class OpenableObject : PropertyChangedHelper
    {
        #region Fields
        private DateTime _dateLastOpened;
        private bool _isPinned;
        private bool _isOpen;
        private string _identifier;
        #endregion

        #region Properties
        public DateTime DateLastOpened { get { return _dateLastOpened; } set { _dateLastOpened = value; RaisePropertyChangedEvent(); } }
        public bool IsPinned { get { return _isPinned; } set { _isPinned = value; RaisePropertyChangedEvent(); } }
        public bool IsOpen { get { return _isOpen; } set { _isOpen = value; RaisePropertyChangedEvent(); } }
        public string DateLastOpenedString
        {
            get
            {
                return DateLastOpened.ToString(AppInfo.LastOpenedTimeFormatString);
            }
        }
        public string Identifier { get { return _identifier; } set { _identifier = value; RaisePropertyChangedEvent(); } }
        #endregion
    }
}
