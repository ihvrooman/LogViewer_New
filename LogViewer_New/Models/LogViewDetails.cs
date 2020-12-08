using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public class LogViewDetails
    {
        #region Properties
        public LogView LogView { get; set; }
        public List<string> LogFilePaths { get; set; } = new List<string>();
        public List<string> DatabaseNames { get; set; } = new List<string>();
        #endregion
    }
}
