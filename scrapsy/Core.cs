using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using scrapsy.Enums;
using scrapsy.Interfaces;
using scrapsy.Services;
using scrapsy.Stores;

namespace scrapsy
{
    public static class Core
    {
        public static IServiceProvider ServiceProvider;
        public static ILoggerService Logger;


        /// <summary>
        ///     Entry Point
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            
            ServiceProvider = BuildServiceProvider();
                var store = ServiceProvider.GetService<BestBuy>();
                store?.Run();
                Logger.LogInfo("scrapsy bot has exited press any key to close command prompt");
                Console.ReadKey();
        }

        private static IServiceProvider BuildServiceProvider()
        {
            
            Logger = new LoggerService()
            {
                LogLevel = LoggerLevel.Info | LoggerLevel.Severe |
                           LoggerLevel.Warning
            };
            var services = new ServiceCollection();
            services.AddSingleton(Logger);
            services.AddSingleton<DirectoryService>();
            services.AddTransient<BestBuy>();
            return services.BuildServiceProvider();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.SaveLogs();
        }

        public static Func<IWebDriver, bool> ElementIsVisible(IWebElement element)
        {
            return driver =>
            {
                try
                {
                    return element.Displayed;
                }
                catch (Exception)
                {
                    return false;
                }
            };
        }
    }
}