using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace JobBot;
internal class JobRow
{
    private IWebElement _element;
    public long JobID;
    private IWebElement _jobTitle;
    public string JobTitle;
    private IWebElement _companyName;
    public string CompanyName;
    private IWebElement _location;
    public string Location;
    private IWebElement? _payAndBenefits;
    public int? HourlyMin;
    public int? HourlyMax;
    public int? SalaryMin;
    public int? SalaryMax;
    public bool Has401k;
    public bool Dental;
    public bool Medical;
    public bool Vision;
    public int OtherBenefitsCount;
    private IWebElement _bottomRow;
    private IWebElement? _viewed;
    public bool Viewed;
    private IWebElement? _promoted;
    public bool Promoted;
    private IWebElement? _easyApply;
    public bool EasyApply;

    // XPath of root example:
    // /html/body/div[6]/div[3]/div[4]/div/div/main/div/div[2]/div[1]/div/ul/li[1]
    public JobRow(IWebElement element)
    {
        _element = element;
        JobID = long.Parse(_element.GetDomAttribute("data-occludable-job-id"));
        _jobTitle = _element.FindElement(By.XPath("./div/div/div[1]/div/div[2]/div[1]/a/span[1]/strong"));
        JobTitle = _jobTitle.Text;
        _companyName = _element.FindElement(By.XPath("./div/div/div[1]/div[1]/div[2]/div[2]/span"));
        CompanyName = _companyName.Text;
        _location = _element.FindElement(By.XPath("./div/div/div[1]/div/div[2]/div[3]/ul/li/span"));
        Location = _location.Text;
        _payAndBenefits = _element.FindElementOrDefault(By.XPath("./div/div/div[1]/div/div[2]/div[4]/ul/li/span"));
        if (_payAndBenefits is not null)
        {
            var things = _payAndBenefits.Text.Split('·');
            foreach (var thing in things)
            {
                if (thing.Contains("/yr"))
                {
                    var rangeMatch = Regex.Match(thing, @"\$(\d+)K/yr - \$(\d+)K/yr");
                    var singleMatch = Regex.Match(thing, @"\$(\d+)K/yr");

                    if (rangeMatch.Success)
                    {
                        SalaryMin = int.Parse(rangeMatch.Groups[1].Value) * 1000;
                        SalaryMax = int.Parse(rangeMatch.Groups[2].Value) * 1000;
                    }
                    else if (singleMatch.Success)
                    {
                        SalaryMin = int.Parse(singleMatch.Groups[1].Value) * 1000;
                        SalaryMax = SalaryMin;
                    }
                }
                else if (thing.Contains("/hr"))
                {
                    var rangeMatch = Regex.Match(thing, @"\$(\d+)/hr - \$(\d+)/hr");
                    var singleMatch = Regex.Match(thing, @"\$(\d+)/hr");

                    if (rangeMatch.Success)
                    {
                        HourlyMin = int.Parse(rangeMatch.Groups[1].Value);
                        HourlyMax = int.Parse(rangeMatch.Groups[2].Value);
                    }
                    else if (singleMatch.Success)
                    {
                        HourlyMin = int.Parse(singleMatch.Groups[1].Value);
                        HourlyMax = HourlyMin;
                    }
                }
                else
                {
                    if (thing.Contains("401(k)"))
                        Has401k = true;
                    if (thing.Contains("Dental"))
                        Dental = true;
                    if (thing.Contains("Medical"))
                        Medical = true;
                    if (thing.Contains("Vision"))
                        Vision = true;

                    var benefitsMatch = Regex.Match(thing, @"\+(\d+) benefits");
                    if (benefitsMatch.Success)
                    {
                        OtherBenefitsCount = int.Parse(benefitsMatch.Groups[1].Value);
                    }

                }
            }
            _bottomRow = _element.FindElement(By.XPath("./div/div/div[1]/ul"));
            _viewed = _bottomRow.FindElementOrDefault(By.XPath("./li[text()=\"Viewed\"]"));
            Viewed = _viewed is not null;
            _promoted = _bottomRow.FindElementOrDefault(By.XPath("./li[span[text()=\"Promoted\"]]"));
            Promoted = _promoted is not null;
            _easyApply = _bottomRow.FindElementOrDefault(By.XPath("./li/span[text()=\"Easy Apply\"]"));
            EasyApply = _easyApply is not null;
        }
    }
}
