using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using QuantBrainsMonitor.Models;

namespace QuantBrainsMonitor.Converters
{
    /// <summary>
    /// 將策略狀態轉換為顏色
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StrategyStatus status)
            {
                switch (status)
                {
                    case StrategyStatus.Running:
                        return new SolidColorBrush(Colors.Green);
                    case StrategyStatus.Stopped:
                        return new SolidColorBrush(Colors.Red);
                    case StrategyStatus.Paused:
                        return new SolidColorBrush(Colors.Orange);
                    case StrategyStatus.Error:
                        return new SolidColorBrush(Colors.DarkRed);
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}