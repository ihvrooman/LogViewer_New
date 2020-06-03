using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AppStandards.Logging;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.ViewModels;

namespace LogViewer.Models
{
    /// <summary>
    /// Represents a log entry.
    /// </summary>
    public class LogEntry : PropertyChangedHelper
    {
        #region Fields
        private Timer _timer;
        private bool _isNew;
        #endregion

        #region Properties
        /// <summary>
        /// The log entry's <see cref="LogMessageType"/>.
        /// </summary>
        public LogMessageType Type { get; set; }
        /// <summary>
        /// The log entry's UTC timestamp.
        /// </summary>
        public DateTime UtcTimeStamp { get; set; }
        /// <summary>
        /// The log entry's <see cref="UtcTimeStamp"/> converted to local time.
        /// </summary>
        public string TimeStamp
        {
            get
            {
                return UtcTimeStamp.ToLocalTime().ToString("MM/dd/yyy HH:mm:ss:ffff");
            }
        }
        /// <summary>
        /// The username which was used to write to the log.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The computer that the log entry was recorded from.
        /// </summary>
        public string Computername { get; set; }
        /// <summary>
        /// The log entry's log message.
        /// </summary>
        public string Message { get; set; }
        public ILogEntriesSource LogEntriesSource { get; set; }
        public string Identifier { get; set; }
        public bool IsNew
        {
            get { return _isNew; }
            set {
                _isNew = value;
                RaisePropertyChangedEvent();
                if (IsNew)
                {
                    _timer = new Timer(3000);
                    _timer.Elapsed += Timer_Elapsed;
                    _timer.Start();
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="UtcTimeStamp">The UTC timestamp.</param>
        /// <param name="message">The log message.</param>
        /// <param name="logEntriesSource">The <see cref="ILogEntriesSource"/> that this <see cref="LogEntry"/> came from.</param>
        /// <param name="identifier">This <see cref="LogEntry"/>'s unique identifier.</param>
        /// <param name="username">The username associated with the <see cref="LogEntry"/>.</param>
        /// <param name="computername">The computername associated with the <see cref="LogEntry"/>.</param>
        public LogEntry(LogMessageType type, DateTime UtcTimeStamp, string message, ILogEntriesSource logEntriesSource, string identifier, string username = "", string computername = "")
        {
            Type = type;
            this.UtcTimeStamp = UtcTimeStamp;
            Username = username;
            Computername = computername;
            Message = message;
            LogEntriesSource = logEntriesSource;
            Identifier = identifier;
        }
        #endregion

        #region Public/Internal methods
        /// <summary>
        /// Indicates whether or not two <see cref="LogEntry"/>s are equal.
        /// </summary>
        /// <param name="firstLogEntry">The first <see cref="LogEntry"/>.</param>
        /// <param name="secondLogEntry">The second <see cref="LogEntry"/>.</param>
        /// <returns>A <see cref="bool"/> which indicates whether or not the two <see cref="LogEntry"/>s are equal.</returns>
        public static bool AreEqual(LogEntry firstLogEntry, LogEntry secondLogEntry)
        {
            if (firstLogEntry == null && secondLogEntry == null)
            {
                return true;
            }

            if (firstLogEntry == null || secondLogEntry == null)
            {
                return false;
            }

            return firstLogEntry.UtcTimeStamp == secondLogEntry.UtcTimeStamp && firstLogEntry.Message == secondLogEntry.Message && firstLogEntry.Type == secondLogEntry.Type;
        }

        public static string GetIdentifier(string sourceIdentifier, long index)
        {
            return sourceIdentifier + index.ToString();
        }
        #endregion

        #region Private methods
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;
            IsNew = false;
        }
        #endregion
    }
}
