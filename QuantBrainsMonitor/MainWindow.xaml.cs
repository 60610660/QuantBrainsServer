using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO; // 這一行是必須的，用於Path和File操作
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace QuantBrainsMonitor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //private TcpClient _client;
        //private NetworkStream _stream;
        private bool _isConnected;
        private DispatcherTimer _updateTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        // 屬性
        public ObservableCollection<StrategyInfo> Strategies { get; set; }
        public SeriesCollection EquityChartData { get; set; }
        public string[] ChartLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public string ConnectionStatus => IsConnected ? "已連接" : "未連接";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 初始化集合
            Strategies = new ObservableCollection<StrategyInfo>();
            EquityChartData = new SeriesCollection();
            ChartLabels = new string[] { };
            YFormatter = value => value.ToString("C");

            // 初始化圖表
            InitializeChart();

            // 初始化計時器
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _updateTimer.Tick += UpdateTimer_Tick;

            // 添加測試數據
            AddTestData();
        }

        private void InitializeChart()
        {
            // 初始化圖表
            EquityChartData.Clear();

            var columnSeries = new ColumnSeries
            {
                Title = "策略獲利",
                Values = new ChartValues<double>()
            };

            EquityChartData.Add(columnSeries);
        }

        private void AddTestData()
        {
            // 添加測試數據
            Strategies.Add(new StrategyInfo
            {
                Name = "移動平均線策略",
                Status = "運行中",
                Profit = 1250.50,
                Drawdown = 0.05,
                WinRate = 0.65
            });

            Strategies.Add(new StrategyInfo
            {
                Name = "RSI策略",
                Status = "已停止",
                Profit = -320.75,
                Drawdown = 0.12,
                WinRate = 0.45
            });

            Strategies.Add(new StrategyInfo
            {
                Name = "布林通道策略",
                Status = "已暫停",
                Profit = 870.25,
                Drawdown = 0.08,
                WinRate = 0.58
            });

            // 更新圖表
            UpdateChart();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                // 模擬連接
                IsConnected = true;
                AddLogMessage("成功連接到策略管理器");
            }
            else
            {
                // 模擬斷開
                IsConnected = false;
                AddLogMessage("已斷開連接");
            }
        }

        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogMessage("已發送啟動所有策略命令");

            // 修改策略狀態（僅供測試）
            foreach (var strategy in Strategies)
            {
                strategy.Status = "運行中";
            }
        }

        private void StopAllButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogMessage("已發送停止所有策略命令");

            // 修改策略狀態（僅供測試）
            foreach (var strategy in Strategies)
            {
                strategy.Status = "已停止";
            }
        }

        private void PauseAllButton_Click(object sender, RoutedEventArgs e)
        {
            AddLogMessage("已發送暫停所有策略命令");

            // 修改策略狀態（僅供測試）
            foreach (var strategy in Strategies)
            {
                strategy.Status = "已暫停";
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                // 模擬數據更新
                Random rand = new Random();
                foreach (var strategy in Strategies)
                {
                    strategy.Profit += rand.Next(-100, 100);
                }

                UpdateChart();
                AddLogMessage("已更新數據");
            }
        }

        private void UpdateChart()
        {
            // 更新圖表數據
            if (Strategies.Count == 0) return;

            var series = EquityChartData[0] as ColumnSeries;
            series.Values.Clear();

            var labels = new string[Strategies.Count];

            for (int i = 0; i < Strategies.Count; i++)
            {
                series.Values.Add(Strategies[i].Profit);
                labels[i] = Strategies[i].Name;
            }

            ChartLabels = labels;
            OnPropertyChanged(nameof(ChartLabels));
        }

        private void AddLogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LogTextBox.AppendText($"[{timestamp}] {message}\n");
            LogTextBox.ScrollToEnd();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 添加在 MainWindow 類的最後面，在 OnPropertyChanged 方法之前或之後
        private void TestCommunicationButton_Click(object sender, RoutedEventArgs e)
        {
            TestCommunication();
        }

        private void TestCommunication()
        {
            try
            {
                // 使用MT5最兼容的目錄路徑
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MetaQuotes", "Terminal", "D0E8209F77C8CF37AD8BF550E51FF075",
                    "MQL5", "Files", "QuantBrains_Command.txt");

                // 寫入測試命令
                string command = "GET_STATUS";

                AddLogMessage($"嘗試寫入命令到: {path}");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // 使用無BOM的UTF-8編碼
                using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
                {
                    writer.Write(command);
                }

                AddLogMessage("測試文件已寫入，請檢查MT5日誌");
            }
            catch (Exception ex)
            {
                AddLogMessage($"測試通訊時出錯: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 策略信息類別
    /// </summary>
    public class StrategyInfo : INotifyPropertyChanged
    {
        private string _name;
        private string _status;
        private double _profit;
        private double _drawdown;
        private double _winRate;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}