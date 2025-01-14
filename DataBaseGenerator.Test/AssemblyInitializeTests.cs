using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataBaseGenerator.Test
{
    [TestClass]
    [DoNotParallelize]
    public static class AssemblyInitializeTests
    {
        private static ILogger _logger;
        private const string _namedLoggerEmulator = "Emulator";
        private const string _namedLoggerDevices = "Devices";
        public static IHost Hosts { get; private set; }

        [AssemblyInitialize]
        public static void BeforeTestSuites(TestContext testContext)
        {
            ConfigBuilder();
            _logger.LogTrace("Enter in BeforeTestSuites in AssemblyInitialize");
        }

        public static void ConfigBuilder()
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = AppContext.BaseDirectory
            });

            builder.Services.RegisterLogger();

            Hosts = builder.Build();

            var logger = Hosts.Services.GetRequiredService<ILoggerFactory>();

            _logger = logger.CreateLogger(_namedLoggerEmulator);

        }

        private static IServiceCollection RegisterLogger(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFiles(multiBuilder =>
                {
                    multiBuilder.AddFile(_namedLoggerDevices);
                    multiBuilder.AddFile(_namedLoggerEmulator);
                });
            });

            return serviceCollection;
        }










    }
}
