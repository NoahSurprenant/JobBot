using OpenQA.Selenium;

namespace JobBot;
public class JobPage
{
    public long JobID;
    public Header Header { get; set; }
    public JobPage(IWebDriver driver)
    {
        JobID = long.Parse(driver.Url.TrimStart("https://www.linkedin.com/jobs/view/".ToCharArray()).Split('/')[0]);
        Header = new Header(driver.FindElement(By.XPath("/html/body/div[6]/div[3]/div[2]/div/div/main/div[2]/div[1]/div/div[1]/div/div/div")), JobID);
    }
}
