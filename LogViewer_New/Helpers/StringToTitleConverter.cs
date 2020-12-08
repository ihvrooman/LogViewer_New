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

namespace LogViewer.Helpers
{
    public class StringToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            //var maxWidth = (int)parameter;
            var proposedTitle = value.ToString();
            if (proposedTitle.Length > 25)
            {
                //proposedTitle = proposedTitle.Substring(0, 24) + "...";
            }
            return proposedTitle;

            //var formattedText = new FormattedText(
            //    proposedTitle,
            //    CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface("Segoe UI"),

            //    16,
            //    Brushes.Black,
            //    new NumberSubstitution(),
            //    1);
            //while (formattedText.Width > maxWidth && proposedTitle.Length > 5)
            //{
            //    proposedTitle = proposedTitle.Substring()
            //    formattedText = new FormattedText(
            //    proposedTitle,
            //    CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface("Segoe UI"),

            //    16,
            //    Brushes.Black,
            //    new NumberSubstitution(),
            //    1);
            //}

            //return new Size(formattedText.Width, formattedText.Height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
