//+------------------------------------------------------------------+
//|                                         QuantBrainsServer.mq5    |
//|                                    Copyright 2025, QuantBrains    |
//|                                       https://quantbrains.club/   |
//+------------------------------------------------------------------+
#property copyright "Copyright 2025, QuantBrains"
#property link      "https://quantbrains.club/"
#property version   "1.00"

#include <Trade\Trade.mqh>
#include <Arrays\ArrayObj.mqh>
#include <Files\FileTxt.mqh>
#include <Files\File.mqh>
#include <Trade\PositionInfo.mqh>
#include <Trade\OrderInfo.mqh>

// 策略狀態枚舉
enum ENUM_STRATEGY_STATUS
{
   STRATEGY_STOPPED = 0,  // 已停止
   STRATEGY_RUNNING = 1,  // 運行中
   STRATEGY_PAUSED  = 2,  // 已暫停
   STRATEGY_ERROR   = 3   // 錯誤
};

// 策略資訊類別 - 改為繼承自CObject
class StrategyInfo : public CObject
{
public:
   int               id;                 // 策略ID (魔術號)
   string            name;               // 策略名稱
   string            symbol;             // 交易品種
   string            timeframe;          // 時間週期
   int               status;             // 策略狀態 (使用 ENUM_STRATEGY_STATUS 的值)
   double            profit;             // 當前獲利
   double            drawdown;           // 回撤
   double            winRate;            // 勝率
   double            momentum;           // 動能值
   int               totalTrades;        // 總交易數
   datetime          lastUpdate;         // 最後更新時間
   
   // 必須添加虛擬析構函數
   virtual ~StrategyInfo() {}
};

// 全局變量
CArrayObj     g_strategies;              // 策略列表
CTrade        g_trade;                   // 交易物件
bool          g_serverRunning = false;   // 伺服器運行狀態
string        g_lastError = "";          // 最後錯誤信息
string        g_commandFile = "QuantBrains_Command.txt";   // 命令檔案
string        g_responseFile = "QuantBrains_Response.txt"; // 回應檔案
int           g_updateInterval = 100;     // 檢查檔案的間隔(毫秒)
datetime      g_lastCheckTime = 0;        // 上次檢查命令檔案的時間

//+------------------------------------------------------------------+
//| 專家顧問初始化函數                                                |
//+------------------------------------------------------------------+
int OnInit()
{
   // 初始化策略列表
   InitializeStrategies();
   
   // 啟動伺服器
   if(!StartServer())
   {
      Print("伺服器啟動失敗: ", g_lastError);
      return INIT_FAILED;
   }
   
   Print("QuantBrains 伺服器已啟動");
   return INIT_SUCCEEDED;
}

//+------------------------------------------------------------------+
//| 專家顧問反初始化函數                                              |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   StopServer();
   Print("QuantBrains 伺服器已停止");
   
   // 清理策略列表
   for(int i = g_strategies.Total() - 1; i >= 0; i--)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      delete strategy;
   }
   g_strategies.Clear();
}

//+------------------------------------------------------------------+
//| 專家顧問報價函數                                                  |
//+------------------------------------------------------------------+
void OnTick()
{
   // 處理通訊請求
   ProcessCommunication();
   
   // 更新策略狀態
   UpdateStrategies();
}

//+------------------------------------------------------------------+
//| 啟動伺服器                                                        |
//+------------------------------------------------------------------+
bool StartServer()
{
   if(g_serverRunning)
      return true;
      
   // 初始化檔案通訊
   g_serverRunning = true;
   g_lastCheckTime = 0;
   
   // 清理舊的檔案
   FileDelete(g_commandFile);
   FileDelete(g_responseFile);
   
   return true;
}

//+------------------------------------------------------------------+
//| 停止伺服器                                                        |
//+------------------------------------------------------------------+
void StopServer()
{
   if(!g_serverRunning)
      return;
      
   // 清理檔案
   FileDelete(g_commandFile);
   FileDelete(g_responseFile);
   
   g_serverRunning = false;
}

