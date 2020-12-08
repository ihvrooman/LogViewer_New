using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public class QuickDateRange
    {
        #region Properties
        public string Name { get; set; }
        public DateTime MinDate { get; set; }
        public int MinMilliseconds { get; set; }
        public DateTime MaxDate { get; set; }
        public int MaxMilliseconds { get; set; }
        #endregion
    }
}
