using System.Collections.Generic;
using QuantBrainsMonitor.Models;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// 風險管理服務接口
    /// </summary>
    public interface IRiskManagementService
    {
        /// <summary>
        /// 計算風險平價權重
        /// </summary>
        Dictionary<int, double> CalculateRiskParityWeights(List<Strategy> strategies);

        /// <summary>
        /// 計算策略的資金標準化部位
        /// </summary>
        double CalculateStandardizedPosition(Strategy strategy, double accountEquity);

        /// <summary>
        /// 檢查策略是否達到最小動能門檻
        /// </summary>
        bool CheckMomentumThreshold(Strategy strategy, double threshold);

        /// <summary>
        /// 計算投資組合的總風險
        /// </summary>
        double CalculatePortfolioRisk(List<Strategy> strategies);

        /// <summary>
        /// 動態調整策略權重
        /// </summary>
        void AdjustStrategyWeights(List<Strategy> strategies);
    }
}