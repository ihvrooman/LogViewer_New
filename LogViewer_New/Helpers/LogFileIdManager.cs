using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    internal static class LogFileIdManager
    {
        #region Fields
        private volatile static int _nextAvailableId;
        #endregion

        #region Internal methods
        internal static int GetNextAvailableId()
        {
            return _nextAvailableId++;
        }
        #endregion
    }
}
