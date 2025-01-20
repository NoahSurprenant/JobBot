using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Random _random = new Random();
    private readonly string _proxy;
    private readonly string _cache;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _proxy = configuration.GetValue<string>("Proxy") ?? throw new Exception("Missing proxy");
        _cache = configuration.GetValue<string>("Cache") ?? throw new Exception("Missing cache location");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument($"user-data-dir={_cache}");
        //options.AddArgument("--remote-debugging-port=9292");
        //options.AddArgument("--headless");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalChromeOption("useAutomationExtension", false);

        options.AddArgument($"--proxy-server={_proxy}");

        using var service = ChromeDriverService.CreateDefaultService();

        var p = service.DriverServicePath;

        //var driverPath = "C:\\Users\\grunt\\.cache\\selenium\\chromedriver\\win64\\132.0.6834.83";
        //var oldFile = Path.Combine(driverPath, "chromedriver.exe");
        //var newFile = Path.Combine(driverPath, "test.exe");

        //if (!Path.Exists(newFile))
        //{
        //    using var s = File.OpenRead(oldFile);
        //    var text = File.ReadAllText(oldFile, Encoding.Latin1).Replace("cdc_", "abc_");
        //    File.WriteAllText(newFile, text, Encoding.Latin1);
        //}

        //https://stackoverflow.com/questions/33225947/can-a-website-detect-when-you-are-using-selenium-with-chromedriver/41220267#41220267
        //service.DriverServicePath = driverPath;
        //service.DriverServiceExecutableName = "test.exe";

        using var driver = new ChromeDriver(service: service, options: options);

        driver.ExecuteCdpCommand("Page.removeScriptToEvaluateOnNewDocument", new([new("identifier", "1")]));

        //https://setcookie.net/
        //https://nowsecure.nl/
        //https://hmaker.github.io/selenium-detector/
        //https://whatismyipaddress.com/
        //https://linkedin.com/
        driver.Navigate().GoToUrl("https://linkedin.com/");
        await Wait();

        await NavigateToJobsPage(driver);

        var job = "developer";
        var location = "Detroit Metropolitan Area";
        await Search(driver, job, location);

        //*[@id="main"]/div/div[2]/div[1]/div/ul

        

        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        var xxx = w.Until(x => x.FindElement(By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li")));
        await Task.Delay(2000);

        

        var jobRows = driver.FindElements(By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li"));

        var width = (long)driver.ExecuteScript("return window.innerWidth;");
        var height = (long)driver.ExecuteScript("return window.innerHeight;");

        var typed = await jobRows.ToAsyncEnumerable().SelectAwait(async x =>
        {
            var location = x.Location;
            var size = x.Size;

            var inViewport = (location.X >= 0 &&
                                location.Y >= 0 &&
                                location.X + size.Width <= width &&
                                location.Y + size.Height <= height);

            if (inViewport is false)
            {
                new Actions(driver).ScrollToElement(x).Perform();
                await Wait(1, 1);
            }
            return new JobRow(x);
        }).ToArrayAsync();

        driver.Quit();
    }
    //https://devhints.io/xpath
    private async Task NavigateToJobsPage(ChromeDriver driver)
    {
        var by = By.XPath("//*[@id=\"global-nav\"]/div/nav/ul/li[3]");
        var e = driver.FindElement(by);
        //var a = new Actions(driver);
        //a.MoveToElement(e, 100, 0).Click().Perform();
        e.Click();
        await Wait();
    }

    /// <summary>
    /// Submits search for a job at a location. Must already have /jobs loaded
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="job"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private async Task Search(ChromeDriver driver, string job, string location)
    {
        //*[@id="jobs-search-box-keyword-id-ember29"]
        //*[@id="jobs-search-box-keyword-id-ember213"]
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        var title = w.Until(x => x.FindElement(By.XPath("//*[starts-with(@id,'jobs-search-box-keyword-id-ember')]")));
        title.Click();
        await title.SendHumanKeys(job);

        //*[@id="jobs-search-box-location-id-ember29"]
        var loc = driver.FindElement(By.XPath("//*[starts-with(@id,'jobs-search-box-location-id-ember')]"));
        var valueEntered = loc.GetDomProperty("value");
        if (valueEntered != location)
        {
            await Wait();
            loc.Click();
            loc.Clear();
            await loc.SendHumanKeys(location);
            loc.Click();
            loc.SendKeys(Keys.Enter);
        }
        else
        {
            // Location was already correct, so just hit enter on title
            // instead of being weird and hitting enter on location when we are not touching it
            title.SendKeys(Keys.Enter);
        }
        await Wait();
    }

    private async Task Wait(int min = 2, int max = 10)
    {
        await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 1)));
        return;

        if (min < 1)
            throw new ArgumentOutOfRangeException("Min must be at least 1");
        if (max < min)
            throw new ArgumentOutOfRangeException("Max cannot be less than min");
        await Task.Delay(TimeSpan.FromSeconds(_random.Next(min, max)));
    }
}

public static class WebElementExt
{
    private readonly static Random _random = new();

    public static async Task SendHumanKeys(this IWebElement element, string text)
    {
        foreach (var t in text)
        {
            element.SendKeys(t.ToString());
            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(20, 150)));
        }
    }

    public static IWebElement? FindElementOrDefault(this IWebElement element, By by)
    {
        try
        {
            return element.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
