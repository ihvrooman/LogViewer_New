using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Models
{
    public class AddDatabaseInfo
    {
        #region Constants
        public const char DatabaseInfoSplitter = '*';
        #endregion

        #region Properties
        public string SQLInstanceName { get; private set; }
        public string DatabaseName { get; private set; }
        public string SQLUsername { get; private set; }
        public string SQLPassword { get; private set; }
        #endregion

        #region Constructor
        public AddDatabaseInfo(string unformattedDatabaseName)
        {
            var split = unformattedDatabaseName.Split(DatabaseInfoSplitter);
            SQLInstanceName = split[0].ToUpper();
            DatabaseName = split[1].ToUpper();

            if (split.Length >= 4)
            {
                SQLUsername = split[2].ToUpper();
                SQLPassword = split[3];
            }
        }
        #endregion

        #region Methods
        public string ToFormattedString()
        {
            var formattedString = SQLInstanceName + DatabaseInfoSplitter + DatabaseName;
            if (!string.IsNullOrEmpty(SQLUsername) && !string.IsNullOrEmpty(SQLPassword))
            {
                formattedString += DatabaseInfoSplitter + SQLUsername + DatabaseInfoSplitter + SQLPassword;
            }
            return formattedString;
        }

        public string ToConnectionString()
        {
            if (!string.IsNullOrEmpty(SQLUsername) && !string.IsNullOrEmpty(SQLPassword))
            {
                return $"Data Source={SQLInstanceName};Initial Catalog={DatabaseName};User ID = {SQLUsername}; Password={SQLPassword}";
            }
            else
            {
                return $"Data Source={SQLInstanceName};Integrated Security=true;Initial Catalog={DatabaseName}";
            }
        }
        #endregion
    }
}
