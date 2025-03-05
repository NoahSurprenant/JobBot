using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace JobBot.PageObjectModels;

public class Row(IWebDriver driver, int index) : BasePage(driver)
{
    // XPath of root example:
    // /html/body/div[6]/div[3]/div[4]/div/div/main/div/div[2]/div[1]/div/ul/li[1]
    private readonly By ByRoot = By.XPath($"//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li[{index}]");
    private IWebElement Root() => Driver.FindElement(ByRoot);

    public bool InViewport(long width, long height)
    {
        var root = Root();
        var location = root.Location;
        var size = root.Size;
        return (location.X >= 0 &&
                                location.Y >= 0 &&
                                location.X + size.Width <= width &&
                                location.Y + size.Height <= height);
    }

    public void ScrollTo()
    {
        new Actions(Driver).ScrollToElement(Root()).Perform();
        //await Wait(1, 1);
    }

    public bool IsCurrentlySelected()
    {
        var div = Root().FindElement(By.XPath("./div/div"));
        var atr = div.GetDomAttribute("aria-current");
        return atr is not null;
    }

    public async Task LoadDetailPane()
    {
        var w = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
        var JOBID = JobID(); // Load JobID once now, rather than in loop (not efficient) or after click (may throw DOM changed error)
        Root().Click();

        var xxx = w.Until(x =>
        {
            //var jobTitleElement = x.FindElementOrDefault(By.XPath("/html/body/div[6]/div[3]/div[4]/div/div/main/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div/div[2]/div/h1/a"));
            var jobTitleElement = x.FindElementOrDefault(By.XPath("//*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div/div[2]/div/h1/a"));
            if (jobTitleElement is null)
                return false;
            var jobID = long.Parse(jobTitleElement.GetDomAttribute("href").TrimStart("/jobs/view/".ToCharArray()).Split('/')[0]);
            return jobID == JOBID;
        });
        await Task.Delay(2000);
    }

    public long JobID() => long.Parse(Root().GetDomAttribute("data-occludable-job-id"));
    public string JobTitle() => Root().FindElement(By.XPath("./div/div/div[1]/div/div[2]/div[1]/a/span[1]/strong")).Text;
    public string CompanyName() => Root().FindElement(By.XPath("./div/div/div[1]/div[1]/div[2]/div[2]/span")).Text;
    public string Location() => Root().FindElement(By.XPath("./div/div/div[1]/div/div[2]/div[3]/ul/li/span")).Text;


    private IWebElement? PayAndBenefits() => Root().FindElementOrDefault(By.XPath("./div/div/div[1]/div/div[2]/div[4]/ul/li/span"));
    private string[]? PayAndBenefitsSplit() => PayAndBenefits()?.Text?.Split('.');
    public decimal? HourlyMin() => PayAndBenefitsSplit()?.FirstOrDefault(x => x.Contains("/yr"))?.GetHourlyRange()?.min;
    public decimal? HourlyMax() => PayAndBenefitsSplit()?.FirstOrDefault(x => x.Contains("/yr"))?.GetHourlyRange()?.max;
    public decimal? SalaryMin() => PayAndBenefitsSplit()?.FirstOrDefault(x => x.Contains("/yr"))?.GetSalaryRange()?.min;
    public decimal? SalaryMax() => PayAndBenefitsSplit()?.FirstOrDefault(x => x.Contains("/yr"))?.GetSalaryRange()?.max;
    public bool Has401k() => PayAndBenefitsSplit()?.Any(x => x.Contains("401(k)")) ?? false;
    public bool Dental() => PayAndBenefitsSplit()?.Any(x => x.Contains("Dental")) ?? false;
    public bool Medical() => PayAndBenefitsSplit()?.Any(x => x.Contains("Medical")) ?? false;
    public bool Vision() => PayAndBenefitsSplit()?.Any(x => x.Contains("Vision")) ?? false;
    public int OtherBenefitsCount()
    {
        var first = PayAndBenefitsSplit()?.FirstOrDefault(x => x.Contains("benefits"));
        if (first is null)
            return 0;
        var benefitsMatch = Regex.Match(first, @"\+(\d+) benefits");
        if (benefitsMatch.Success)
            return int.Parse(benefitsMatch.Groups[1].Value);
        return 0;
    }


    private IWebElement BottomRow() => Root().FindElement(By.XPath("./div/div/div[1]/ul"));
    public bool Applied() => BottomRow().FindElementOrDefault(By.XPath("./li[text()=\"Applied\"]")) is not null;
    public bool Viewed() => BottomRow().FindElementOrDefault(By.XPath("./li[text()=\"Viewed\"]")) is not null;
    public bool Promoted() => BottomRow().FindElementOrDefault(By.XPath("./li[span[text()=\"Promoted\"]]")) is not null;
    public bool EasyApply() => BottomRow().FindElementOrDefault(By.XPath("./li/span[text()=\"Easy Apply\"]")) is not null;

    public RowDto ToDto()
    {
        return new RowDto()
        {
            JobID = JobID(),
            JobTitle = JobTitle(),
            CompanyName = CompanyName(),
            Location = Location(),
            HourlyMin = HourlyMin(),
            HourlyMax = HourlyMax(),
            SalaryMin = SalaryMin(),
            SalaryMax = SalaryMax(),
            Has401k = Has401k(),
            Dental = Dental(),
            Medical = Medical(),
            Vision = Vision(),
            OtherBenefitsCount = OtherBenefitsCount(),
            Applied = Applied(),
            Viewed = Viewed(),
            Promoted = Promoted(),
            EasyApply = EasyApply(),
        };
    }
}
