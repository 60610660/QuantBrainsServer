using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantBrainsMonitor.Models;
using QuantBrainsMonitor.Services;

namespace QuantBrainsMonitor.ViewModels
{
    /// <summary>
    /// 主視窗 ViewModel
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMt5CommunicationService _communicationService;
        private readonly IRiskManagementService _riskManagementService;
        private readonly IEvaluationService _evaluationService;

        private bool _isConnected;
        private string _connectionStatus;
        private string _logText = "";
        private Strategy _selectedStrategy;

        public ObservableCollection<Strategy> Strategies { get; } = new ObservableCollection<Strategy>();

        // 圖表數據
        public SeriesCollection PerformanceSeries { get; private set; }
        public SeriesCollection MomentumSeries { get; private set; }
        public SeriesCollection RiskSeries { get; private set; }
        public string[] ChartLabels { get; private set; }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public string ConnectionStatus => IsConnected ? "已連接" : "未連接";

        public Strategy SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                _selectedStrategy = value;
                OnPropertyChanged();
            }
        }

        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged();
            }
        }

        // 命令
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartAllCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand PauseAllCommand { get; }
        public ICommand StartStrategyCommand { get; }
        public ICommand StopStrategyCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(
            IMt5CommunicationService communicationService,
            IRiskManagementService riskManagementService,
            IEvaluationService evaluationService)
        {
            _communicationService = communicationService;
            _riskManagementService = riskManagementService;
            _evaluationService = evaluationService;

            // 初始化圖表
            InitializeCharts();

            // 設置事件處理
            _communicationService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _communicationService.DataReceived += OnDataReceived;
            _communicationService.ErrorOccurred += OnErrorOccurred;

            // 初始化命令
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
            StartAllCommand = new RelayCommand(StartAll, () => IsConnected);
            StopAllCommand = new RelayCommand(StopAll, () => IsConnected);
            PauseAllCommand = new RelayCommand(PauseAll, () => IsConnected);
            StartStrategyCommand = new RelayCommand(StartStrategy, () => IsConnected && SelectedStrategy != null);
            StopStrategyCommand = new RelayCommand(StopStrategy, () => IsConnected && SelectedStrategy != null);
            RefreshCommand = new RelayCommand(Refresh, () => IsConnected);

            // 加載測試數據（僅用於開發）
            LoadTestData();
        }

        private void InitializeCharts()
        {
            // 初始化圖表集合
            PerformanceSeries = new SeriesCollection();
            MomentumSeries = new SeriesCollection();
            RiskSeries = new SeriesCollection();

            // 初始化標籤
            ChartLabels = new string[] { };

            // 添加默認系列
            PerformanceSeries.Add(new ColumnSeries
            {
                Title = "策略獲利",
                Values = new ChartValues<double>()
            });

            MomentumSeries.Add(new LineSeries
            {
                Title = "動能值",
                Values = new ChartValues<double>()
            });

            RiskSeries.Add(new LineSeries
            {
                Title = "風險指標",
                Values = new ChartValues<double>()
            });
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            IsConnected = isConnected;
        }

        private void OnDataReceived(object sender, string data)
        {
            try
            {
                AddLogMessage($"收到數據: {data.Substring(0, Math.Min(100, data.Length))}...");

                // 處理可能的編碼問題
                if (data.StartsWith("\ufeff"))
                {
                    data = data.Substring(1); // 移除BOM標記
                }

                // 使用較寬容的JSON設置
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                dynamic response = JsonConvert.DeserializeObject<dynamic>(data, settings);

                // 明確檢查success屬性
                bool isSuccess = false;
                if (response.success != null)
                {
                    isSuccess = Convert.ToBoolean(response.success.Value);
                }

                if (isSuccess)
                {
                    AddLogMessage($"操作成功: {response.message}");

                    if (response.data != null)
                    {
                        if (response.data is JArray strategiesArray)
                        {
                            ProcessStrategiesData(strategiesArray);
                        }
                        else if (response.data is JObject statusObject)
                        {
                            ProcessStatusData(statusObject);
                        }
                    }
                }
                else
                {
                    AddLogMessage($"操作失敗: {response.message}");
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"處理數據錯誤: {ex.Message}");
            }
        }

        private void ProcessStrategiesData(JArray strategiesArray)
        {
            Strategies.Clear();

            foreach (var item in strategiesArray)
            {
                var strategy = new Strategy
                {
                    Id = item["id"].ToObject<int>(),
                    Name = item["name"].ToString(),
                    Symbol = item["symbol"].ToString(),
                    Timeframe = item["timeframe"].ToString(),
                    Status = (StrategyStatus)item["status"].ToObject<int>(),
                    Profit = item["profit"].ToObject<double>(),
                    Drawdown = item["drawdown"].ToObject<double>(),
                    WinRate = item["winRate"].ToObject<double>(),
                    Momentum = item["momentum"].ToObject<double>(),
                    TotalTrades = item["totalTrades"].ToObject<int>(),
                    LastUpdate = item["lastUpdate"].ToObject<DateTime>()
                };

                Strategies.Add(strategy);
            }

            AddLogMessage($"已更新 {Strategies.Count} 個策略");
            UpdateCharts();
        }

        private void ProcessStatusData(JObject statusObject)
        {
            if (statusObject["account"] != null)
            {
                var account = statusObject["account"];
                AddLogMessage($"帳戶餘額: {account["balance"]} {account["currency"]}");
                AddLogMessage($"帳戶凈值: {account["equity"]} {account["currency"]}");
                AddLogMessage($"可用保證金: {account["freeMargin"]} {account["currency"]}");
            }

            if (statusObject["strategyCount"] != null)
            {
                AddLogMessage($"總策略數: {statusObject["strategyCount"]}");
            }

            if (statusObject["activeCount"] != null)
            {
                AddLogMessage($"活動策略數: {statusObject["activeCount"]}");
            }
        }

        private void OnErrorOccurred(object sender, Exception ex)
        {
            // 處理錯誤
            AddLogMessage($"錯誤: {ex.Message}");
        }

        private async void Connect()
        {
            AddLogMessage("正在連接到 MT5...");
            AddLogMessage("尋找MetaTrader 5目錄...");

            bool connected = false;
            try
            {
                connected = await _communicationService.ConnectAsync("localhost", 8888);
                if (connected)
                {
                    AddLogMessage("已成功連接到 MT5");

                    // 獲取策略列表
                    AddLogMessage("請求策略列表...");
                    await _communicationService.GetStrategiesAsync();
                }
                else
                {
                    AddLogMessage("連接失敗，請確認 MT5 已啟動並載入 QuantBrainsServer EA");
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"連接過程中發生錯誤: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            _communicationService.Disconnect();
            AddLogMessage("已斷開連接");
        }

        private async void StartAll()
        {
            AddLogMessage("正在啟動所有策略...");
            await _communicationService.StartAllAsync();
        }

        private async void StopAll()
        {
            AddLogMessage("正在停止所有策略...");
            await _communicationService.StopAllAsync();
        }

        private async void PauseAll()
        {
            AddLogMessage("正在暫停所有策略...");
            await _communicationService.PauseAllAsync();
        }

        private async void StartStrategy()
        {
            if (SelectedStrategy == null) return;

            AddLogMessage($"正在啟動策略: {SelectedStrategy.Name}");
            await _communicationService.StartStrategyAsync(SelectedStrategy.Id);
        }

        private async void StopStrategy()
        {
            if (SelectedStrategy == null) return;

            AddLogMessage($"正在停止策略: {SelectedStrategy.Name}");
            await _communicationService.StopStrategyAsync(SelectedStrategy.Id);
        }

        private async void Refresh()
        {
            AddLogMessage("正在刷新狀態...");
            await _communicationService.GetStrategiesAsync();
        }

        private void UpdateCharts()
        {
            if (Strategies.Count == 0) return;

            // 更新績效圖表
            var performanceSeries = PerformanceSeries[0] as ColumnSeries;
            if (performanceSeries != null)
            {
                performanceSeries.Values.Clear();
                foreach (var strategy in Strategies)
                {
                    performanceSeries.Values.Add(strategy.Profit);
                }
            }

            // 更新動能圖表
            var momentumSeries = MomentumSeries[0] as LineSeries;
            if (momentumSeries != null)
            {
                momentumSeries.Values.Clear();
                foreach (var strategy in Strategies)
                {
                    momentumSeries.Values.Add(strategy.Momentum);
                }
            }

            // 更新風險圖表
            var riskSeries = RiskSeries[0] as LineSeries;
            if (riskSeries != null)
            {
                riskSeries.Values.Clear();
                foreach (var strategy in Strategies)
                {
                    riskSeries.Values.Add(strategy.Drawdown * 100);
                }
            }

            // 更新標籤
            var labels = new string[Strategies.Count];
            for (int i = 0; i < Strategies.Count; i++)
            {
                labels[i] = Strategies[i].Name;
            }
            ChartLabels = labels;
            OnPropertyChanged(nameof(ChartLabels));
        }

        private void AddLogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LogText += $"[{timestamp}] {message}\n";

            // 保持日誌不超過一定長度
            if (LogText.Length > 10000)
            {
                LogText = LogText.Substring(LogText.Length - 10000);
            }
        }

        private void LoadTestData()
        {
            // 添加測試數據
            Strategies.Add(new Strategy
            {
                Id = 1,
                Name = "移動平均線策略",
                Status = StrategyStatus.Running,
                Symbol = "EURUSD",
                Timeframe = "H1",
                MagicNumber = 12345,
                Profit = 1250.50,
                Drawdown = 0.05,
                WinRate = 0.65,
                Momentum = 75.8,
                TotalTrades = 42,
                LastUpdate = DateTime.Now
            });

            Strategies.Add(new Strategy
            {
                Id = 2,
                Name = "RSI策略",
                Status = StrategyStatus.Stopped,
                Symbol = "GBPUSD",
                Timeframe = "M15",
                MagicNumber = 67890,
                Profit = -320.75,
                Drawdown = 0.12,
                WinRate = 0.45,
                Momentum = 35.2,
                TotalTrades = 38,
                LastUpdate = DateTime.Now
            });

            Strategies.Add(new Strategy
            {
                Id = 3,
                Name = "布林通道策略",
                Status = StrategyStatus.Paused,
                Symbol = "USDJPY",
                Timeframe = "H4",
                MagicNumber = 54321,
                Profit = 870.25,
                Drawdown = 0.08,
                WinRate = 0.58,
                Momentum = 62.5,
                TotalTrades = 27,
                LastUpdate = DateTime.Now
            });

            // 更新圖表
            UpdateCharts();

            // 添加日誌
            AddLogMessage("已載入測試數據");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 命令實作
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}