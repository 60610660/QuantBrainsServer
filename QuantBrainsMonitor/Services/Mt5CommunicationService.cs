using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// 基於文件通訊的MetaTrader 5通訊服務
    /// </summary>
    public class Mt5CommunicationService : IMt5CommunicationService
    {
        private const string CommandFile = "QuantBrains_Command.txt";
        private const string ResponseFile = "QuantBrains_Response.txt";
        private string _mt5DataPath;
        private bool _isConnected;
        private Timer _responseCheckTimer;

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<string> DataReceived;
        public event EventHandler<Exception> ErrorOccurred;

        public bool IsConnected => _isConnected;

        public Mt5CommunicationService()
        {
            _mt5DataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal");
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                // 尋找MT5的檔案夾
                _mt5DataPath = FindMT5DataFolder();

                if (string.IsNullOrEmpty(_mt5DataPath))
                {
                    throw new InvalidOperationException("無法找到MetaTrader 5的數據資料夾");
                }

                AddLogMessage($"找到MT5資料夾: {_mt5DataPath}");

                // 檢查EA是否在運行
                if (!await TestCommunicationAsync())
                {
                    throw new InvalidOperationException("無法連接到MetaTrader 5，請確保QuantBrainsServer EA已啟動");
                }

                // 啟動定時檢查回應檔案
                _responseCheckTimer = new Timer(CheckResponseFile, null, 0, 100);

                _isConnected = true;
                ConnectionStatusChanged?.Invoke(this, _isConnected);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;

            _responseCheckTimer?.Dispose();
            _responseCheckTimer = null;

            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, _isConnected);
        }

        public async Task<string> SendCommandAsync(string command)
        {
            if (!_isConnected)
                throw new InvalidOperationException("未連接到MetaTrader 5");

            try
            {
                // 刪除舊的回應檔案
                DeleteFile(ResponseFile);

                // 寫入命令檔案
                await WriteCommandFileAsync(command);

                // 等待回應
                string response = await WaitForResponseAsync();
                return response;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return null;
            }
        }

        public async Task<string> GetStrategiesAsync()
        {
            return await SendCommandAsync("GET_STRATEGIES");
        }

        public async Task<string> StartStrategyAsync(int id)
        {
            return await SendCommandAsync($"START_STRATEGY|{id}");
        }

        public async Task<string> StopStrategyAsync(int id)
        {
            return await SendCommandAsync($"STOP_STRATEGY|{id}");
        }

        public async Task<string> PauseStrategyAsync(int id)
        {
            return await SendCommandAsync($"PAUSE_STRATEGY|{id}");
        }

        public async Task<string> StartAllAsync()
        {
            return await SendCommandAsync("START_ALL");
        }

        public async Task<string> StopAllAsync()
        {
            return await SendCommandAsync("STOP_ALL");
        }

        public async Task<string> PauseAllAsync()
        {
            return await SendCommandAsync("PAUSE_ALL");
        }

        public async Task<string> GetStatusAsync()
        {
            return await SendCommandAsync("GET_STATUS");
        }

        private string FindMT5DataFolder()
        {
            try
            {
                AddLogMessage("尋找MetaTrader 5目錄...");

                // 明確的MT5目錄路徑 (根據您的截圖)
                string[] specificPaths = new string[]
                {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal", "D37B5D99C267D068A345C349C0EC90C5"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal", "D0E8209F77C8CF37AD8BF550E51FF075")
                };

                foreach (string path in specificPaths)
                {
                    AddLogMessage($"檢查路徑: {path}");

                    if (Directory.Exists(path) && Directory.Exists(Path.Combine(path, "MQL5")))
                    {
                        AddLogMessage($"找到有效的MT5目錄: {path}");
                        return path;
                    }
                }

                // 傳統搜索方法...
                // [保留您原來的代碼]

                AddLogMessage("未找到有效的MT5目錄，使用公共目錄");
                return "Common";
            }
            catch (Exception ex)
            {
                AddLogMessage($"查找MT5目錄時出錯: {ex.Message}");
                return "Common";
            }
        }

        private async Task<bool> TestCommunicationAsync()
        {
            try
            {
                AddLogMessage("測試與MT5的通訊...");

                // 刪除舊檔案
                DeleteFile(CommandFile);
                DeleteFile(ResponseFile);

                // 顯式指定命令路徑用於測試
                string commandPath1 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MetaQuotes", "Terminal", "D0E8209F77C8CF37AD8BF550E51FF075", "MQL5", "Files", CommandFile);

                string commandPath2 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MetaQuotes", "Terminal", "D37B5D99C267D068A345C349C0EC90C5", "MQL5", "Files", CommandFile);

                string commandPathCommon = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "MetaQuotes", "Terminal", "Common", "Files", CommandFile);

                AddLogMessage($"嘗試寫入測試命令到: \n{commandPath1}\n{commandPath2}\n{commandPathCommon}");

                // 嘗試寫入到所有路徑
                try { File.WriteAllText(commandPath1, "GET_STATUS", Encoding.UTF8); } catch { }
                try { File.WriteAllText(commandPath2, "GET_STATUS", Encoding.UTF8); } catch { }
                try { File.WriteAllText(commandPathCommon, "GET_STATUS", Encoding.UTF8); } catch { }

                // 等待回應，超時10秒
                string response = await WaitForResponseAsync(10000);

                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                AddLogMessage($"測試通訊時出錯: {ex.Message}");
                return false;
            }
        }

        private async Task WriteCommandFileAsync(string command)
        {
            try
            {
                // 使用已知可行的路徑
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MetaQuotes", "Terminal", "D0E8209F77C8CF37AD8BF550E51FF075",
                    "MQL5", "Files", CommandFile);

                AddLogMessage($"寫入命令到: {path}");

                // 確保目錄存在
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // 使用無BOM的UTF-8編碼
                using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
                {
                    writer.Write(command);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                AddLogMessage($"寫入命令檔案時出錯: {ex.Message}");
                throw;
            }
        }

        private async Task TryWriteFileAsync(string filePath, string content)
        {
            try
            {
                AddLogMessage($"嘗試寫入命令到: {filePath}");

                // 確保目錄存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // 寫入檔案
                File.WriteAllText(filePath, content, Encoding.UTF8);

                AddLogMessage($"已成功寫入命令到: {filePath}");
            }
            catch (Exception ex)
            {
                AddLogMessage($"寫入到 {filePath} 失敗: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task<string> WaitForResponseAsync(int timeout = 10000)
        {
            // 嘗試從多個可能的位置讀取
            List<string> possiblePaths = new List<string>();

            // 1. MT5實例目錄
            if (_mt5DataPath != "Common")
            {
                possiblePaths.Add(Path.Combine(_mt5DataPath, "MQL5", "Files", ResponseFile));
            }

            // 2. 公共目錄
            possiblePaths.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MetaQuotes", "Terminal", "Common", "Files", ResponseFile));

            // 3. 用戶目錄下所有MT5實例
            string userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal");

            if (Directory.Exists(userPath))
            {
                string[] directories = Directory.GetDirectories(userPath);
                foreach (string dir in directories)
                {
                    possiblePaths.Add(Path.Combine(dir, "MQL5", "Files", ResponseFile));
                }
            }

            AddLogMessage($"將檢查以下路徑獲取回應: {string.Join(", ", possiblePaths)}");

            int elapsed = 0;
            int checkInterval = 100;

            while (elapsed < timeout)
            {
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            AddLogMessage($"找到回應檔案: {path}");

                            // 讀取回應
                            string response = File.ReadAllText(path, Encoding.UTF8);

                            AddLogMessage($"成功讀取回應: {response}");

                            // 刪除回應檔案
                            try
                            {
                                File.Delete(path);
                                AddLogMessage($"已刪除回應檔案: {path}");
                            }
                            catch (Exception ex)
                            {
                                AddLogMessage($"刪除回應檔案失敗: {ex.Message}");
                            }

                            return response;
                        }
                        catch (Exception ex)
                        {
                            AddLogMessage($"讀取 {path} 失敗: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(checkInterval);
                elapsed += checkInterval;
            }

            throw new TimeoutException("等待回應超時");
        }

        private void CheckResponseFile(object state)
        {
            if (!_isConnected)
                return;

            // 檢查多個可能的位置
            List<string> possiblePaths = new List<string>();

            // 1. MT5實例目錄
            if (_mt5DataPath != "Common")
            {
                possiblePaths.Add(Path.Combine(_mt5DataPath, "MQL5", "Files", ResponseFile));
            }

            // 2. 公共目錄
            possiblePaths.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MetaQuotes", "Terminal", "Common", "Files", ResponseFile));

            // 3. 用戶目錄下所有MT5實例
            string userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal");

            if (Directory.Exists(userPath))
            {
                string[] directories = Directory.GetDirectories(userPath);
                foreach (string dir in directories)
                {
                    possiblePaths.Add(Path.Combine(dir, "MQL5", "Files", ResponseFile));
                }
            }

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        // 讀取回應
                        string response = File.ReadAllText(path, Encoding.UTF8);

                        // 刪除回應檔案
                        try { File.Delete(path); } catch { }

                        // 觸發數據接收事件
                        DataReceived?.Invoke(this, response);
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, new Exception($"讀取 {path} 失敗: {ex.Message}"));
                    }
                }
            }
        }

        private void DeleteFile(string filename)
        {
            // 嘗試刪除多個可能位置的檔案
            List<string> possiblePaths = new List<string>();

            // 1. MT5實例目錄
            if (_mt5DataPath != "Common")
            {
                possiblePaths.Add(Path.Combine(_mt5DataPath, "MQL5", "Files", filename));
            }

            // 2. 公共目錄
            possiblePaths.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MetaQuotes", "Terminal", "Common", "Files", filename));

            // 3. 用戶目錄下所有MT5實例
            string userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MetaQuotes", "Terminal");

            if (Directory.Exists(userPath))
            {
                string[] directories = Directory.GetDirectories(userPath);
                foreach (string dir in directories)
                {
                    possiblePaths.Add(Path.Combine(dir, "MQL5", "Files", filename));
                }
            }

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                        AddLogMessage($"已刪除檔案: {path}");
                    }
                    catch (Exception ex)
                    {
                        AddLogMessage($"刪除 {path} 失敗: {ex.Message}");
                    }
                }
            }
        }

        private void AddLogMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
            // 可以選擇是否通過錯誤事件傳播日誌
            // ErrorOccurred?.Invoke(this, new Exception(message));
        }
    }
}