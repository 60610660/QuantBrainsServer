using System;
using System.Threading.Tasks;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// MetaTrader 5 通訊服務接口
    /// </summary>
    public interface IMt5CommunicationService
    {
        /// <summary>
        /// 連接到 MT5 服務器
        /// </summary>
        Task<bool> ConnectAsync(string host, int port);

        /// <summary>
        /// 斷開連接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 發送命令到 MT5
        /// </summary>
        Task<string> SendCommandAsync(string command);

        /// <summary>
        /// 獲取策略列表
        /// </summary>
        Task<string> GetStrategiesAsync();

        /// <summary>
        /// 啟動策略
        /// </summary>
        Task<string> StartStrategyAsync(int id);

        /// <summary>
        /// 停止策略
        /// </summary>
        Task<string> StopStrategyAsync(int id);

        /// <summary>
        /// 暫停策略
        /// </summary>
        Task<string> PauseStrategyAsync(int id);

        /// <summary>
        /// 啟動所有策略
        /// </summary>
        Task<string> StartAllAsync();

        /// <summary>
        /// 停止所有策略
        /// </summary>
        Task<string> StopAllAsync();

        /// <summary>
        /// 暫停所有策略
        /// </summary>
        Task<string> PauseAllAsync();

        /// <summary>
        /// 獲取狀態
        /// </summary>
        Task<string> GetStatusAsync();

        /// <summary>
        /// 連接狀態變更事件
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// 收到數據事件
        /// </summary>
        event EventHandler<string> DataReceived;

        /// <summary>
        /// 錯誤發生事件
        /// </summary>
        event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// 是否已連接
        /// </summary>
        bool IsConnected { get; }
    }
}