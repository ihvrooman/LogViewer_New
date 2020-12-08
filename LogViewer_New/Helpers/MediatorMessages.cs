using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    public class MediatorMessages
    {
        #region Constants
        public const string RequestToggleLaunchViewIsOpen = "0";
        public const string RequestOpenHomeView = "1";
        public const string RequestSetSelectedLogView = "2";
        public const string SelectedLogViewChanged = "3";
        public const string FilterOptionsChanged = "4";
        public const string AddAvailableUsername = "5";
        public const string AddAvailableComputername = "6";
        public const string LogSourceRemovedFromSelectedLogView = "7";
        public const string RequestOpenOpenLogViewView = "8";
        public const string RequestOpenOpenLogFileView = "9";
        public const string RequestOpenOpenDatabaseView = "10";
        public const string SelectedLogMessageChanged = "11";
        public const string AutoRefreshSettingChanged = "12";
        public const string RequestRefreshLogFile = "13";
        public const string AdjustOpenLogViewCount = "14";
        public const string CanCloseLaunchViewChanged = "15";
        public const string RequestRefreshDatabase = "16";
        public const string RequestShowLogViewDetailsDialog = "17";
        public const string RequestOpenLogView = "18";
        public const string UpdateIgnoreToggleLaunchViewIsOpenRequests = "19";
        #endregion
    }
}