//+------------------------------------------------------------------+
//| 初始化策略列表                                                    |
//+------------------------------------------------------------------+
void InitializeStrategies()
{
   // 清空策略列表
   for(int i = g_strategies.Total() - 1; i >= 0; i--)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      delete strategy;
   }
   g_strategies.Clear();
   
   // 添加策略1 - 移動平均線策略
   StrategyInfo* strategy1 = new StrategyInfo();
   strategy1.id = 12345;
   strategy1.name = "移動平均線策略";
   strategy1.symbol = "EURUSD";
   strategy1.timeframe = "H1";
   strategy1.status = STRATEGY_RUNNING;
   strategy1.profit = 1250.50;
   strategy1.drawdown = 0.05;
   strategy1.winRate = 0.65;
   strategy1.momentum = 75.8;
   strategy1.totalTrades = 42;
   strategy1.lastUpdate = TimeCurrent();
   g_strategies.Add(strategy1);
   
   // 添加策略2 - RSI策略
   StrategyInfo* strategy2 = new StrategyInfo();
   strategy2.id = 67890;
   strategy2.name = "RSI策略";
   strategy2.symbol = "GBPUSD";
   strategy2.timeframe = "M15";
   strategy2.status = STRATEGY_STOPPED;
   strategy2.profit = -320.75;
   strategy2.drawdown = 0.12;
   strategy2.winRate = 0.45;
   strategy2.momentum = 35.2;
   strategy2.totalTrades = 38;
   strategy2.lastUpdate = TimeCurrent();
   g_strategies.Add(strategy2);
   
   // 添加策略3 - 布林通道策略
   StrategyInfo* strategy3 = new StrategyInfo();
   strategy3.id = 54321;
   strategy3.name = "布林通道策略";
   strategy3.symbol = "USDJPY";
   strategy3.timeframe = "H4";
   strategy3.status = STRATEGY_PAUSED;
   strategy3.profit = 870.25;
   strategy3.drawdown = 0.08;
   strategy3.winRate = 0.58;
   strategy3.momentum = 62.5;
   strategy3.totalTrades = 27;
   strategy3.lastUpdate = TimeCurrent();
   g_strategies.Add(strategy3);
}

//+------------------------------------------------------------------+
//| 更新策略狀態                                                      |
//+------------------------------------------------------------------+
void UpdateStrategies()
{
   // 在實際應用中，這裡會更新每個策略的狀態、獲利等資訊
   // 此處僅作示範，實際實現應根據真實策略的運行狀況
   
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      if(strategy.status == STRATEGY_RUNNING)
      {
         // 模擬策略運行，隨機更新獲利
         if(MathRand() % 2 == 0)
            strategy.profit += MathRand() % 10;
         else
            strategy.profit -= MathRand() % 5;
            
         // 更新動能值
         strategy.momentum = MathMax(0, MathMin(100, strategy.momentum + (MathRand() % 10 - 5) / 10.0));
         
         // 更新最後更新時間
         strategy.lastUpdate = TimeCurrent();
      }
   }
}

