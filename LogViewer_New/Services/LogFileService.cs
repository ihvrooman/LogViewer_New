using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class LogFileService
    {
        #region Public methods
        public static async Task<ServiceOperationResult> LoadLogEntriesIntoSourceCacheAsync(LogFile logFile, SourceCache<LogEntry, string> logEntriesSourceCache)
        {
            var serviceOperationHelper = new ServiceOperationHelper(typeof(LogEntry), Plurality.Plural, ServiceOperation.Load, $"{typeof(LogFile).Name} with network path \"{logFile.NetworkFile.FullName}\"");
            await Task.Run(() =>
            {
                try
                {
                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Attempting);

                    var numberOfOldLogEntries = logEntriesSourceCache.Keys.Where(k => k.Contains(logFile.Identifier)).Count();
                    var contents = File.ReadAllText(logFile.NetworkFile.FullName);
                    long logEntryIndex = 0;
                    logEntriesSourceCache.Edit(innerCache =>
                    {
                        foreach (var line in GetLogEntriesFromLogContents(contents))
                        {
                            if (++logEntryIndex > numberOfOldLogEntries)
                            {
                                var newLogEntry = ParseLogEntry(line, logFile, LogEntry.GetIdentifier(logFile.Identifier, logEntryIndex));
                                newLogEntry.IsNew = true;
                                innerCache.AddOrUpdate(newLogEntry);
                            }
                        }
                    });

                    serviceOperationHelper.LogServiceOperation(ServiceOperationStatus.Succeeded);
                }
                catch (Exception ex)
                {
                    serviceOperationHelper.LogServiceOperation(ex.Message);
                }
            });
            return serviceOperationHelper.ServiceOperationResult;
        }
        #endregion

        #region Private methods
        private static List<string> GetLogEntriesFromLogContents(string logContents)
        {
            //Determine whether or not log type is standard (i.e. created by the AppStandards library)
            var prefix = string.Empty;
            if (logContents.Length > 2)
            {
                prefix = logContents.Substring(0, 3);
            }
            if (prefix == "-E-" || prefix == "-W-" || prefix == "-I-" || prefix == "-V-" || prefix == "-D-" || prefix == "-D:E-" || prefix == "-D:W-" || prefix == "-D:I-" || prefix == "-D:V-" || prefix == "-U-")
            {
                //Log is standard log, begin parsing
                var partialLogEntryStrings = Regex.Split(logContents, "(-[EWIVDU]-)");
                if (partialLogEntryStrings.Length < 2)
                {
                    partialLogEntryStrings = Regex.Split(logContents, "(-[D]:[EWIV]-)");
                }
                return CombinePartialLogEntries(partialLogEntryStrings);
            }
            else if (IsProficyDateTime(logContents.Substring(0, 23), new TimeSpan(0, 0, 0), out DateTime proficyDateTime, false))
            {
                //Log is a proficy log, begin parsing
                logContents = logContents.Replace("\t", " ");
                var partialLogEntryStrings = Regex.Split(logContents, @"(\d+[-]\d+[-]\d+\s\d+[:]\d+[:]\d+[,.]\d+)");
                return CombinePartialLogEntries(partialLogEntryStrings);
            }
            else if (logContents.Substring(0, 7).Contains("**"))
            {
                //Special case: log contents begin with header (*** Log Started ***)
                for (int i = 0; i < logContents.Length; i++)
                {
                    if (int.TryParse(logContents.Substring(i, 1), out int int1))
                    {
                        //If hit an int, might be start of datetime. Try removing header and re-processing
                        return GetLogEntriesFromLogContents(logContents.Substring(i, logContents.Length - i - 1));
                    }
                }
            }
            else if (DateTime.TryParse(logContents.Substring(0, 27).Replace('T', ' '), out _))
            {
                //Log entry is an NLog log entry
                var logEntryPartialStrings = Regex.Split(logContents, @"(\d+[-]\d+[-]\d+\w\d+[:]\d+[:]\d+[,.]\d+)");
                return CombinePartialLogEntries(logEntryPartialStrings);
            }
            throw new Exception($"Could not parse log contents as Standard, NLog, or Proficy log.");
        }

        private static List<string> CombinePartialLogEntries(string[] partialLogEntryStrings)
        {
            var logEntries = new List<string>();
            var combine = false;
            var timeStamp = string.Empty;
            foreach (var partialLogEntryString in partialLogEntryStrings)
            {
                if (!string.IsNullOrWhiteSpace(partialLogEntryString))
                {
                    if (combine)
                    {
                        logEntries.Add(timeStamp + partialLogEntryString);
                    }
                    else
                    {
                        timeStamp = partialLogEntryString;
                    }
                    combine = !combine;
                }
            }

            return logEntries;
        }

        /// <summary>
        /// Parses a log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        private static LogEntry ParseLogEntry(string logEntryString, LogFile parentLogFile, string logEntryIdentifier)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            //Try to parse as standard log entry
            try
            {
                return ParseStandardLogEntry(logEntryString, parentLogFile, logEntryIdentifier);
            }
            catch (Exception ex)
            {
                //If can't parse as standard, try parsing as NLog
                try
                {
                    return ParseNLogEntry(logEntryString, parentLogFile, logEntryIdentifier);
                }
                catch (Exception ex_nlog)
                {
                    //If can't parse as standard or NLog, try parsing as proficy log entry
                    try
                    {
                        //Add computername to filter options
                        Mediator.NotifyColleagues(MediatorMessages.AddAvailableComputername, parentLogFile.ComputerName);
                        return ParseProficyLogEntry(logEntryString, parentLogFile, logEntryIdentifier);
                    }
                    catch (Exception ex_prof)
                    {
                        var message = $"Could not parse log entry string as standard, NLog, or Proficy log entry. ";
                        if (logEntryString != null)
                        {
                            message += $"| Log entry string: \"{logEntryString}\" ";
                        }
                        message += $"| Inner exception message from standard parse: {ex.Message} | Inner exception message from NLog parse: {ex_nlog.Message} | Inner exception message from Proficy parse: {ex_prof.Message}";
                        throw new Exception(message);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a standard log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        private static LogEntry ParseStandardLogEntry(string logEntryString, LogFile parentLogFile, string logEntryIdentifier)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            try
            {
                //Extract the type, timestamp, and message as strings
                var logEntryChars = logEntryString.ToCharArray();
                var typeString = logEntryString.Substring(0, 3); //TODO: LogViewer doesn't currently parse log entries with type indicator that has more than 3 chars (e.g. '-D:E-').
                var timeStampString = string.Empty;
                var username = string.Empty;
                var computerName = string.Empty;
                var message = string.Empty;

                var startingIndex = 4;
                bool skipSpace = true;
                for (int i = startingIndex; i < logEntryChars.Length; i++)
                {
                    if (char.IsWhiteSpace(logEntryChars[i]))
                    {
                        if (skipSpace)
                        {
                            skipSpace = false;
                        }
                        else
                        {
                            timeStampString = logEntryString.Substring(startingIndex, i - startingIndex);
                            timeStampString = timeStampString.Substring(0, timeStampString.Length);
                            message = logEntryString.Substring(i + 1, logEntryString.Length - i - 1);
                            break;
                        }
                    }
                }

                //Determine whether or not log entry contains username and computername
                var usernamePrefix = " | Username: ";
                var computernamePrefix = " | Computer name: ";
                startingIndex = message.IndexOf(usernamePrefix);
                if (startingIndex > -1)
                {
                    var computernameStartingIndex = message.IndexOf(computernamePrefix);
                    if (computernameStartingIndex > -1)
                    {
                        //Extract username
                        username = message.Substring(startingIndex + usernamePrefix.Length, message.Length - startingIndex - (message.Length - computernameStartingIndex) - usernamePrefix.Length);

                        //Extract computername
                        computerName = message.Substring(computernameStartingIndex + computernamePrefix.Length, message.Length - computernameStartingIndex - computernamePrefix.Length);

                        //Remove newline character from computername (the newline char seperates log entries in log file
                        computerName = computerName.Replace(Environment.NewLine, string.Empty);

                        //Add username and computername to filter options
                        Mediator.NotifyColleagues(MediatorMessages.AddAvailableUsername, username);
                        Mediator.NotifyColleagues(MediatorMessages.AddAvailableComputername, computerName);

                        //Extract message
                        message = message.Substring(0, startingIndex);
                    }
                }

                //Determine the type
                LogMessageType type = LogMessageType.Unknown;
                switch (typeString)
                {
                    case "-E-":
                        type = LogMessageType.Error;
                        break;
                    case "-W-":
                        type = LogMessageType.Warning;
                        break;
                    case "-I-":
                        type = LogMessageType.Information;
                        break;
                    case "-V-":
                        type = LogMessageType.Verbose;
                        break;
                    case "-D-":
                        type = LogMessageType.Debug;
                        break;
                    case "-D:E-":
                        type = LogMessageType.DebugError;
                        break;
                    case "-D:W-":
                        type = LogMessageType.DebugWarning;
                        break;
                    case "-D:I-":
                        type = LogMessageType.DebugInformation;
                        break;
                    case "-D:V-":
                        type = LogMessageType.DebugVerbose;
                        break;
                    default:
                        type = LogMessageType.Unknown;
                        break;
                }

                //Parse time stamp
                var timeStamp = DateTime.ParseExact(timeStampString, "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

                return new LogEntry(type, timeStamp, message, parentLogFile, logEntryIdentifier, username, computerName);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse log entry as standard log entry. ";
                message += $"| Inner exception message: {ex.Message}";
                throw new Exception(message);
            }
        }

        private static LogEntry ParseNLogEntry(string logEntryString, LogFile parentLogFile, string logEntryIdentifier)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            try
            {
                //Extract the type, timestamp, and message as strings
                var typeString = string.Empty;
                var numOfChars = 1;
                for (int i = 36; i < logEntryString.Length - 37; i++)
                {
                    if (logEntryString[i] == ' ')
                    {
                        break;
                    }
                    ++numOfChars;
                }
                typeString = logEntryString.Substring(36, numOfChars).Replace(" ", string.Empty);


                var timeStampString = logEntryString.Substring(0, 24).Replace('T', ' ');
                var username = string.Empty;
                var computerName = string.Empty;
                var messageCharSkips = 44;
                var message = logEntryString.Substring(messageCharSkips, logEntryString.Length - messageCharSkips).Replace(Environment.NewLine, string.Empty);

                //Determine the type
                LogMessageType type = LogMessageType.Unknown;
                switch (typeString)
                {
                    case "ERROR":
                        type = LogMessageType.Error;
                        break;
                    case "FATAL":
                        type = LogMessageType.Error;
                        break;
                    case "WARNING":
                        type = LogMessageType.Warning;
                        break;
                    case "WARN":
                        type = LogMessageType.Warning;
                        break;
                    case "INFO":
                        type = LogMessageType.Information;
                        break;
                    case "VERBOSE":
                        type = LogMessageType.Verbose;
                        break;
                    case "DEBUG":
                        type = LogMessageType.Debug;
                        break;
                    default:
                        type = LogMessageType.Unknown;
                        break;
                }

                //Parse time stamp
                var timeStamp = DateTime.ParseExact(timeStampString, "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);

                return new LogEntry(type, timeStamp, message, parentLogFile, logEntryIdentifier, username, computerName);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse log entry as NLog log entry. ";
                message += $"| Inner exception message: {ex.Message}";
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Parses a Proficy log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        private static LogEntry ParseProficyLogEntry(string logEntryString, LogFile parentLogFile, string logEntryIdentifier)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            try
            {
                //Extract the timestamp and message as strings
                var timeStampString = logEntryString.Substring(0, 23);
                var message = logEntryString.Substring(24, logEntryString.Length - 26);

                #region Extract log message type
                LogMessageType type = LogMessageType.Unknown;
                var tryExtractingLogMessageTypeFromFront = true;

                //try extracting type from end of log message
                var logMessageTypeString = string.Empty;
                if (message.EndsWith("]"))
                {
                    try
                    {
                        var stopPoint = message.Length >= 20 ? message.Length - 20 : 0;
                        for (int i = message.Length - 2; i >= stopPoint; i--)
                        {
                            var lastChar = message.Substring(i, 1);
                            if (lastChar == "[")
                            {
                                break;
                            }
                            else
                            {
                                logMessageTypeString = lastChar + logMessageTypeString;
                            }
                        }
                        type = ParseProficyLogMessageType(logMessageTypeString);
                        if (type != LogMessageType.Unknown)
                        {
                            message = message.Substring(0, message.Length - logMessageTypeString.Length - 2);
                            tryExtractingLogMessageTypeFromFront = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"An error occurred when trying to extract log message type from back end of Proficy log message: \"{message}\" | Error message: {ex.Message}", LogMessageType.Warning);
                    }
                }

                //try extracting type from front of log message
                if (tryExtractingLogMessageTypeFromFront)
                {
                    logMessageTypeString = string.Empty;
                    try
                    {
                        var stopPoint = message.Length >= 150 ? 150 : message.Length;
                        var skip = false;
                        var numSkippedChars = 0;
                        var endSkipChar = " ";

                        for (int i = 0; i < stopPoint; i++)
                        {
                            string nextChar = message.Substring(i, 1);
                            if (nextChar == ":" || nextChar == "[")
                            {
                                if (nextChar == "[")
                                {
                                    //If skipping something with "[(some string)]" at front, stop skipping when hit end bracket
                                    endSkipChar = "]";
                                }

                                if (!string.IsNullOrWhiteSpace(logMessageTypeString))
                                {
                                    break;
                                }
                                else
                                {
                                    skip = true;
                                }
                            }
                            else if (skip && nextChar == endSkipChar)
                            {
                                skip = false;
                                numSkippedChars = i + 1;
                            }
                            else if (!skip)
                            {
                                logMessageTypeString += nextChar;
                            }
                        }
                        type = ParseProficyLogMessageType(logMessageTypeString);
                        if (type != LogMessageType.Unknown)
                        {
                            var skippedChars = message.Substring(0, numSkippedChars);
                            message = skippedChars + message.Substring(logMessageTypeString.Length + numSkippedChars, message.Length - logMessageTypeString.Length - numSkippedChars);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"An error occurred when trying to extract log message type from front end of Proficy log message: \"{message}\" | Error message: {ex.Message}", LogMessageType.Warning);
                    }
                }
                #endregion

                //Parse time stamp
                IsProficyDateTime(timeStampString, -parentLogFile.ComputerOffsetFromLocalTime, out DateTime timeStamp);

                return new LogEntry(type, timeStamp, message, parentLogFile, logEntryIdentifier, computername: parentLogFile.ComputerName);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse log entry as a Proficy log entry. ";
                message += $"| Inner exception message: {ex.Message}";
                throw new Exception(message);
            }
        }

        private static LogMessageType ParseProficyLogMessageType(string proficyLogMessageTypeString)
        {
            //Remove whitespace
            proficyLogMessageTypeString = Regex.Replace(proficyLogMessageTypeString, @"\s", string.Empty);

            switch (proficyLogMessageTypeString.ToLower())
            {
                case "error":
                    return LogMessageType.Error;
                case "err":
                    return LogMessageType.Error;
                case "error:error":
                    return LogMessageType.Error;
                case "warning":
                    return LogMessageType.Warning;
                case "warn":
                    return LogMessageType.Warning;
                case "information":
                    return LogMessageType.Information;
                case "info":
                    return LogMessageType.Information;
                case "verbose":
                    return LogMessageType.Verbose;
                case "debug":
                    return LogMessageType.Debug;
                case "debug:error":
                    return LogMessageType.DebugError;
                case "debug:warning":
                    return LogMessageType.DebugWarning;
                case "debug:information":
                    return LogMessageType.DebugInformation;
                case "debug:info":
                    return LogMessageType.DebugInformation;
                case "debug:verbose":
                    return LogMessageType.DebugVerbose;
                default:
                    return LogMessageType.Unknown;
            }
        }

        private static bool IsProficyDateTime(string proficyDateTimeString, TimeSpan offsetAdjustment, out DateTime proficyDateTime, bool throwExceptions = true)
        {
            try
            {
                proficyDateTime = DateTime.ParseExact(proficyDateTimeString, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                proficyDateTime = proficyDateTime.AddMilliseconds(offsetAdjustment.TotalMilliseconds);
                return true;
            }
            catch
            {
                try
                {
                    proficyDateTime = DateTime.ParseExact(proficyDateTimeString, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                    proficyDateTime = proficyDateTime.AddMilliseconds(offsetAdjustment.TotalMilliseconds);
                    return true;
                }
                catch
                {
                    if (throwExceptions)
                    {
                        throw new Exception($"Could not parse string \"{proficyDateTimeString}\" as Proficy dateTime.");
                    }
                    else
                    {
                        proficyDateTime = DateTime.MinValue;
                        return false;
                    }
                }
            }
        }
        #endregion
    }
}
