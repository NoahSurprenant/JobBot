using OpenQA.Selenium;

namespace JobBot;
public class JobDetailPane
{
    public Header Header { get; set; }


    //*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div
    public JobDetailPane(IWebElement wrapper)
    {
        Header = new Header(wrapper.FindElement(By.XPath("./div[1]/div/div[1]/div")));
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