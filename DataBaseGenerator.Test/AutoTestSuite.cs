using System;
using System.Threading.Tasks;
using DataBaseGenerator.Test.Core;
using DataBaseGenerator.Test.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataBaseGenerator.Test
{
    [TestClass]
    [DoNotParallelize]
    public class AutoTestSuite
    {        
        private ILogger _logger;
        private ITestClient _testClient;

        private string _pathToTestClient = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator\\bin\\Debug\\net8.0-windows\\DataBaseGenerator.UI.Wpf.exe";

        public AutoTestSuite()
        {
            _logger = AssemblyInitializeTests.Hosts.Services.GetRequiredService<ILogger<AutoTestSuite>>();
            _testClient = new DataBaseTestClient(_pathToTestClient);
        }


        [TestMethod]
        public async Task TestStartTestClient()
        {
            try
            {
                _logger.LogInformation("Entering in Test Start test client");

                var menuState = await _testClient.StartAsync(TimeSpan.FromSeconds(30));
                Assert.IsNotNull(menuState);

                var openWindow = await menuState.GoToStateAsync("MainWindowState", TimeSpan.FromSeconds(30));

                var window = openWindow.GetMainWindow();

                if (openWindow is MainWindowState mainWindowState)
                {
                    mainWindowState.ClickConnectButton();

                    var openDialogWindow = mainWindowState.CheckDialogWindowOpen();

                    Assert.IsTrue(openDialogWindow);
                    _logger.LogInformation("Test complet");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Test Go To Main Window State Failed");
                throw;
            }
            finally
            {
                _testClient.Kill();
            }
        }


    }
}
