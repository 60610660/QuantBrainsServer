using System;
using System.Globalization;
using System.Windows.Data;

namespace QuantBrainsMonitor.Converters
{
    /// <summary>
    /// 將布爾值轉換為文字的轉換器
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? "斷開" : "連接";
            }
            return "連接";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}