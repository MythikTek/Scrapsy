using OpenQA.Selenium;
using scrapsy.Enums;

namespace scrapsy.Interfaces
{
    public interface IStore
    {
        IWebDriver WebDriver { get; }

        void Run();

        void ChangeState(BotState newState);
    }
}