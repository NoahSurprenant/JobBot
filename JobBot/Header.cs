using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace JobBot;
public class Header
{
    public void Click(IWebDriver driver)
    {
        _jobTitleElement.Click();
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        //w.Until(x => x.FindElementOrDefault(By.XPath("/html/body/div[6]/div[3]/div[2]/div/div/main/div[2]/div[1]/div/div[1]/div/div/div")) is not null);
        w.Until(x => x.FindElementOrDefault(By.XPath("//*[@id=\"main\"]/div[2]/div[1]/div/div[1]/div/div/div")) is not null);
    }

    public void ClickEasyApply(IWebDriver driver)
    {
        _applyElement.Click();
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        w.Until(x => x.FindElementOrDefault(By.XPath("//*[@id=\"jobs-apply-header\"]"))?.Text == "Apply to " + CompanyName);
    }

    public long JobID;
    public string CompanyName;
    public string? CompanyLink;
    public string JobTitle;
    public string Location;
    public bool IsRepost;
    public int Amount;
    public DurationKind DurationKind;
    public int Applicants;
    public OfficeKind OfficeKind;
    public TimeKind TimeKind;
    public decimal? HourlyMin;
    public decimal? HourlyMax;
    public decimal? SalaryMin;
    public decimal? SalaryMax;

    private readonly IWebElement _header;
    private readonly IWebElement _jobTitleElement;
    private readonly IWebElement _applyElement;

    // Full:
    //*[@id="main"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/./div[1]/div/div[1]/div

    // ./div[1]/div/div[1]/div
    public Header(IWebElement header, long? jobID = null)
    {
        _header = header;
        var companyLink = _header.FindElementOrDefault(By.XPath("./div[1]/div[1]/div/a")); // Sometime there will be no link
        CompanyName = companyLink?.Text ?? _header.FindElement(By.XPath("./div[1]/div[1]/div")).Text;
        CompanyLink = companyLink?.GetDomAttribute("href").Replace("https://www.linkedin.com/company/", "");
        _jobTitleElement = _header.FindElementOrDefault(By.XPath("./div[2]/div/h1/a")) ?? _header.FindElement(By.XPath("./div[2]/div/h1"));
        JobID = jobID ?? long.Parse(_jobTitleElement.GetDomAttribute("href").TrimStart("/jobs/view/".ToCharArray()).Split('/')[0]);
        JobTitle = _jobTitleElement.Text;

        var list = _header.FindElements(By.XPath("./div[3]/div/span")).Where(x => x.Text is not " · ").ToArray();
        Location = list[0].Text;

        var split = list[1].Text.Split(' ');
        IsRepost = split[0] is "Reposted";

        Amount = int.Parse(IsRepost ? split[1] : split[0]);
        var durationStr = IsRepost ? split[2] : split[1];
        DurationKind = durationStr switch
        {
            "hours" or "hour" => DurationKind.Hours,
            "days" or "day" => DurationKind.Days,
            "weeks" or "week" => DurationKind.Weeks,
            "months" or "month" => DurationKind.Months,
            "minutes" or "minute" => DurationKind.Minutes,
            "seconds" or "second" => DurationKind.Seconds, // Haven't seen this yet but best to assume it can happen
            _ => throw new ArgumentOutOfRangeException(nameof(durationStr), durationStr, $"Expected days or weeks but got **{durationStr}**"),
        };

        var applicantPhrase = list[2].Text.Split(' ');
        if (int.TryParse(applicantPhrase[1], out var a))
        {
            Applicants = a;
        }
        else
        {
            Applicants = int.Parse(applicantPhrase[0]);
        }

        var pills = _header.FindElements(By.XPath("./button/div/span")).Select(x => x.Text).ToArray();

        OfficeKind = pills.Any(x => x is "Remote") ? OfficeKind.Remote
            : pills.Any(x => x is "On-site") ? OfficeKind.OnSite
            : pills.Any(x => x is "Hybrid") ? OfficeKind.Hybrid
            : OfficeKind.Unknown;

        TimeKind = pills.Any(x => x is "Full-time") ? TimeKind.Fulltime
            : pills.Any(x => x is "Part-time") ? TimeKind.Parttime
            : TimeKind.Unknown;

        var yearlyStr = pills.FirstOrDefault(x => x.Contains("/yr"));
        if (yearlyStr is not null)
        {
            var range = yearlyStr.GetSalaryRange();
            SalaryMin = range?.min;
            SalaryMax = range?.max;
        }
        else
        {
            var hourlyStr = pills.FirstOrDefault(x => x.Contains("/hr"));

            if (hourlyStr is not null)
            {
                var range = hourlyStr.GetHourlyRange();
                HourlyMin = range?.min;
                HourlyMax = range?.max;
            }
        }

        try
        {
            _applyElement = _header.FindElement(By.XPath("./div[5]/div/div/div/button/span"));
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}
