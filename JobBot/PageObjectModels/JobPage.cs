using OpenQA.Selenium;

namespace JobBot.PageObjectModels;
public class JobPage : IDetail
{
    public long JobID;
    public Header Header { get; set; }
    public HeaderDto HeaderDto { get; set; }
    public JobDetails JobDetails { get; set; }
    public JobPage(IWebDriver driver)
    {
        JobID = long.Parse(driver.Url.TrimStart("https://www.linkedin.com/jobs/view/".ToCharArray()).Split('/')[0]);
        // Full path to header:             /html/body/div[5]/div[3]/div[2]/div/div/main/div[2]/div[1]/div/div[1]/div/div/div
        // To job-view-layout jobs-details  /html/body/div[5]/div[3]/div[2]/div/div/main/div[2]
        // By class is                      //div[@class='job-view-layout jobs-details']
        // So final xpath is                //div[@class='job-view-layout jobs-details']/div[1]/div/div[1]/div/div/div
        Header = new Header(driver, HeaderType.JobPage, JobID);
        HeaderDto = Header.ToDto();
        JobDetails = new JobDetails(driver);
    }
}