//+------------------------------------------------------------------+
//| 處理通訊請求                                                      |
//+------------------------------------------------------------------+
void ProcessCommunication()
{
   if(!g_serverRunning)
   {
      Print("伺服器未運行，跳過通訊處理");
      return;
   }
      
   // 定期檢查命令檔案
   datetime currentTime = TimeCurrent();
   if(currentTime > g_lastCheckTime + g_updateInterval/1000)
   {
      g_lastCheckTime = currentTime;
      Print("檢查命令檔案...");
      
      // 檢查一般文件夾中的命令檔案
      Print("檢查一般文件夾中的命令檔案: ", g_commandFile);
      if(FileIsExist(g_commandFile))
      {
         Print("找到命令文件(一般文件夾): ", g_commandFile);
         
         // 讀取命令
         Print("嘗試讀取命令文件內容...");
         string command = ReadFileContent(g_commandFile);
         
         // 刪除檔案，表示已讀取
         Print("嘗試刪除命令文件...");
         if(FileDelete(g_commandFile))
            Print("已成功刪除命令文件");
         else
            Print("刪除命令文件失敗，錯誤碼: ", GetLastError());
         
         if(command != "")
         {
            Print("收到命令: ", command);
            
            // 處理命令
            string response = ProcessCommand(command);
            
            // 寫入回應
            Print("嘗試寫入回應...");
            WriteFileContent(g_responseFile, response);
         }
         else
         {
            Print("命令文件為空");
         }
      }
      else
      {
         Print("一般文件夾中未找到命令文件");
      }
      
      // 檢查公共文件夾中的命令檔案
      Print("檢查公共文件夾中的命令檔案: ", g_commandFile);
      if(FileIsExist(g_commandFile, FILE_COMMON))
      {
         Print("找到命令文件(公共文件夾): ", g_commandFile);
         
         // 讀取命令
         Print("嘗試開啟公共文件夾中的命令文件...");
         int handle = FileOpen(g_commandFile, FILE_READ|FILE_TXT|FILE_ANSI|FILE_COMMON);
         
         if(handle != INVALID_HANDLE)
         {
            Print("成功開啟公共文件夾中的命令文件");
            string command = FileReadString(handle);
            FileClose(handle);
            Print("已讀取命令內容: ", command);
            
            // 刪除檔案，表示已讀取
            Print("嘗試刪除公共文件夾中的命令文件...");
            if(FileDelete(g_commandFile, FILE_COMMON))
               Print("已成功刪除公共文件夾中的命令文件");
            else
               Print("刪除公共文件夾中的命令文件失敗，錯誤碼: ", GetLastError());
            
            if(command != "")
            {
               Print("收到命令: ", command);
               
               // 處理命令
               string response = ProcessCommand(command);
               
               // 嘗試寫入回應到公共文件夾
               Print("嘗試寫入回應到公共文件夾...");
               int responseHandle = FileOpen(g_responseFile, FILE_WRITE|FILE_TXT|FILE_ANSI|FILE_COMMON);
               
               if(responseHandle != INVALID_HANDLE)
               {
                  FileWriteString(responseHandle, response);
                  FileClose(responseHandle);
                  Print("已成功寫入回應到公共文件夾: ", response);
               }
               else
               {
                  Print("無法開啟公共文件夾中的回應檔案，錯誤碼: ", GetLastError());
               }
            }
            else
            {
               Print("公共文件夾中的命令文件為空");
            }
         }
         else
         {
            Print("無法開啟公共文件夾中的命令檔案，錯誤碼: ", GetLastError());
         }
      }
      else
      {
         Print("公共文件夾中未找到命令文件");
      }
   }
}

//+------------------------------------------------------------------+
//| 讀取檔案內容                                                     |
//+------------------------------------------------------------------+
string ReadFileContent(string filename)
{
   Print("嘗試讀取檔案: ", filename);
   int handle = FileOpen(filename, FILE_READ|FILE_TXT|FILE_ANSI);
   if(handle == INVALID_HANDLE)
   {
      Print("無法開啟檔案: ", filename, " 錯誤碼: ", GetLastError());
      return "";
   }
      
   string content = "";
   while(!FileIsEnding(handle))
   {
      content += FileReadString(handle);
   }
   
   FileClose(handle);
   Print("已成功讀取檔案內容: ", content);
   return content;
}

//+------------------------------------------------------------------+
//| 寫入檔案內容                                                     |
//+------------------------------------------------------------------+
void WriteFileContent(string filename, string content)
{
   Print("嘗試寫入檔案: ", filename);
   int handle = FileOpen(filename, FILE_WRITE|FILE_TXT|FILE_ANSI);
   if(handle == INVALID_HANDLE)
   {
      Print("無法開啟檔案: ", filename, " 錯誤碼: ", GetLastError());
      
      // 嘗試寫入到公共目錄
      Print("嘗試寫入到公共目錄: ", filename);
      handle = FileOpen(filename, FILE_WRITE|FILE_TXT|FILE_ANSI|FILE_COMMON);
      if(handle == INVALID_HANDLE)
      {
         Print("也無法開啟公共目錄中的檔案，錯誤碼: ", GetLastError());
         return;
      }
      else
      {
         Print("成功開啟公共目錄中的檔案");
      }
   }
   else
   {
      Print("成功開啟檔案");
   }
      
   FileWriteString(handle, content);
   FileClose(handle);
   Print("已成功寫入檔案內容: ", content);
}

