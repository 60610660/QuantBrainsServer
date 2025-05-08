using System.Windows;
using QuantBrainsMonitor.Services;
using QuantBrainsMonitor.ViewModels;

namespace QuantBrainsMonitor
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化服務
            var communicationService = new Mt5CommunicationService();
            var riskManagementService = new RiskManagementService();
            var evaluationService = new EvaluationService();

            // 創建主視窗 ViewModel
            var mainViewModel = new MainViewModel(
                communicationService,
                riskManagementService,
                evaluationService);

            // 創建並顯示主視窗
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
    }
}