using System;
using System.Collections.Generic;
using System.Linq;
using QuantBrainsMonitor.Models;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// 風險管理服務實作
    /// </summary>
    public class RiskManagementService : IRiskManagementService
    {
        // 配置參數
        //private readonly double _riskFreeRate = 0.02; // 無風險利率
        //private readonly double _targetRisk = 0.10;   // 目標風險水平
        //private readonly int _lookbackDays = 300;     // 評價回溯週期

        public Dictionary<int, double> CalculateRiskParityWeights(List<Strategy> strategies)
        {
            var weights = new Dictionary<int, double>();

            if (strategies == null || strategies.Count == 0)
                return weights;

            // 計算每個策略的風險貢獻
            double totalRisk = strategies.Sum(s => s.Drawdown);

            if (Math.Abs(totalRisk) < 0.0001)
            {
                // 如果總風險為零，平均分配
                double equalWeight = 1.0 / strategies.Count;
                foreach (var strategy in strategies)
                {
                    weights[strategy.Id] = equalWeight;
                }
                return weights;
            }

            // 應用風險平價原則
            foreach (var strategy in strategies)
            {
                // 風險平價權重 = 1 / (相對風險貢獻)
                double risk = Math.Max(strategy.Drawdown, 0.0001); // 防止除以零
                double weight = (1.0 / risk) / strategies.Sum(s => 1.0 / Math.Max(s.Drawdown, 0.0001));
                weights[strategy.Id] = weight;
            }

            return weights;
        }

        public double CalculateStandardizedPosition(Strategy strategy, double accountEquity)
        {
            if (strategy == null || accountEquity <= 0)
                return 0;

            // 基本部位大小（基於帳戶資金的1%風險）
            double basePosition = accountEquity * 0.01;

            // 根據回撤進行風險調整
            double riskFactor = Math.Max(1.0 - strategy.Drawdown * 2, 0.1); // 最小為10%

            // 根據動能進行調整
            double momentumFactor = Math.Max(0, strategy.Momentum) / 100.0;

            // 資金標準化部位
            double standardizedPosition = basePosition * riskFactor * momentumFactor;

            return standardizedPosition;
        }

        public bool CheckMomentumThreshold(Strategy strategy, double threshold)
        {
            if (strategy == null)
                return false;

            // 檢查策略動能是否超過門檻
            return strategy.Momentum >= threshold;
        }

        public double CalculatePortfolioRisk(List<Strategy> strategies)
        {
            if (strategies == null || strategies.Count == 0)
                return 0;

            // 這裡只是一個簡單的風險計算方法
            // 實際應用中可以使用更複雜的方法，如VaR或期望遭劇損

            // 加權平均回撤
            double totalWeight = strategies.Sum(s => Math.Max(s.Profit, 0));

            if (totalWeight <= 0)
                return strategies.Average(s => s.Drawdown);

            double portfolioRisk = strategies.Sum(s => s.Drawdown * Math.Max(s.Profit, 0) / totalWeight);

            return portfolioRisk;
        }

        public void AdjustStrategyWeights(List<Strategy> strategies)
        {
            if (strategies == null || strategies.Count == 0)
                return;

            // 計算風險平價權重
            var weights = CalculateRiskParityWeights(strategies);

            // 應用動能調整
            foreach (var strategy in strategies)
            {
                if (weights.TryGetValue(strategy.Id, out double weight))
                {
                    // 根據動能調整權重
                    double momentumAdjustment = Math.Max(0, strategy.Momentum) / 100.0;
                    weights[strategy.Id] = weight * momentumAdjustment;
                }
            }

            // 重新歸一化權重
            double totalWeight = weights.Values.Sum();
            if (totalWeight > 0)
            {
                foreach (int key in weights.Keys.ToList())
                {
                    weights[key] /= totalWeight;
                }
            }

            // 這裡可以將調整後的權重應用到策略上
        }
    }
}