//+------------------------------------------------------------------+
//| 處理命令                                                          |
//+------------------------------------------------------------------+
string ProcessCommand(string command)
{
   Print("收到命令: ", command);
   
   // 解析命令
   string parts[];
   int partCount = StringSplit(command, '|', parts);
   
   if(partCount == 0)
      return CreateErrorResponse("無效的命令格式");
      
   string cmd = parts[0];
   
   // 根據命令類型處理
   if(cmd == "GET_STRATEGIES")
   {
      return GetStrategiesResponse();
   }
   else if(cmd == "START_STRATEGY" && partCount > 1)
   {
      int id = (int)StringToInteger(parts[1]);
      return StartStrategyResponse(id);
   }
   else if(cmd == "STOP_STRATEGY" && partCount > 1)
   {
      int id = (int)StringToInteger(parts[1]);
      return StopStrategyResponse(id);
   }
   else if(cmd == "PAUSE_STRATEGY" && partCount > 1)
   {
      int id = (int)StringToInteger(parts[1]);
      return PauseStrategyResponse(id);
   }
   else if(cmd == "START_ALL")
   {
      return StartAllResponse();
   }
   else if(cmd == "STOP_ALL")
   {
      return StopAllResponse();
   }
   else if(cmd == "PAUSE_ALL")
   {
      return PauseAllResponse();
   }
   else if(cmd == "GET_STATUS")
   {
      return GetStatusResponse();
   }
   
   return CreateErrorResponse("未知的命令: " + cmd);
}

//+------------------------------------------------------------------+
//| 創建錯誤回應                                                      |
//+------------------------------------------------------------------+
string CreateErrorResponse(string message)
{
   return "{\"success\":false,\"message\":\"" + message + "\",\"data\":null}\n";
}

//+------------------------------------------------------------------+
//| 創建成功回應                                                      |
//+------------------------------------------------------------------+
string CreateSuccessResponse(string message, string data)
{
   return "{\"success\":true,\"message\":\"" + message + "\",\"data\":" + data + "}\n";
}

//+------------------------------------------------------------------+
//| 獲取策略列表回應                                                  |
//+------------------------------------------------------------------+
string GetStrategiesResponse()
{
   string strategiesJson = "[";
   
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      strategiesJson += "{";
      strategiesJson += "\"id\":" + IntegerToString(strategy.id) + ",";
      strategiesJson += "\"name\":\"" + strategy.name + "\",";
      strategiesJson += "\"symbol\":\"" + strategy.symbol + "\",";
      strategiesJson += "\"timeframe\":\"" + strategy.timeframe + "\",";
      strategiesJson += "\"status\":" + IntegerToString(strategy.status) + ",";
      strategiesJson += "\"profit\":" + DoubleToString(strategy.profit, 2) + ",";
      strategiesJson += "\"drawdown\":" + DoubleToString(strategy.drawdown, 2) + ",";
      strategiesJson += "\"winRate\":" + DoubleToString(strategy.winRate, 2) + ",";
      strategiesJson += "\"momentum\":" + DoubleToString(strategy.momentum, 2) + ",";
      strategiesJson += "\"totalTrades\":" + IntegerToString(strategy.totalTrades) + ",";
      strategiesJson += "\"lastUpdate\":\"" + TimeToString(strategy.lastUpdate, TIME_DATE|TIME_SECONDS) + "\"";
      strategiesJson += "}";
      
      if(i < g_strategies.Total() - 1)
         strategiesJson += ",";
   }
   
   strategiesJson += "]";
   
   return CreateSuccessResponse("獲取策略列表成功", strategiesJson);
}

//+------------------------------------------------------------------+
//| 啟動策略回應                                                      |
//+------------------------------------------------------------------+
string StartStrategyResponse(int id)
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      if(strategy.id == id)
      {
         strategy.status = STRATEGY_RUNNING;
         return CreateSuccessResponse("策略已啟動", "\"" + strategy.name + "\"");
      }
   }
   
   return CreateErrorResponse("找不到ID為 " + IntegerToString(id) + " 的策略");
}

