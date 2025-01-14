using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataBaseGenerator.Test.Core;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace DataBaseGenerator.Test.Services
{
    internal class DataBaseTestClient : ITestClient
    {

        private readonly string _programPath;
        private readonly string _arguments;
        private readonly ILogger _logger = AssemblyInitializeTests.Hosts.Services.GetRequiredService<ILogger<DataBaseTestClient>>();
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
        private Application _application;

        public DataBaseTestClient(string programPath)
        {
            _programPath = programPath;
        }

        public async Task<IClientState> StartAsync(TimeSpan timeout)
        {
            _logger.LogTrace("Entering in StartAsync in DataBaseTestClient");

            Window mainWindow = null;
            ConditionFactory cf = null;


            var retryCounts = (int)(timeout.TotalMilliseconds / _retryDelay.TotalMilliseconds);

            //launch

            var startInfo = new ProcessStartInfo { FileName = _programPath, UseShellExecute = true, CreateNoWindow = false };
            _application = FlaUI.Core.Application.Launch(startInfo);

            var mainWindowWaitPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCounts, retryAttempt => _retryDelay);

            await mainWindowWaitPolicy.ExecuteAsync(async () =>
            {
                _logger.LogTrace("Try to find Main window");

                var automation = new UIA3Automation();
                var windows = _application.GetAllTopLevelWindows(automation);
                cf = new ConditionFactory(new UIA3PropertyLibrary());

                if (windows.All(w => w.FrameworkType != FrameworkType.Wpf))
                {
                    _logger.LogError("Main window did not find");
                    throw new Exception("Main window did not find");
                }

                mainWindow = windows.Single(w => w.FrameworkType == FrameworkType.Wpf);
                await Task.CompletedTask;
                _logger.LogTrace("Main window is found");
            });

            _logger.LogTrace("Wait for MainWindow state");
            //wait for Authenticate state
            var menuWindowWaitPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCounts, retryAttempt => _retryDelay);
            await menuWindowWaitPolicy.ExecuteAsync(async () =>
            {
                _logger.LogTrace("Try to find ConnectButton");

                var connectButton = mainWindow.FindFirstDescendant(cf.ByAutomationId("ConnectButton"))
                    .AsButton();
                connectButton.DrawHighlight();
                if (connectButton == null)
                {
                    _logger.LogError("Can`t go to MainWindow State");
                    throw new Exception("Button did not find");
                }

                await Task.CompletedTask;
                _logger.LogTrace("ConnectButton is found");
            });

            _logger.LogDebug("DataBaseTestClient Started");
            return new MainWindowState(mainWindow, cf);
        }


        public void Kill()
        {
            _logger.LogTrace($"DataBaseTestClient is Kill");
            _application.Kill();
        }
    }
}
