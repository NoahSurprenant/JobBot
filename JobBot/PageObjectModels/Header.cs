using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace JobBot.PageObjectModels;
// Traditionally JobID was only required for JobPage, but it is likely good to require it for detail page to ensure that we are on the right page still
public class Header(IWebDriver driver, HeaderType HeaderType, long JobID) : BasePage(driver)
{
    private By ByRoot => HeaderType switch
    {
        HeaderType.JobPage => By.XPath("//div[@class='job-view-layout jobs-details']/div[1]/div/div[1]/div/div/div"),
        HeaderType.DetailPane => By.XPath("//*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div"),
        _ => throw new ArgumentOutOfRangeException(),
    };

    private IWebElement Root() => Driver.FindElement(ByRoot);

    public void Click(IWebDriver driver)
    {
        _jobTitleElement.Click();
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        //w.Until(x => x.FindElementOrDefault(By.XPath("/html/body/div[6]/div[3]/div[2]/div/div/main/div[2]/div[1]/div/div[1]/div/div/div")) is not null);
        w.Until(x => x.FindElementOrDefault(By.XPath("//*[@id=\"main\"]/div[2]/div[1]/div/div[1]/div/div/div")) is not null);
    }

    public async Task ClickEasyApply(IWebDriver driver)
    {
        if (_applyElement is null)
            throw new Exception("Cannot apply to job there is not easy apply " + JobID);
        _applyElement.Click();
        await Task.Delay(1000);
        var btn = driver.FindElementOrDefault(By.XPath("//span[text()=\"Continue applying\"]/.."));
        if (btn is not null)
            btn.Click();
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        w.Until(x => x.FindElementOrDefault(By.XPath("//*[@id=\"jobs-apply-header\"]"))?.Text == "Apply to " + CompanyName);
    }

    private IWebElement? _companyLink => Root().FindElementOrDefaultAsWrapper(By.XPath("./div[1]/div[1]/div/a")); // Sometime there will be no link, maybe always use else clause?
    public string CompanyName => _companyLink?.Text ?? Root().FindElementAsWrapper(By.XPath("./div[1]/div[1]/div")).Text;
    public string? CompanyLink => _companyLink?.GetDomAttribute("href").Replace("https://www.linkedin.com/company/", "");
    private IWebElement _jobTitleElement => Root().FindElementOrDefaultAsWrapper(By.XPath("./div[2]/div/h1/a")) ?? Root().FindElement(By.XPath("./div[2]/div/h1"));
    // This should not be needed anymore if we always pass in ctor
    private long LegacyJobID => long.Parse(_jobTitleElement.GetDomAttribute("href").TrimStart("/jobs/view/".ToCharArray()).Split('/')[0]);
    public string JobTitle => _jobTitleElement.Text;

    private IWebElement[] _list => Root().FindElements(By.XPath("./div[3]/div/span")).Where(x => x.Text is not " · ").ToArray();
    public string Location => _list[0].Text;
    private string[] _split => _list[1].Text.Split(' ');
    public bool IsRepost => _split[0] is "Reposted";
    public int Amount => int.Parse(IsRepost ? _split[1] : _split[0]);
    private string _durationString => IsRepost ? _split[2] : _split[1];
    public DurationKind DurationKind => _durationString switch
    {
        "hours" or "hour" => DurationKind.Hours,
        "days" or "day" => DurationKind.Days,
        "weeks" or "week" => DurationKind.Weeks,
        "months" or "month" => DurationKind.Months,
        "minutes" or "minute" => DurationKind.Minutes,
        "seconds" or "second" => DurationKind.Seconds, // Haven't seen this yet but best to assume it can happen
        _ => throw new ArgumentOutOfRangeException(nameof(_durationString), _durationString, $"Expected days or weeks but got **{_durationString}**"),
    };
    public int Applicants()
    {
        var applicantPhrase = _list[2].Text.Split(' ');
        if (int.TryParse(applicantPhrase[1], out var a))
        {
            return a;
        }
        else
        {
            return int.Parse(applicantPhrase[0]);
        }
    }
    private string[] Pills => Root().FindElements(By.XPath("./button/div/span")).Select(x => x.Text).ToArray();
    public OfficeKind OfficeKind => Pills.Any(x => x is "Remote") ? OfficeKind.Remote
            : Pills.Any(x => x is "On-site") ? OfficeKind.OnSite
            : Pills.Any(x => x is "Hybrid") ? OfficeKind.Hybrid
            : OfficeKind.Unknown;

    public TimeKind TimeKind => Pills.Any(x => x is "Full-time") ? TimeKind.Fulltime
            : Pills.Any(x => x is "Part-time") ? TimeKind.Parttime
            : TimeKind.Unknown;

    public decimal? SalaryMin()
    {
        return Pills.FirstOrDefault(x => x.Contains("/yr"))?.GetSalaryRange()?.min;
    }
    public decimal? SalaryMax()
    {
        return Pills.FirstOrDefault(x => x.Contains("/yr"))?.GetSalaryRange()?.max;
    }
    public decimal? HourlyMin()
    {
        return Pills.FirstOrDefault(x => x.Contains("/hr"))?.GetHourlyRange()?.min;
    }
    public decimal? HourlyMax()
    {
        return Pills.FirstOrDefault(x => x.Contains("/hr"))?.GetHourlyRange()?.max;
    }

    private IWebElement? _applyElement => Root().FindElementOrDefault(By.XPath("./div[5]/div/div/div/button/span"));

    public HeaderDto ToDto()
    {
        return new HeaderDto()
        {
            CompanyName = CompanyName,
            CompanyLink = CompanyLink,
            JobID = JobID,
            JobTitle = JobTitle,
            Location = Location,
            IsRepost = IsRepost,
            Amount = Amount,
            DurationKind = DurationKind,
            Applicants = Applicants(),
            OfficeKind = OfficeKind,
            TimeKind = TimeKind,
            SalaryMin = SalaryMin(),
            SalaryMax = SalaryMax(),
            HourlyMin = HourlyMin(),
            HourlyMax = HourlyMax(),
        };
    }
}

public enum HeaderType
{
    JobPage,
    DetailPane,
}

