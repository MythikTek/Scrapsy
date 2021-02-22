using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using scrapsy.Enums;
using scrapsy.Services;
using scrapsy.Stores.Data;
using Spectre.Console;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace scrapsy.Stores
{
    public class BestBuy : BotStore
    {
        private const string Login = "https://www.bestbuy.com/identity/global/signin";
        private const string Cart = "https://www.bestbuy.com/cart";

        private const string LogInPageTitle = "Sign In to Best Buy";
        private const string HomePageTitle = "Best Buy | Official Online Store | Shop Now & Save";

        private const string UsernameFieldId = "//*[@id='fld-e']";
        private const string PasswordFieldId = "fld-p1";
        private const string SignInElement = "//*[@data-track='Sign In']";
        private const string CheckOutElement = "//*[@data-track='Checkout - Top']";
        private const string CvvFieldId = "credit-card-cvv";
        private const string PlaceOrderButtonCss = ".button__fast-track";
        private BestBuyAuthentication _authentication;

        private BestBuyConfig _config;
        private bool _configured;

        private string _currentSku = "";
        private string _cvv;

        private string _email;
        private string _password;


        private BestBuyProductData[] _productData;
        private WebDriverWait _waitFor;

        private HashSet<string> _options = new();

        public BestBuy(DirectoryService directoryService) : base(directoryService)
        {
            //_waitFor = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(_config.Timeout));
            //_productData = _config.ProductData;
        }

        private string ProductUrl => $"https://www.bestbuy.com/site//{_currentSku}.p?skuId={_currentSku}";

        private string BuyButtonElement => $"//*[@data-sku-id='{_currentSku}']";


        protected override void OnConfigure()
        {
            _options = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("[yellow]Select bot options[/]")
                    .PageSize(4)
                    .AddChoices("Headless Mode", "Test Mode", "One Shot")).ToHashSet();

            ConfigureFromFile();

            var optionsChosen = string.Join(" | ", _options);
            
            Core.Logger.LogTrace($"Options Chosen: {optionsChosen}");

            StartWebDriver();
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
            _productData = _config.ProductData.ToArray();

            //change state to start logging in
            ChangeState(BotState.LogIn);
        }

        protected override void OnStartUp()
        {
            SetUpAuthentication("NULL");
            _config = new BestBuyConfig();
            ChangeState(BotState.Configure);
        }

        private void ConfigureBot()
        {
            _config = new BestBuyConfig();
            _config.Delay = 3000;
            _config.Timeout = 10;

            ShowBotConfiguration();

            var configChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to configure?[/]")
                    .PageSize(4)
                    .AddChoices("Adjust Delay", "Adjust Timeout", "Adjust Products", "Configure From File", "Cancel"));

            switch (configChoice)
            {
                case "Adjust Delay":
                    ShowAdjustDelay();
                    break;
                case "Adjust Timeout":
                    ShowAdjustTimeout();
                    break;
                case "Adjust Products":
                    ShowAdjustProducts();
                    break;
                case "Cancel":
                    ChangeState(BotState.Configure);
                    break;
                case "Configure From File":
                    ConfigureFromFile();
                    break;
            }

            WebDriver = new ChromeDriver(DirectoryService.CurrentDirectory);
        }

        private void ConfigureFromFile()
        {
            Core.Logger.LogInfo("Loading configuration file");
            
            string json;
            var settingsFile = DirectoryService.ConfigDirectory + @"\BestBuyConfig.json";

            //check if file exists
            if (!File.Exists(settingsFile))
            {
                Core.Logger.LogWarning("No configuration file eixits please save a configuration first.");
                ChangeState(BotState.Configure);
                return;
            }
            
            json = File.ReadAllText(settingsFile);

            _config = JsonSerializer.Deserialize<BestBuyConfig>(json);
            _configured = true;
            //ChangeState(BotState.Configure);
        }

        private void ShowAdjustDelay()
        {
            _config.Delay = AnsiConsole.Ask<int>("[yellow]Please set delay value in milisecondsp - default: 3000[/]");
            Core.Logger.LogInfo($"Bot Delay now set to {_config.Delay} miliseconds.");
            ChangeState(BotState.Configure);
        }

        private void ShowAdjustTimeout()
        {
            _config.Timeout = AnsiConsole.Ask<int>("[yellow]Please set timeout value in seconds - default: 10[/]");
            Core.Logger.LogInfo($"Bot timeout now set to {_config.Timeout} seconds.");
            ChangeState(BotState.Configure);
        }

        private void ShowAdjustProducts()
        {
            ShowBotConfiguration();

            var actionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to do?[/]")
                    .PageSize(4)
                    .AddChoices("Edit Prodcut", "Add Product", "Remove Product", "Cancel"));

            switch (actionChoice)
            {
                case "Edit Product":
                    EditProdcut();
                    break;
                case "Add Product":
                    AddProduct();
                    break;
                case "Remove Product":
                    RemoveProduct();
                    break;
                case "Cancel":
                    ChangeState(BotState.Configure);
                    break;
            }
        }

        private void EditProdcut()
        {
            if (_config.ProductData.Count == 0)
            {
                Core.Logger.LogWarning("No products available to edit, please add a product first.");
                ShowAdjustProducts();
                return;
            }

            //get current products
            var products = _config.ProductData.ToDictionary(x => x.Name, x => x);

            //select product to edit
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Which Product would you like to edit?[/]")
                    .PageSize(products.Keys.ToArray().Length >= 3 ? products.Keys.ToArray().Length : 3)
                    .AddChoices(products.Keys.ToArray())
                    .AddChoice("Cancel"));

            if (choice == "Cancel")
            {
                ChangeState(BotState.Configure);
                return;
            }

            var product = products[choice];
            product.Name = AnsiConsole.Ask<string>("[yellow]Please update product name[/]");
            product.Sku = AnsiConsole.Ask<string>("[yellow]Please update product sku[/]");
            product.ModelNumber = AnsiConsole.Ask<string>("[yellow]Please update product model number[/]");
            product.MinimumPrice = AnsiConsole.Ask<int>("[yellow]Please update product minimum price[/]");
            product.MaximumPrice = AnsiConsole.Ask<int>("[yellow]Please update product maximum price[/]");

            ChangeState(BotState.Configure);
        }

        private void AddProduct()
        {
            var product = new BestBuyProductData
            {
                Name = AnsiConsole.Ask<string>("[yellow]Please enter product name[/]"),
                Sku = AnsiConsole.Ask<string>("[yellow]Please enter product sku[/]"),
                ModelNumber = AnsiConsole.Ask<string>("[yellow]Please enter product model number[/]"),
                MinimumPrice = AnsiConsole.Ask<int>("[yellow]Please enter product minimum price[/]"),
                MaximumPrice = AnsiConsole.Ask<int>("[yellow]Please enter product maximum price[/]")
            };

            _config.ProductData.Add(product);
            _configured = true;
            ChangeState(BotState.Configure);
        }

        private void RemoveProduct()
        {
            if (_config.ProductData.Count == 0)
            {
                Core.Logger.LogWarning("No products available to edit, please add a product first.");
                ShowAdjustProducts();
                return;
            }

            //get current products
            var products = _config.ProductData.ToDictionary(x => x.Name, x => x);

            //select product to remove
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Which Product would you like to remove?[/]")
                    .PageSize(products.Keys.ToArray().Length >= 3 ? products.Keys.ToArray().Length : 3)
                    .AddChoices(products.Keys.ToArray())
                    .AddChoice("Cancel"));

            if (choice == "Cancel")
            {
                ChangeState(BotState.Configure);
                return;
            }

            _config.ProductData.Remove(products[choice]);
            if (_config.ProductData.Count == 0)
                _configured = false;
            ChangeState(BotState.Configure);
        }

        private void ShowBotConfiguration()
        {
            var botConfig = new Table();
            botConfig.Title("Bot Settings");
            botConfig.AddColumn("Property");
            botConfig.AddColumn("Value");
            botConfig.AddRow("Delay", _config.Delay.ToString());
            botConfig.AddRow("Timeout", _config.Delay.ToString());

            AnsiConsole.Render(botConfig);
            var products = new Tree("Products");

            foreach (var item in _config.ProductData)
            {
                var product = new Table();
                product.Title(item.Name);
                product.AddColumn("Property");
                product.AddColumn("Value");

                product.AddRow("Sku", item.Sku);
                product.AddRow("Model Number", item.ModelNumber);
                product.AddRow("Maximum Price", item.MaximumPrice.ToString());
                product.AddRow("Minimum Price", item.MinimumPrice.ToString());

                products.AddNode(product);
            }

            AnsiConsole.Render(products);
        }

        private void SetUpAuthentication(string authPath)
        {
            _email = AnsiConsole.Ask<string>("[yellow]Please enter your Best Buy email[/]");
            _password = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Please enter your Best Buy password[/]")
                    .Secret());
            _cvv = AnsiConsole.Ask<string>(
                "[yellow]Please enter your credit card cvv number[/]");
            Core.Logger.LogInfo("Account info stored");
        }

        protected override void OnLogIn()
        {
            ChangeState(BotState.CheckCart);
        }

        protected override void OnCheckForItems()
        {
            /*TODO: ADD FUNCTIONALITY TO CHECK IF PRICE IS WITHIN RANGE IN CONFIG FILE*/

            foreach (var productData in _productData)
            {
                var wait = false;
                Task.Factory.StartNew(() =>
                {
                    Core.Logger.LogTrace("Check Item Delay Started...");
                    Thread.Sleep(3000);
                    wait = true;
                    Core.Logger.LogTrace("Check Item Delay Completed...");
                });

                //Check price for current produect
                _currentSku = productData.Sku;
                Core.Logger.LogInfo($"Checking Availability for product:{productData.Name} sku:{productData.Sku}");
                WebDriver.Url = ProductUrl; //set the url

                _waitFor.Until(x => wait); //wait for delay to complete

                //wait for page to load
                _waitFor.Until(x => x.Title.Contains(productData.ModelNumber));

                //get buy button element
                Core.Logger.LogTrace("Getting Buy Now Button");
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
    }
}