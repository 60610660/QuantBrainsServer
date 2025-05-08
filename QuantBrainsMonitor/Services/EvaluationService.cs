using System;
using QuantBrainsMonitor.Models;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// 評價函數服務實作
    /// </summary>
    public class EvaluationService : IEvaluationService
    {
        // 這裡需要注意：實際評價函數需要歷史交易數據來計算
        // 為了示例簡化，這裡使用當前數據進行估算

        public double CalculateMomentum(Strategy strategy)
        {
            if (strategy == null)
                return 0;

            // 簡化的動能計算
            // 實際應用中，需要使用歷史交易數據計算

            // 結合獲利率、勝率和回撤計算動能
            double profitFactor = Math.Max(0, strategy.Profit) / Math.Max(1000, Math.Abs(strategy.Profit)) * 100;
            double winFactor = strategy.WinRate * 100;
            double drawdownFactor = Math.Max(0, 1 - strategy.Drawdown * 5) * 100;

            // 動能綜合計算 (0-100)
            double momentum = (profitFactor * 0.4 + winFactor * 0.3 + drawdownFactor * 0.3);

            return Math.Max(0, Math.Min(100, momentum));
        }

        public double CalculateSharpeRatio(Strategy strategy, double riskFreeRate = 0.02)
        {
            if (strategy == null)
                return 0;

            // 假設年化收益率
            double annualReturn = strategy.Profit / 10000; // 假設初始資金為10000

            // 假設年化波動率 (使用回撤作為簡化的風險指標)
            double volatility = Math.Max(0.01, strategy.Drawdown);

            // 夏普比率 = (年化收益率 - 無風險利率) / 年化波動率
            double sharpeRatio = (annualReturn - riskFreeRate) / volatility;

            return sharpeRatio;
        }

        public double CalculateSortinoRatio(Strategy strategy, double riskFreeRate = 0.02)
        {
            if (strategy == null)
                return 0;

            // 假設年化收益率
            double annualReturn = strategy.Profit / 10000; // 假設初始資金為10000

            // 假設下行風險 (簡化使用回撤作為下行風險)
            double downSideRisk = Math.Max(0.01, strategy.Drawdown);

            // 索提諾比率 = (年化收益率 - 無風險利率) / 下行風險
            double sortinoRatio = (annualReturn - riskFreeRate) / downSideRisk;

            return sortinoRatio;
        }

        public double CalculateOptimalF(Strategy strategy, bool robust = false)
        {
            if (strategy == null)
                return 0;

            // 簡化的Optimal-F計算
            // 實際應用中需要使用完整的交易歷史數據

            // 假設交易勝率
            double winRate = strategy.WinRate;

            // 假設盈虧比 (簡化計算)
            double winLossRatio = 1.5; // 假設值

            // 基本Optimal-F計算
            // f* = p - (1-p)/R
            // p = 勝率
            // R = 盈虧比
            double optimalF = winRate - (1 - winRate) / winLossRatio;

            // 如果要求穩健版本，降低風險
            if (robust)
            {
                optimalF *= 0.5; // 穩健係數，實際應根據回測確定
            }

            return Math.Max(0, Math.Min(1, optimalF));
        }

        public StrategyEvaluation EvaluateStrategy(Strategy strategy)
        {
            if (strategy == null)
                return null;

            var evaluation = new StrategyEvaluation
            {
                Momentum = CalculateMomentum(strategy),
                SharpeRatio = CalculateSharpeRatio(strategy),
                SortinoRatio = CalculateSortinoRatio(strategy),
                OptimalF = CalculateOptimalF(strategy, false),
                RobustOptimalF = CalculateOptimalF(strategy, true),
                ExpectedReturn = strategy.Profit / 10000 * 100, // 假設百分比收益率
                RiskAdjustedReturn = (strategy.Profit / 10000) / Math.Max(0.01, strategy.Drawdown) * 100
            };

            return evaluation;
        }
    }
}