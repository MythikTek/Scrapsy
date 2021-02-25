using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using scrapsy.Enums;
using scrapsy.Services;
using scrapsy.Stores.Data;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace scrapsy.Stores
{
    public class BestBuy : BotStore
    {
        #region constants
        private const string Cart = "https://www.bestbuy.com/cart";
        private const string UsernameFieldId = "//*[@id='fld-e']";
        private const string PasswordFieldId = "fld-p1";
        private const string SignInElement = "//*[@data-track='Sign In']";
        private const string CheckOutElement = "//*[@data-track='Checkout - Top']";
        private const string CvvFieldId = "credit-card-cvv";
        private string BuyButtonElement => $"//*[@data-sku-id='{_currentSku}']";
        private const string PlaceOrderButtonCss = ".button__fast-track";
        #endregion

        #region variables
        private const string OneShot = "One Shot";
        private const string TestMode = "Test Mode";

        private string _email;
        private string _password;
        private string _cvv;
        private string _currentSku = "";

        private BestBuyConfig _config;
        private HashSet<string> _options = new HashSet<string>();

        private WebDriverWait _waitFor;
        #endregion

        #region constructors
        public BestBuy(DirectoryService directoryService) : base(directoryService)
        {
        }
        #endregion

        #region Start Up
        protected override void OnStartUp()
        {
            SetUpAuthentication();
            //_config = new BestBuyConfig();
            ChangeState(BotState.Configure);
        }
        
        private void SetUpAuthentication()
        {
            _email = AnsiConsole.Ask<string>("[yellow]Please enter your Best Buy email[/]");
            _password = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Please enter your Best Buy password[/]")
                    .Secret());
            _cvv = AnsiConsole.Ask<string>(
                "[yellow]Please enter your credit card cvv number[/]");
            Core.Logger.LogInfo("Account info stored");
        }
        #endregion

        #region Configuration
        protected override void OnConfigure()
        {
            ConfigureBot();

            _options = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("[yellow1]Select bot options[/]")
                    .PageSize(4)
                    .AddChoices(OneShot, TestMode)).ToHashSet();

            var optionsChosen = string.Join(" | ", _options);

            Core.Logger.LogTrace($"Options Chosen: {optionsChosen}");

            StartWebDriver();
        }
        
        private void ConfigureBot()
        {
            //check if configDirectory exists
            var configDir = DirectoryService.ConfigDirectory + @"\BestBuy";

            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            //check if any configuration files exist
            var configExists = DirectoryService.CheckIfFilesExist(configDir, "*.config");

            //set up configuration actions
            var newConfig = "Create New Configuration";
            var loadConfig = "Load Existing Configuration";

            //create configuration actions list
            var configActions = new List<string> { newConfig };

            //add load from file option if configurations already exist
            if (configExists)
                configActions.Add(loadConfig);

            //ask user how bot should be configured
            var configType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow1]How would you like to configure Scrapsy?[/]")
                .PageSize(3)
                .AddChoices(configActions));

            if (configType == newConfig)
                RunNewConfiguration();
            else if (configType == loadConfig)
                ConfigureFromFile();
        }
        
        private void RunNewConfiguration()
        {
            _config = new BestBuyConfig();

            //ask user for a link to an item
            var productLink = AnsiConsole.Ask<string>("[yellow1]Please Enter url to your desired item[/]");
            _config.Links.Add(productLink);
            Core.Logger.LogInfo($"{productLink} added to configuration");

            //ask if user would like to add any additional links
            while (true)
            {
                if (AnsiConsole.Confirm("[yellow1]Would you like to add another url?[/]"))
                {
                    productLink = AnsiConsole.Ask<string>("[yellow1]Please Enter url to your desired item[/]");
                    _config.Links.Add(productLink);
                    Core.Logger.LogInfo($"{productLink} added to configuration");
                }
                else
                {
                    break;
                }
            }

            //request if user wants to save file
            if (AnsiConsole.Confirm("[yellow1]Would you like to save this configuration for future use?[/]"))
            {
                var configName = AnsiConsole.Ask<string>("[yellow]Please enter a name for your configuration.[/]");

                var filePath = DirectoryService.ConfigDirectory + @"\BestBuy\" + configName + ".config";
                var json = JsonSerializer.Serialize<BestBuyConfig>(_config);

                File.WriteAllText(filePath, json);
            }
        }
        
        private void ConfigureFromFile()
        {
            var configFiles = DirectoryService.GetFilesInDirectory(DirectoryService.ConfigDirectory + @"\BestBuy", "*.config")
                .ToDictionary(x => Path.GetFileNameWithoutExtension(x.Name), x => x);

            var fileToLoad = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow1]Which configuration would you like to load[/]")
                .PageSize(configFiles.Keys.Count >= 3 ? configFiles.Keys.Count : 3)
                .AddChoices(configFiles.Keys.ToArray()));

            string json;
            var settingsFile = configFiles[fileToLoad].FullName;

            json = File.ReadAllText(settingsFile);

            _config = JsonSerializer.Deserialize<BestBuyConfig>(json);
        }
        
        private void StartWebDriver()
        {
            //create chrome options
            var chromeOptions = new ChromeOptions();

            //add headless mode if enabled
            if (_options.Contains("Headless Mode"))
                chromeOptions.AddArguments("--headless");

            //create web driver
            WebDriver = new ChromeDriver(DirectoryService.CurrentDirectory, chromeOptions);

            //set up WebDriverWait object
            _waitFor = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(_config.Timeout));

            //get product data

            //change state to start logging in
            ChangeState(BotState.LogIn);
        }
        #endregion

        #region LifeTime
        protected override void OnLogIn()
        {
            ChangeState(BotState.CheckCart);
        }

        protected override void OnCheckForItems()
        {
            /*TODO: ADD FUNCTIONALITY TO CHECK IF PRICE IS WITHIN RANGE IN CONFIG FILE*/

            foreach (var link in _config.Links)
            {
                var wait = Task.Factory.StartNew(() =>
                {
                    Core.Logger.LogTrace("Check Item Delay Started...");
                    Thread.Sleep(_config.Delay);
                    Core.Logger.LogTrace("Check Item Delay Completed...");
                });

                Core.Logger.LogInfo($"Navigating to product url: {link}");
                WebDriver.Navigate().GoToUrl(link); //set the url

                //get buy button element
                Core.Logger.LogTrace("Getting Buy Now Button");

                //get sku from url
                _currentSku = link.Split('=').Last();

                var buyBtn = _waitFor.Until(x => x.FindElement(By.XPath(BuyButtonElement)));

                //check if available

                if (buyBtn.Enabled)
                {
                    Core.Logger.LogInfo("Product available buying now!");
                    /*TODO: Add product in-stock alert*/

                    ChangeState(BotState.CheckOut);
                    return;
                }

                Core.Logger.LogWarning($"not in stock.. Checking next item");
                wait.Wait();
            }

            ChangeState(BotState.CheckForItems);
        }

        protected override void OnCheckOut()
        {
            //get buy button
            Core.Logger.LogTrace("Finding Buy Button");
            var buyBtn = _waitFor.Until(x => x.FindElement(By.XPath(BuyButtonElement)));

            Core.Logger.LogTrace("Clicking Buy Button");
            buyBtn.Click();

            //Go to Cart
            Core.Logger.LogTrace("Entering Cart");
            WebDriver.Url = Cart;

            //Check out
            Core.Logger.LogInfo("Checking Out");
            var checkOutBtn = _waitFor.Until(x => x.FindElement(By.XPath(CheckOutElement)));

            checkOutBtn.Click();

            //Enter email
            Core.Logger.LogTrace("Getting Email Field");
            var emailField = _waitFor.Until(x => x.FindElement(By.XPath(UsernameFieldId)));

            Core.Logger.LogDebug("Typing Email");
            emailField.SendKeys(_email);

            //Enter password
            Core.Logger.LogTrace("Getting Password Field");
            var passwordField = _waitFor.Until(x => x.FindElement(By.Id(PasswordFieldId)));

            Core.Logger.LogDebug("Typing Password");
            passwordField.SendKeys(_password);

            //Sign in
            Core.Logger.LogTrace("Getting Sign In Button");
            var signInButton = _waitFor.Until(x => x.FindElement(By.XPath(SignInElement)));

            Core.Logger.LogDebug("Clicking Sign In Button");
            signInButton.Click();

            //Enter CVV
            Core.Logger.LogTrace("Getting CVV field");
            var cvvField = _waitFor.Until(x => x.FindElement(By.Id(CvvFieldId)));

            Core.Logger.LogInfo("Entering cvv number");
            cvvField.SendKeys(_cvv);

            //Purchase Item
            Core.Logger.LogTrace("Getting Place Order Button");
            var placeOrderBtn = _waitFor.Until(x => x.FindElement(By.CssSelector(PlaceOrderButtonCss)));

            Core.Logger.LogInfo("Placing Order");
            Purchase(placeOrderBtn);

            ChangeState(BotState.ShutDown);
        }

        private void Purchase(IWebElement purchaseElement)
        {
            if (_options.Contains("Test Mode"))
            {
                Core.Logger.LogWarning("Test Mode Activated, Order was not actually placed");
                return;
            }

            purchaseElement.Click();
        }

        protected override void OnShutDown()
        {
            //close chrome driver

            //return if one shot enabled
            if (_options.Contains("One Shot"))
            {
                //ask if user wants to restart
                if (!AnsiConsole.Confirm("[yellow]Would you like to restart the bot?[/]"))
                {
                    //if not wanting to restart, exit and close program
                    return;
                }
                WebDriver.Quit();
                StartWebDriver();
                return;
            }

            //wait 15 seconds before restarting - allows time for order to process
            Core.Logger.LogInfo("Waiting 15 seconds for purchase to confirm, restarting once timer is complete");
            Thread.Sleep(15000);
            WebDriver.Quit();
            Core.Logger.LogInfo("Restarting");
            //restart bot
            StartWebDriver();
        }

        protected override void OnCheckCart()
        {
            Core.Logger.LogInfo("Checking Cart");

            ChangeState(BotState.CheckForItems);
        }

        #endregion
    }
}