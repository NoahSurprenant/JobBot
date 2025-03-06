using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace JobBot.PageObjectModels;

//https://devhints.io/xpath

/// <summary>
/// This class handles clicking the job tab button, inputing a search, and clicking button to go to next page of job listing
/// </summary>
public class SearchPage(IWebDriver driver) : BasePage(driver: driver)
{
    //*[@id="main"]/div/div[2]/div[1]/div/ul
    private readonly By ByRows = By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li");

    // Sometimes
    //*[@id="jobs-search-box-keyword-id-ember29"]
    // Sometimes
    //*[@id="jobs-search-box-keyword-id-ember213"]
    // So use start with
    private readonly By ByTitle = By.XPath("//*[starts-with(@id,'jobs-search-box-keyword-id-ember')]");

    private readonly By ByLocationInput = By.XPath("//*[starts-with(@id,'jobs-search-box-location-id-ember')]");

    private readonly By ByEasyApplyCheckbox = By.XPath("/html/body/div/div[3]/div[4]/section/div/section/div/div/div/ul/li/div/button[text()=\"Easy Apply\"]");

    private readonly By ByJobTabButton = By.XPath("//*[@id=\"global-nav\"]/div/nav/ul/li[3]");

    public async Task ClickJobTabButton()
    {
        var e = driver.FindElement(ByJobTabButton);
        e.Click();
        await Wait();
    }

    /// <summary>
    /// Submits search for a job at a location. Must already have /jobs loaded
    /// </summary>
    public async Task Search(string job, string location, bool easyApply = true)
    {
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        var title = w.Until(x => x.FindElementOrDefault(ByTitle)!);
        title.Click();
        await title.SendHumanKeys(job);

        var locationInput = driver.FindElement(ByLocationInput);
        var valueEntered = locationInput.GetDomProperty("value");
        // Not sure why this if chain is not working on my actual account but does on my alt
        // On my main account it keeps reverting to United States for some reason
        //if (valueEntered != location)
        //{
        await Wait();
        locationInput.Click();
        locationInput.Clear();
        await locationInput.SendHumanKeys(location);
        locationInput.SendKeys(Keys.Enter);
        //}
        //else
        //{
        //    // Location was already correct, so just hit enter on title
        //    // instead of being weird and hitting enter on location when we are not touching it
        //    title.SendKeys(Keys.Enter);
        //}
        await Wait();
        var ea = w.Until(x => x.FindElementOrDefault(ByEasyApplyCheckbox));
        var eaValue = ea!.GetDomAttribute("aria-checked");
        var eaBool = bool.Parse(eaValue);
        if (eaBool != easyApply)
            ea!.Click();
        await Wait();

        var xxx = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
            .Until(x => x.FindElementOrDefault(ByRows) is not null);
        await Task.Delay(2000);
    }

    /// <summary>
    /// Clicks button to go to next page of job listings
    /// </summary>
    /// <returns>True if success, false is no more pages</returns>
    public async Task<bool> ClickNextPage()
    {
        var page = driver.FindElements(By.XPath("//*[@id=\"jobs-search-results-footer\"]/div[2]/ul/li/button"));
        var currentPage = page.Where(x => x.GetDomAttribute("aria-current") is not null).Select(x => int.Parse(x.GetDomAttribute("aria-label").Replace("Page ", ""))).FirstOrDefault();
        var next = currentPage + 1;
        var all = page.Select(x => int.Parse(x.GetDomAttribute("aria-label").Replace("Page ", ""))).ToArray();
        if (all.Any(x => x == next))
        {
            // Click next page
            var btn = page.FirstOrDefault(x => x.GetDomAttribute("aria-label") == "Page " + next)
                ?? throw new Exception("Failed to find button for Page " + next);
            btn.Click();
            // Wait until load
            await Wait(4, 6);
            return true;
        }
        else // Ran out of pages
        {
            return false;
        }
    }

    public IEnumerable<int> GetJobIndexes()
    {
        return driver.FindElements(By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li")).Select((_, i) => i + 1);
    }
}