//+------------------------------------------------------------------+
//| 停止策略回應                                                      |
//+------------------------------------------------------------------+
string StopStrategyResponse(int id)
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      if(strategy.id == id)
      {
         strategy.status = STRATEGY_STOPPED;
         return CreateSuccessResponse("策略已停止", "\"" + strategy.name + "\"");
      }
   }
   
   return CreateErrorResponse("找不到ID為 " + IntegerToString(id) + " 的策略");
}

//+------------------------------------------------------------------+
//| 暫停策略回應                                                      |
//+------------------------------------------------------------------+
string PauseStrategyResponse(int id)
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      if(strategy.id == id)
      {
         strategy.status = STRATEGY_PAUSED;
         return CreateSuccessResponse("策略已暫停", "\"" + strategy.name + "\"");
      }
   }
   
   return CreateErrorResponse("找不到ID為 " + IntegerToString(id) + " 的策略");
}

//+------------------------------------------------------------------+
//| 啟動所有策略回應                                                  |
//+------------------------------------------------------------------+
string StartAllResponse()
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      strategy.status = STRATEGY_RUNNING;
   }
   
   return CreateSuccessResponse("所有策略已啟動", IntegerToString(g_strategies.Total()));
}

//+------------------------------------------------------------------+
//| 停止所有策略回應                                                  |
//+------------------------------------------------------------------+
string StopAllResponse()
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      strategy.status = STRATEGY_STOPPED;
   }
   
   return CreateSuccessResponse("所有策略已停止", IntegerToString(g_strategies.Total()));
}

//+------------------------------------------------------------------+
//| 暫停所有策略回應                                                  |
//+------------------------------------------------------------------+
string PauseAllResponse()
{
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      strategy.status = STRATEGY_PAUSED;
   }
   
   return CreateSuccessResponse("所有策略已暫停", IntegerToString(g_strategies.Total()));
}

//+------------------------------------------------------------------+
//| 獲取狀態回應                                                      |
//+------------------------------------------------------------------+
string GetStatusResponse()
{
   string statusJson = "{";
   
   statusJson += "\"serverTime\":\"" + TimeToString(TimeCurrent(), TIME_DATE|TIME_SECONDS) + "\",";
   statusJson += "\"strategyCount\":" + IntegerToString(g_strategies.Total()) + ",";
   statusJson += "\"activeCount\":" + IntegerToString(GetActiveStrategyCount()) + ",";
   statusJson += "\"account\":{";
   statusJson += "\"balance\":" + DoubleToString(AccountInfoDouble(ACCOUNT_BALANCE), 2) + ",";
   statusJson += "\"equity\":" + DoubleToString(AccountInfoDouble(ACCOUNT_EQUITY), 2) + ",";
   statusJson += "\"margin\":" + DoubleToString(AccountInfoDouble(ACCOUNT_MARGIN), 2) + ",";
   statusJson += "\"freeMargin\":" + DoubleToString(AccountInfoDouble(ACCOUNT_MARGIN_FREE), 2) + ",";
   statusJson += "\"currency\":\"" + AccountInfoString(ACCOUNT_CURRENCY) + "\",";
   statusJson += "\"leverage\":" + IntegerToString(AccountInfoInteger(ACCOUNT_LEVERAGE));
   statusJson += "}";
   
   statusJson += "}";
   
   return CreateSuccessResponse("獲取狀態成功", statusJson);
}

//+------------------------------------------------------------------+
//| 獲取活動策略數量                                                  |
//+------------------------------------------------------------------+
int GetActiveStrategyCount()
{
   int count = 0;
   
   for(int i = 0; i < g_strategies.Total(); i++)
   {
      StrategyInfo* strategy = (StrategyInfo*)g_strategies.At(i);
      
      if(strategy.status == STRATEGY_RUNNING)
         count++;
   }
   
   return count;
}
//+------------------------------------------------------------------+
//| 專家顧問報價函數                                                  |
//+------------------------------------------------------------------+
