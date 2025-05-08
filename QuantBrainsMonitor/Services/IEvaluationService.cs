using QuantBrainsMonitor.Models;

namespace QuantBrainsMonitor.Services
{
    /// <summary>
    /// 評價函數服務接口
    /// </summary>
    public interface IEvaluationService
    {
        /// <summary>
        /// 計算策略動能（期望值）
        /// </summary>
        double CalculateMomentum(Strategy strategy);

        /// <summary>
        /// 計算夏普比率
        /// </summary>
        double CalculateSharpeRatio(Strategy strategy, double riskFreeRate = 0.02);

        /// <summary>
        /// 計算索提諾比率
        /// </summary>
        double CalculateSortinoRatio(Strategy strategy, double riskFreeRate = 0.02);

        /// <summary>
        /// 計算Optimal-F值
        /// </summary>
        double CalculateOptimalF(Strategy strategy, bool robust = false);

        /// <summary>
        /// 評估策略績效
        /// </summary>
        StrategyEvaluation EvaluateStrategy(Strategy strategy);
    }

    /// <summary>
    /// 策略評估結果
    /// </summary>
    public class StrategyEvaluation
    {
        public double Momentum { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double OptimalF { get; set; }
        public double RobustOptimalF { get; set; }
        public double ExpectedReturn { get; set; }
        public double RiskAdjustedReturn { get; set; }
    }
}