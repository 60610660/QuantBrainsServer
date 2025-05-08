using System;
using System.ComponentModel;

namespace QuantBrainsMonitor.Models
{
    /// <summary>
    /// 策略狀態枚舉
    /// </summary>
    public enum StrategyStatus
    {
        Stopped,   // 已停止
        Running,   // 運行中
        Paused,    // 已暫停
        Error      // 錯誤
    }

    /// <summary>
    /// 策略模型類
    /// </summary>
    public class Strategy : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private StrategyStatus _status;
        private string _symbol;
        private string _timeframe;
        private int _magicNumber;
        private double _profit;
        private double _drawdown;
        private double _winRate;
        private double _momentum; // 動能值
        private int _totalTrades;
        private DateTime _lastUpdate;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public StrategyStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case StrategyStatus.Running:
                        return "運行中";
                    case StrategyStatus.Stopped:
                        return "已停止";
                    case StrategyStatus.Paused:
                        return "已暫停";
                    case StrategyStatus.Error:
                        return "錯誤";
                    default:
                        return "未知";
                }
            }
        }

        public string Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }

        public string Timeframe
        {
            get => _timeframe;
            set
            {
                _timeframe = value;
                OnPropertyChanged(nameof(Timeframe));
            }
        }

        public int MagicNumber
        {
            get => _magicNumber;
            set
            {
                _magicNumber = value;
                OnPropertyChanged(nameof(MagicNumber));
            }
        }

        public double Profit
        {
            get => _profit;
            set
            {
                _profit = value;
                OnPropertyChanged(nameof(Profit));
            }
        }

        public double Drawdown
        {
            get => _drawdown;
            set
            {
                _drawdown = value;
                OnPropertyChanged(nameof(Drawdown));
            }
        }

        public double WinRate
        {
            get => _winRate;
            set
            {
                _winRate = value;
                OnPropertyChanged(nameof(WinRate));
            }
        }

        public double Momentum
        {
            get => _momentum;
            set
            {
                _momentum = value;
                OnPropertyChanged(nameof(Momentum));
            }
        }

        public int TotalTrades
        {
            get => _totalTrades;
            set
            {
                _totalTrades = value;
                OnPropertyChanged(nameof(TotalTrades));
            }
        }

        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set
            {
                _lastUpdate = value;
                OnPropertyChanged(nameof(LastUpdate));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}