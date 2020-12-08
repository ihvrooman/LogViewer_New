using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    public interface ILogEntriesSource
    {
        #region Properties
        string Identifier { get; }
        string Source { get; }
        bool AutoRefresh { get; set; }
        #endregion
    }
}
