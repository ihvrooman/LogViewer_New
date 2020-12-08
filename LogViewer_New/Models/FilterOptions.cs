using AppStandards.Logging;
using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LogViewer.Models
{
    /// <summary>
    /// Contains options for filtering a <see cref="LogView"/>.
    /// </summary>
    public class FilterOptions : PropertyChangedHelper
    {
        #region Fields
        #region Log message type
        private bool _includeErrors;
        private bool _includeWarnings;
        private bool _includeInformation;
        private bool _includeVerbose;
        private bool _includeDebug;
        private bool _includeDebugErrors;
        private bool _includeDebugWarnings;
        private bool _includeDebugInformation;
        private bool _includeDebugVerbose;
        private bool _includeUnknown;
        #endregion

        #region Date range
        private bool _useQuickDateRange = true;
        private bool _specifyDateRange;
        private QuickDateRange _selectedQuickDateRange;
        private DateTime _minDay;
        private int _minHour;
        private int _minMinute;
        private int _minSecond;
        private int _minMillisecond;
        private DateTime _maxDay;
        private int _maxHour;
        private int _maxMinute;
        private int _maxSecond;
        private int _maxMillisecond;
        #endregion

        #region Other filter options
        private string _filterUsername = string.Empty;
        private string _filterComputername = string.Empty;
        private string _searchTerm = string.Empty;
        private string _exclusionTerm = string.Empty;
        #endregion

        private ICommand _clearFiltersCommand;
        //TODO: Consider using source cache instead of source list for available user and computer names.
        private bool _skipFilterOptionsChanged;
        #endregion

        #region Properties
        #region Log message type
        public bool IncludeErrors { get { return _includeErrors; } set { _includeErrors = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeWarnings { get { return _includeWarnings; } set { _includeWarnings = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeInformation { get { return _includeInformation; } set { _includeInformation = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeVerbose { get { return _includeVerbose; } set { _includeVerbose = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeDebug { get { return _includeDebug; } set { _includeDebug = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeDebugErrors { get { return _includeDebugErrors; } set { _includeDebugErrors = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeDebugWarnings { get { return _includeDebugWarnings; } set { _includeDebugWarnings = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeDebugInformation { get { return _includeDebugInformation; } set { _includeDebugInformation = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeDebugVerbose { get { return _includeDebugVerbose; } set { _includeDebugVerbose = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public bool IncludeUnknown { get { return _includeUnknown; } set { _includeUnknown = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        #endregion

        #region Date range
        public bool UseQuickDateRange { get { return _useQuickDateRange; } set { _useQuickDateRange = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); if (!UseQuickDateRange) { SelectedQuickDateRange = null; } } }
        public bool SpecifyDateRange { get { return _specifyDateRange; } set { _specifyDateRange = value; RaisePropertyChangedEvent(); } }
        public List<QuickDateRange> QuickDateRanges { get; set; } = new List<QuickDateRange>();
        public QuickDateRange SelectedQuickDateRange { get { return _selectedQuickDateRange; } set { _selectedQuickDateRange = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); LoadQuickDateRange(SelectedQuickDateRange); } }

        public DateTime MinDay { get { return _minDay; } set { _minDay = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MinHour { get { return _minHour; } set { value = EnsureValueIsInBounds(value, MaxValue: 23); _minHour = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MinMinute { get { return _minMinute; } set { value = EnsureValueIsInBounds(value); _minMinute = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MinSecond { get { return _minSecond; } set { value = EnsureValueIsInBounds(value); _minSecond = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MinMillisecond { get { return _minMillisecond; } set { value = EnsureValueIsInBounds(value, MaxValue: 9999); _minMillisecond = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public DateTime MinDate
        {
            get
            {
                return DateTime.ParseExact($"{MinDay.ToString("MM/dd/yyyy")} {MinHour.ToString("D2")}:{MinMinute.ToString("D2")}:{MinSecond.ToString("D2")}:{MinMillisecond.ToString("D4")}", "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture);
            }
        }
        public DateTime MaxDay { get { return _maxDay; } set { _maxDay = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MaxHour { get { return _maxHour; } set { value = EnsureValueIsInBounds(value, MaxValue: 23); _maxHour = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MaxMinute { get { return _maxMinute; } set { value = EnsureValueIsInBounds(value); _maxMinute = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MaxSecond { get { return _maxSecond; } set { value = EnsureValueIsInBounds(value); _maxSecond = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public int MaxMillisecond { get { return _maxMillisecond; } set { value = EnsureValueIsInBounds(value, MaxValue: 9999); _maxMillisecond = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public DateTime MaxDate
        {
            get
            {
                return DateTime.ParseExact($"{MaxDay.ToString("MM/dd/yyyy")} {MaxHour.ToString("D2")}:{MaxMinute.ToString("D2")}:{MaxSecond.ToString("D2")}:{MaxMillisecond.ToString("D4")}", "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture);
            }
        }
        #endregion

        #region Other filter options
        public string FilterUsername { get { return _filterUsername; } set { _filterUsername = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public string FilterComputername { get { return _filterComputername; } set { _filterComputername = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public string SearchTerm { get { return _searchTerm; } set { _searchTerm = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        public string ExclusionTerm { get { return _exclusionTerm; } set { _exclusionTerm = value; RaisePropertyChangedEvent(); RaiseFilterOptionsChangedEvent(); } }
        #endregion

        public ICommand ClearFiltersCommand
        {
            get
            {
                if (_clearFiltersCommand == null)
                {
                    _clearFiltersCommand = new RelayCommand(Clear);
                }
                return _clearFiltersCommand;
            }
        }
        #endregion

        #region Events/delegates
        public event RefreshEventHandler Changed;
        public delegate void RefreshEventHandler();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new set of <see cref="FilterOptions"/> with the default settings.
        /// </summary>
        public FilterOptions()
        {
            BuildQuickRanges();
            Clear();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Clears the <see cref="FilterOptions"/>.
        /// </summary>
        public void Clear()
        {
            try
            {
                _skipFilterOptionsChanged = true;

                SelectedQuickDateRange = QuickDateRanges[0];

                IncludeErrors = true;
                IncludeWarnings = true;
                IncludeInformation = true;
                IncludeVerbose = true;
                IncludeDebug = true;
                IncludeDebugErrors = true;
                IncludeDebugWarnings = true;
                IncludeDebugInformation = true;
                IncludeDebugVerbose = true;
                IncludeUnknown = true;

                FilterUsername = string.Empty;
                FilterComputername = string.Empty;

                SearchTerm = string.Empty;
                ExclusionTerm = string.Empty;
            }
            catch (Exception ex)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to clear filters. Error message: {ex.Message}", LogMessageType.Error);
            }
            finally
            {
                _skipFilterOptionsChanged = false;
                RaiseFilterOptionsChangedEvent();
            }
        }

        public void LoadQuickDateRange(QuickDateRange quickDateRange)
        {
            if (quickDateRange == null)
            {
                return;
            }

            MinDay = quickDateRange.MinDate;
            MinHour = quickDateRange.MinDate.Hour;
            MinMinute = quickDateRange.MinDate.Minute;
            MinSecond = quickDateRange.MinDate.Second;
            MinMillisecond = quickDateRange.MinMilliseconds;

            MaxDay = quickDateRange.MaxDate;
            MaxHour = quickDateRange.MaxDate.Hour;
            MaxMinute = quickDateRange.MaxDate.Minute;
            MaxSecond = quickDateRange.MaxDate.Second;
            MaxMillisecond = quickDateRange.MaxMilliseconds;
        }
        #endregion

        #region Private methods
        private void RaiseFilterOptionsChangedEvent()
        {
            if (!_skipFilterOptionsChanged)
            {
                Changed?.Invoke();
                Mediator.NotifyColleagues(MediatorMessages.FilterOptionsChanged, null);
            }
        }

        private int EnsureValueIsInBounds(int value, int minValue = 0, int MaxValue = 59)
        {
            if (value > MaxValue)
            {
                value = MaxValue;
            }
            else if (value < minValue)
            {
                value = minValue;
            }
            return value;
        }

        private void BuildQuickRanges()
        {
            //All time
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "All time",
                MinDate = DateTime.MinValue,
                MaxDate = DateTime.MaxValue,
                MaxMilliseconds = 9999,
            });

            //Today
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Today",
                MinDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Yesterday
            var yesterday = DateTime.Now.AddDays(-1);
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Yesterday",
                MinDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Past two days
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Past two days",
                MinDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Past week
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Past week",
                MinDate = new DateTime(oneWeekAgo.Year, oneWeekAgo.Month, oneWeekAgo.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Past two weeks
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Past two weeks",
                MinDate = new DateTime(twoWeeksAgo.Year, twoWeeksAgo.Month, twoWeeksAgo.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Past 30 days
            var lastThirtyDays = DateTime.Now.AddDays(-30);
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Past 30 days",
                MinDate = new DateTime(lastThirtyDays.Year, lastThirtyDays.Month, lastThirtyDays.Day, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //This month
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "This month",
                MinDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999),
                MaxMilliseconds = 9999,
            });

            //Last month
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            QuickDateRanges.Add(new QuickDateRange()
            {
                Name = "Last month",
                MinDate = new DateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1, 0, 0, 0, 0),
                MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0),
            });
        }
        #endregion
    }
}
