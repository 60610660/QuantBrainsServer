using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuantBrainsMonitor.Converters
{
    /// <summary>
    /// 將動能值轉換為顏色
    /// </summary>
    public class MomentumToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double momentum)
            {
                if (momentum >= 80)
                    return new SolidColorBrush(Colors.Green);
                else if (momentum >= 60)
                    return new SolidColorBrush(Colors.LightGreen);
                else if (momentum >= 40)
                    return new SolidColorBrush(Colors.Yellow);
                else if (momentum >= 20)
                    return new SolidColorBrush(Colors.Orange);
                else
                    return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}