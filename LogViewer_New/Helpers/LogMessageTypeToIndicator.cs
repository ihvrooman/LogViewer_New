using AppStandards.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LogViewer.Helpers
{
    public class LogMessageTypeToIndicator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogMessageType logMessageType = (LogMessageType)value;
            Uri iconUri;
            switch (logMessageType)
            {
                case LogMessageType.Error:
                    iconUri = new Uri("/LogViewer;component/Icons/icons8-cancel-48.png", UriKind.Relative);
                    break;
                case LogMessageType.Warning:
                    iconUri = new Uri("/LogViewer;component/Icons/icons8-error-48.png", UriKind.Relative);
                    break;
                case LogMessageType.Information:
                    iconUri = new Uri("/LogViewer;component/Icons/icons8-info-50.png", UriKind.Relative);
                    break;
                case LogMessageType.Debug:
                    iconUri = new Uri("/LogViewer;component/Icons/icons8-bug-50.png", UriKind.Relative);
                    break;
                case LogMessageType.Unknown:
                    iconUri = new Uri("/LogViewer;component/Icons/icons8-help-50.png", UriKind.Relative);
                    break;
                default:
                    return new TextBlock() { Text = logMessageType.ToString(), HorizontalAlignment = HorizontalAlignment.Center };
            }

            if (iconUri != null)
            {
                var icon = new Image() { Width = 30, Height = 30, HorizontalAlignment = HorizontalAlignment.Center };
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = iconUri;
                bitmap.EndInit();
                icon.Stretch = Stretch.Fill;
                icon.StretchDirection = StretchDirection.Both;
                icon.Source = bitmap;
                return icon;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
