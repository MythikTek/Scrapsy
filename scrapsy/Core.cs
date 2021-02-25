using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using scrapsy.Enums;
using scrapsy.Interfaces;
using scrapsy.Services;
using scrapsy.Stores;
using Spectre.Console;
using System;

namespace scrapsy
{
    public static class Core
    {
        private static IServiceProvider serviceProvider;
        private static ILoggerService logger;

        public static ILoggerService Logger { get => logger; set => logger = value; }
        public static IServiceProvider ServiceProvider { get => serviceProvider; set => serviceProvider = value; }

        /// <summary>
        ///     Entry Point
        /// </summary>
        /// <param name="args"></param>
        private static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            ServiceProvider = BuildServiceProvider();
            AnsiConsole.Render(
                new FigletText("SCRAPSY")
                .LeftAligned()
                .Color(Color.Yellow1));
            ShowCopyRight();

            if (ShowAcknowledgement())
            {
                var store = ServiceProvider.GetService<BestBuy>();
                store?.Run();
            }
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

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
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

        private static void ShowCopyRight()
        {
            AnsiConsole.MarkupLine("[yellow]Copyright © 2021 Miguel Guzman. All Rights Reserved.[/]");
        }

        private static bool ShowAcknowledgement()
        {
            return AnsiConsole.Confirm("[yellow]By typing [[y]] you acknowledge you have read the README.TXT and agree to the terms and conditions outlined in that file.[/]");
        }
    }
}