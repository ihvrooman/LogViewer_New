using LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    public class SelectedLogViewChangedEventArgs
    {
        #region Properties
        public LogView OldLogView { get; set; }
        public LogView NewLogView { get; set; }
        #endregion

        #region Constructor
        public SelectedLogViewChangedEventArgs(LogView oldLogView, LogView newLogView)
        {
            OldLogView = oldLogView;
            NewLogView = newLogView;
        }
        #endregion
    }
}
