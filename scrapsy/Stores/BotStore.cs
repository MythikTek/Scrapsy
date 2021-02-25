using OpenQA.Selenium;
using scrapsy.Enums;
using scrapsy.Interfaces;
using scrapsy.Services;
using System;

namespace scrapsy.Stores
{
    public abstract class BotStore : IStore
    {
        protected bool _test = true;

        protected BotStore(DirectoryService directoryService)
        {
            DirectoryService = directoryService;
        }

        private BotState BotState { get; set; }
        protected DirectoryService DirectoryService { get; set; }
        public IWebDriver WebDriver { get; set; }

        public void Run()
        {
            ChangeState(BotState.StartUp);
        }

        public void ChangeState(BotState newState)
        {
            BotState = newState;
            OnStateChange();
        }

        public void RunTest()
        {
            ChangeState(BotState.StartUp);
        }

        private void OnStateChange()
        {
            switch (BotState)
            {
                case BotState.Configure:
                    try
                    {
                        OnConfigure();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.StartUp:
                    try
                    {
                        OnStartUp();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.LogIn:
                    try
                    {
                        OnLogIn();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.CheckForItems:
                    try
                    {
                        OnCheckForItems();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.CheckOut:
                    try
                    {
                        OnCheckOut();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.ShutDown:
                    try
                    {
                        OnShutDown();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                case BotState.CheckCart:
                    try
                    {
                        OnCheckCart();
                    }
                    catch (Exception e)
                    {
                        Core.Logger.LogException(e);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract void OnConfigure();

        protected abstract void OnStartUp();

        protected abstract void OnLogIn();

        protected abstract void OnCheckForItems();

        protected abstract void OnCheckOut();

        protected abstract void OnShutDown();

        protected abstract void OnCheckCart();

        protected void TakeScreenShot()
        {
            var screenShotDriver = WebDriver as ITakesScreenshot;
            var screenShot = screenShotDriver.GetScreenshot();

            screenShot.SaveAsFile(DirectoryService.CurrentDirectory + @"\test.png", ScreenshotImageFormat.Png);
        }
    }
}