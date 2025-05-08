using System;
using System.Globalization;
using System.Windows.Data;

namespace QuantBrainsMonitor.Converters
{
    /// <summary>
    /// 將可為空的值轉換為布爾值
    /// </summary>
    public class NullableToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果值不為空，返回 true
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}