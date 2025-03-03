using OpenQA.Selenium;

namespace JobBot;
public class JobDetailPane : IDetail
{
    public Header Header { get; set; }
    public JobDetails JobDetails { get; set; }

    //*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div
    public JobDetailPane(IWebDriver driver, IWebElement wrapper)
    {
        Header = new Header(wrapper.FindElement(By.XPath("./div[1]/div/div[1]/div")));
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