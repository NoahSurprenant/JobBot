using OpenQA.Selenium;

namespace JobBot.PageObjectModels;
public class JobDetailPane : IDetail
{
    public Header Header { get; set; }
    public HeaderDto HeaderDto { get; set; }
    public JobDetails JobDetails { get; set; }

    public JobDetailPane(IWebDriver driver, long jobID)
    {
        Header = new Header(driver, HeaderType.DetailPane, jobID);
        HeaderDto = Header.ToDto();
        JobDetails = new JobDetails(driver);
    }
}

public class JobDetails
{
    public string Details = "";
    public JobDetails(IWebDriver driver)
    {
        //var elements = driver.FindElements(By.XPath("//*[@id=\"job-details\"]/div[1]/p[1]/span"));
        //Details = elements.Select(x => x.Text).ToArray();

        var element = driver.FindElement(By.XPath("//*[@id=\"job-details\"]/div[1]/p[1]"));
        //Details = element.Text;
        //Details = element.GetAttribute("textContent");
        Details = element.GetAttribute("innerHTML");
    }
}

public enum DurationKind
{
    Hours,
    Days,
    Weeks,
    Months,
    Minutes,
    Seconds,
}

public enum OfficeKind
{
    Remote,
    OnSite,
    Hybrid,
    Unknown,
}

public enum TimeKind
{
    Fulltime,
    Parttime,
    Unknown,
}