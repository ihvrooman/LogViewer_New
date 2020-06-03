using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LogViewer.Helpers
{
    public class SizeInBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var numberOfBytes = System.Convert.ToDouble(value);
            if (numberOfBytes < 0)
            {
                return string.Empty;
            }
            else if (numberOfBytes < 1000000)
            {
                var numberOfKB = numberOfBytes / 1000;
                if (numberOfKB > 0 && numberOfKB < 1)
                {
                    return "1 KB";
                }
                return Math.Round(numberOfKB, 0).ToString() + " KB";
            }
            else if (numberOfBytes < 1000000000)
            {
                var numberOfMB = numberOfBytes / 1000000;
                if (numberOfMB > 0 && numberOfMB < 1)
                {
                    return "1 MB";
                }
                return Math.Round(numberOfMB, 0).ToString() + " MB";
            }
            else
            {
                var numberOfGB = numberOfBytes / 1000000000;
                if (numberOfGB > 0 && numberOfGB < 1)
                {
                    return "1 GB";
                }
                return Math.Round(numberOfGB, 0).ToString() + " GB";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
