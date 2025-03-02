using JobBot.Database;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace JobBot;

public class Service
{
    private readonly string[] Blacklisted = [
        "Universal Logistics Holdings, Inc.",
        "Central Transport",
        "HR-1",
        "UACL Logistics, LLC",
        ];
    private readonly IDbContextFactory<DataContext> _factory;
    private readonly ILogger<Service> _logger;
    private readonly Random _random = new Random();
    //private readonly string _proxy;
    private readonly string _cache;

    public Service(IDbContextFactory<DataContext> factory, ILogger<Service> logger, IConfiguration configuration)
    {
        _factory = factory;
        _logger = logger;
        //_proxy = configuration.GetValue<string>("Proxy") ?? throw new Exception("Missing proxy");
        _cache = configuration.GetValue<string>("Cache") ?? throw new Exception("Missing cache location");
    }

    public async Task ExecuteAsync(string job, string location, int maxApplyCount, int maxReadCount, CancellationToken stoppingToken)
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument($"user-data-dir={_cache}");
        //options.AddArgument("--remote-debugging-port=9292");
        //options.AddArgument("--headless");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalChromeOption("useAutomationExtension", false);

        //options.AddArgument($"--proxy-server={_proxy}");

        using var service = ChromeDriverService.CreateDefaultService();

        var p = service.DriverServicePath;

        //var driverPath = "C:\\Users\\grunt\\.cache\\selenium\\chromedriver\\win64\\132.0.6834.83";
        //var oldFile = Path.Combine(driverPath, "chromedriver.exe");
        //var newFile = Path.Combine(driverPath, "test.exe");

        //if (!Path.Exists(newFile))
        //{
        //    using var s = File.OpenRead(oldFile);
        //    var text = File.ReadAllText(oldFile, Encoding.Latin1).Replace("cdc_", "abc_");
        //    File.WriteAllText(newFile, text, Encoding.Latin1);
        //}

        //https://stackoverflow.com/questions/33225947/can-a-website-detect-when-you-are-using-selenium-with-chromedriver/41220267#41220267
        //service.DriverServicePath = driverPath;
        //service.DriverServiceExecutableName = "test.exe";

        using var driver = new ChromeDriver(service: service, options: options);

        driver.ExecuteCdpCommand("Page.removeScriptToEvaluateOnNewDocument", new([new("identifier", "1")]));

        //https://setcookie.net/
        //https://nowsecure.nl/
        //https://hmaker.github.io/selenium-detector/
        //https://whatismyipaddress.com/
        //https://linkedin.com/
        driver.Navigate().GoToUrl("https://linkedin.com/");
        await Wait();

        await NavigateToJobsPage(driver);

        try
        {
            //var job = ".net developer";
            //var location = "Detroit Metropolitan Area";
            var result = await Apply(driver, job, location, maxApplyCount, maxReadCount);
        }
        catch (Exception ex)
        {
            throw;
        }

        driver.Quit();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="driver">Selenium web driver</param>
    /// <param name="job">Job title to search for</param>
    /// <param name="location">location to search for</param>
    /// <param name="maxApplyCount">Max number of jobs to apply to</param>
    /// <param name="maxReadCount">Max number of rows to check before giving up, usually a higher number than applyCount</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<JobRowWithDetail>> Apply(ChromeDriver driver, string job, string location, int maxApplyCount, int maxReadCount)
    {
        await Search(driver, job, location);

        //*[@id="main"]/div/div[2]/div[1]/div/ul

        var xxx = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
            .Until(x => x.FindElementOrDefault(By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li")) is not null);
        var list = new List<JobRowWithDetail>();

        await Task.Delay(2000);

        var loop = true;
        var applyCount = 0;
        var readCount = 0;
        while (loop)
        {
            var appliesRemaining = maxApplyCount - applyCount;
            var readsRemaining = maxReadCount - readCount;
            var r = await CoreLoop(driver, appliesRemaining, readsRemaining);
            list.AddRange(r);
            applyCount += r.Length;
            readCount += 25;
            //count += 1;

            if (applyCount < maxApplyCount && readCount < maxReadCount)
            {
                //var page = driver.FindElements(By.XPath("/html/body/div[6]/div[3]/div[4]/div/div/main/div/div[2]/div[1]/div/div[3]/div[2]/ul/li/button"));
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
                }
                else // Ran out of pages
                {
                    loop = false;
                }
            }
            else
            {
                loop = false;
            }
        }
        
        return list;
    }

    private async Task<JobRowWithDetail[]> CoreLoop(ChromeDriver driver, int appliesRemaining, int readsRemaining)
    {
        using var context = _factory.CreateDbContext();
        var jobRows = driver.FindElements(By.XPath("//*[@id=\"main\"]/div/div[2]/div[1]/div/ul/li")).Take(readsRemaining);

        var width = (long)driver.ExecuteScript("return window.innerWidth;");
        var height = (long)driver.ExecuteScript("return window.innerHeight;");

        var list = new List<JobRowWithDetail>();
        var count = 0;
        foreach (var x in jobRows)
        {
            var JobID = long.Parse(x.GetDomAttribute("data-occludable-job-id"));
            var existing = context.JobPostings.FirstOrDefault(x => x.JobPostingID == JobID);
            if (existing is not null || existing is not null && existing.NoApplyReason is not null)
            {
                continue;
            }

            var location = x.Location;
            var size = x.Size;

            var inViewport = (location.X >= 0 &&
                                location.Y >= 0 &&
                                location.X + size.Width <= width &&
                                location.Y + size.Height <= height);

            if (inViewport is false)
            {
                new Actions(driver).ScrollToElement(x).Perform();
                //await Wait(1, 1);
            }
            await Wait(1, 1);

            var row = new JobRow(x);

            await LoadDetailPane(driver, x, row.JobID);

            

            var detailContent = driver.FindElement(By.XPath("//*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div"));
            var item = new JobRowWithDetail(row, new(driver, detailContent));

            using var transaction = context.Database.BeginTransaction();


            if (existing is not null)
            {
                existing.JobPostingDetail = context.JobPostingDetails.FirstOrDefault(x => x.JobPostingID == existing.JobPostingID);
            }

            //upsert
            var dbRow = existing?.Update(item) ?? JobPosting.Create(item);
            if (existing is null)
                context.Add(dbRow);

            // Missing in db
            if (row.Applied)
            {
                // We need to add to db only. No apply action needed.
            }
            else if (count < appliesRemaining) // Should we apply?
            {
                if (item.JobRow.JobTitle.Contains("wordpress"))
                {
                    dbRow.NoApplyReason = "Wordpress";
                }
                else if (item.JobRow.EasyApply is false)
                {
                    dbRow.NoApplyReason = "No easy apply";
                }
                else if (Blacklisted.Contains(item.JobRow.CompanyName))
                {
                    dbRow.NoApplyReason = "Blacklisted company";
                }
                else if (dbRow.NoApplyReason is not null)
                {

                }
                else
                {
                    await item.JobDetailPane.Header.ClickEasyApply(driver);
                    await Wait(1, 1);

                    if (existing is not null)
                    {
                        // Pull related data
                        dbRow.JobPostingQuestions = context
                            .JobPostingQuestions
                            .Include(x => x.Question.Options)
                            .Include(x => x.Question.Option)
                            .Where(x => x.JobPostingID == dbRow.JobPostingID)
                            .ToHashSet();
                    }

                    var result = await DoStepper(driver, context, JobID, dbRow);

                    if (result is false)
                    {
                        dbRow.NoApplyReason = "Missing answers";

                        var closeBtn = driver.FindElement(By.XPath("//button[@aria-label='Dismiss']"));
                        closeBtn.Click();
                        await Wait();

                        var discard = driver.FindElement(By.XPath("//button[@data-control-name='discard_application_confirm_btn']"));
                        discard.Click();
                        await Wait();
                    }
                    else
                    {
                        row.Applied = true;
                        dbRow.Applied = true;
                        count++;
                        list.Add(item);
                    }
                }
            }

            
            context.SaveChanges();
            transaction.Commit();
        }

        // Example selecting row and then going to job page
        //var first = typed.First();
        //if (first.JobRow.IsCurrentlySelected() is false)
        //{
        //    LoadDetailPane(driver, first.JobRow.Element, first.JobRow.JobID);
        //}

        //var detailContent = driver.FindElement(By.XPath("//*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div"));
        //var dto = new JobDetailPane(detailContent);
        //dto.Header.Click(driver);

        //var jobPage = new JobPage(driver);
        return list.ToArray();
    }

    private async Task<bool> DoStepper(ChromeDriver driver, DataContext context, long JobID, JobPosting dbRow)
    {
        while (true)
        {
            var header = driver.FindElementOrDefault(By.XPath("//form[1]/div[1]/div[1]/h3[1]"));
            if (header is null)
            {
                // Might want a better way to handle these non-form headers...
                header = driver.FindElementOrDefault(By.XPath("//h3/span[text()=\"Work experience\"]"));
                if (header is null)
                {
                    header = driver.FindElementOrDefault(By.XPath("//h3/span[text()=\"Education\"]"));
                    if (header is null)
                    {
                        // Rare but sometimes we get this odd 'Review your application' page that is not part of form.
                        header = driver.FindElement(By.XPath("//h3[text()=\"Review your application\"]"));
                    }
                }
                
            }
            var headerText = header.Text;

            UncheckFollow(driver);
            if (headerText is "Contact info" or "Additional Questions" or "Work authorization" or "Home address")
            {
                var qp = headerText switch
                {
                    "Contact info" => QuestionPage.ContactInfo,
                    "Additional Questions" => QuestionPage.AdditionalQuestions,
                    "Work authorization" => QuestionPage.WorkAuthorization,
                    "Home address" => QuestionPage.HomeAddress,
                    _ => throw new ArgumentOutOfRangeException(nameof(headerText), headerText,
                        $"{nameof(headerText)} was {headerText}. Must be Contact info, Additional Questions, or Work authorization"),
                };
                // Yet another question page found 'Work authorization' which also does not need the div skip.
                // If header can be anything maybe we need better way to determine if we are at a question step
                var questions = new Questions(driver, JobID, skipDiv: qp is QuestionPage.ContactInfo);
                await UpsertQuestions(context, dbRow, questions, qp);

                // Fill in any questions from db that we can.
                // Are there any that we can't? Then bail out

                var missing = questions.AnyUnanswered();

                if (missing)
                {
                    return false;
                }
                    

                var result = await ClickContinue(driver);
                if (result)
                    return true;
            }
            else if (headerText is "Resume" or "Education" or "Review" or "Review your application" or "Work experience")
            {
                var result = await ClickContinue(driver);
                if (result)
                    return true;
                else if (headerText is "Review" or "Review your application")
                    throw new Exception("Failed to submit app");
            }
            else
            {
                throw new Exception("Unrecognized step " + headerText);
            }
            await Wait(3);
        }
    }

    /// <summary>
    /// Returns true if submit. False if just continuing to next step
    /// </summary>
    private async Task<bool> ClickContinue(ChromeDriver driver)
    {
        var continueBtn = driver.FindElementOrDefault(By.XPath("//button[@aria-label='Continue to next step']"))
            ?? driver.FindElementOrDefault(By.XPath("//button[@aria-label='Review your application']"));
        if (continueBtn is not null)
        {
            continueBtn.Click();
            return false;
        }
        else
        {
            var sumbitBtn = driver.FindElementOrDefault(By.XPath("//button[@aria-label='Submit application']"));
            if (sumbitBtn is null)
                throw new Exception("Cannot continue to next step or submit");


            // For testing purposes we will just close the dialog instead and pretend we submit
            var closeBtn = driver.FindElement(By.XPath("//button[@aria-label='Dismiss']"));
            closeBtn.Click();
            await Wait();

            var discard = driver.FindElement(By.XPath("//button[@data-control-name='discard_application_confirm_btn']"));
            discard.Click();
            await Wait();

            return true;

            sumbitBtn.Click();
            return true;
        }
    }

    private static void UncheckFollow(ChromeDriver driver)
    {
        var follow = driver.FindElementOrDefault(By.XPath("//*[@id=\"follow-company-checkbox\"]"));
        if (follow is not null)
        {
            var followLabel = driver.FindElement(By.XPath("//label[@for='follow-company-checkbox']"));
            var before = follow.Selected;
            if (follow.Selected is true)
            {
                followLabel.Click();
            }
            var after = follow.Selected;
        }
    }

    private static async Task UpsertQuestions(DataContext context, JobPosting dbRow, Questions questions, QuestionPage questionPage)
    {
        var toRemove1 = dbRow.JobPostingQuestions.Where(x => x.QuestionPage == questionPage && x.Question.QuestionKind == QuestionKind.SingleLine).ExceptBy(questions.Singles.Select(x => x.Label), x => x.Label);
        var toRemove2 = dbRow.JobPostingQuestions.Where(x => x.QuestionPage == questionPage && x.Question.QuestionKind == QuestionKind.ComboBox).ExceptBy(questions.Combos.Select(x => x.Label), x => x.Label);
        var toRemove3 = dbRow.JobPostingQuestions.Where(x => x.QuestionPage == questionPage && x.Question.QuestionKind == QuestionKind.AutoLine).ExceptBy(questions.Autos.Select(x => x.Label), x => x.Label);
        var toRemove4 = dbRow.JobPostingQuestions.Where(x => x.QuestionPage == questionPage && x.Question.QuestionKind == QuestionKind.Radio).ExceptBy(questions.Radios.Select(x => x.Label), x => x.Label);
        var toRemove = toRemove1.Concat(toRemove2).Concat(toRemove3).Concat(toRemove4).ToList();
        foreach (var o in toRemove)
            dbRow.JobPostingQuestions.Remove(o);

        foreach (var question in questions.QuestionDtos)
        {
            var x = context.Questions
                .Include(x => x.Option!.Question)
                .Include(x => x.Options)
                .FirstOrDefault(x => x.Label == question.Label && x.QuestionKind == question.QuestionKind);
            if (x is null)
            {
                x = new Question()
                {
                    Label = question.Label,
                    //Value = question.Input,
                    InputType = question.InputType,
                    QuestionKind = question.QuestionKind,
                    Options = question.Options.Select(x => new Option()
                    {
                        Label = question.Label,
                        QuestionKind = question.QuestionKind,
                        Value = x
                    }).ToHashSet(),
                };
                context.Questions.Add(x);
                context.SaveChanges();
                // Possible circular reference requires these to be seperate writes
                if (question.Input is not null && (question.Input is not "Select an option" || question.QuestionKind is not QuestionKind.ComboBox))
                {
                    x.Value = question.Input;
                    context.SaveChanges();
                }
            }
            else // update selected value?
            {
                // No do not update selected value. Rather fill in from db
                if (question.Input != x.Value)
                {
                    await question.SetInput(x.Value);
                }
            }

            if (dbRow.JobPostingQuestions.Any(x => x.Label == question.Label && x.Question.QuestionKind == x.QuestionKind) is false)
            {
                dbRow.JobPostingQuestions.Add(new JobPostingQuestion()
                {
                    Question = x,
                    QuestionPage = questionPage,
                });
            }

            context.SaveChanges();
        }
    }

    /// <summary>
    /// Click row in left side pane and waits until right pane has loaded
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="x"></param>
    /// <param name="JobID"></param>
    private static async Task LoadDetailPane(IWebDriver driver, IWebElement x, long JobID)
    {
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        x.Click();

        var xxx = w.Until(x =>
        {
            //var jobTitleElement = x.FindElementOrDefault(By.XPath("/html/body/div[6]/div[3]/div[4]/div/div/main/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div/div[2]/div/h1/a"));
            var jobTitleElement = x.FindElementOrDefault(By.XPath("//*[@id=\"main\"]/div/div[2]/div[2]/div/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div/div[2]/div/h1/a"));
            if (jobTitleElement is null)
                return false;
            var jobID = long.Parse(jobTitleElement.GetDomAttribute("href").TrimStart("/jobs/view/".ToCharArray()).Split('/')[0]);
            return jobID == JobID;
        });
        await Task.Delay(2000);
    }

    //https://devhints.io/xpath
    private async Task NavigateToJobsPage(ChromeDriver driver)
    {
        var by = By.XPath("//*[@id=\"global-nav\"]/div/nav/ul/li[3]");
        var e = driver.FindElement(by);
        //var a = new Actions(driver);
        //a.MoveToElement(e, 100, 0).Click().Perform();
        e.Click();
        await Wait();
    }

    /// <summary>
    /// Submits search for a job at a location. Must already have /jobs loaded
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="job"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private async Task Search(ChromeDriver driver, string job, string location, bool easyApply = true)
    {
        //*[@id="jobs-search-box-keyword-id-ember29"]
        //*[@id="jobs-search-box-keyword-id-ember213"]
        var w = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        var title = w.Until(x => x.FindElementOrDefault(By.XPath("//*[starts-with(@id,'jobs-search-box-keyword-id-ember')]"))!);
        title.Click();
        await title.SendHumanKeys(job);

        //*[@id="jobs-search-box-location-id-ember29"]
        var loc = driver.FindElement(By.XPath("//*[starts-with(@id,'jobs-search-box-location-id-ember')]"));
        var valueEntered = loc.GetDomProperty("value");
        // Not sure why this if chain is not working on my actual account but does on my alt
        // On my main account it keeps reverting to United States for some reason
        //if (valueEntered != location)
        //{
            await Wait();
            loc.Click();
            loc.Clear();
            await loc.SendHumanKeys(location);
            //loc.Click();
            loc.SendKeys(Keys.Enter);
        //}
        //else
        //{
        //    // Location was already correct, so just hit enter on title
        //    // instead of being weird and hitting enter on location when we are not touching it
        //    title.SendKeys(Keys.Enter);
        //}
        await Wait();
        //var ea = w.Until(x => x.FindElementOrDefault(By.XPath("/html/body/div[7]/div[3]/div[4]/section/div/section/div/div/div/ul/li[8]/div/button")));
        //var ea = w.Until(x => x.FindElementOrDefault(By.XPath("/html/body/div[6]/div[3]/div[4]/section/div/section/div/div/div/ul/li[8]/div/button")));
        var ea = w.Until(x => x.FindElementOrDefault(By.XPath("/html/body/div/div[3]/div[4]/section/div/section/div/div/div/ul/li/div/button[text()=\"Easy Apply\"]")));
        var eaValue = ea!.GetDomAttribute("aria-checked");
        var eaBool = bool.Parse(eaValue);
        if (eaBool != easyApply)
            ea!.Click();
        await Wait();
    }

    private async Task Wait(int min = 2, int max = 10)
    {
        await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 1)));
        return;

        if (min < 1)
            throw new ArgumentOutOfRangeException("Min must be at least 1");
        if (max < min)
            throw new ArgumentOutOfRangeException("Max cannot be less than min");
        await Task.Delay(TimeSpan.FromSeconds(_random.Next(min, max)));
    }
}

public static class WebElementExt
{
    private readonly static Random _random = new();

    public static async Task SendHumanKeys(this IWebElement element, string text)
    {
        foreach (var t in text)
        {
            element.SendKeys(t.ToString());
            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(20, 150)));
        }
    }

    //public static ReadOnlyCollection<IWebElement> FindElementsAsWrapped(this IWebElement element, By by)
    //{
    //    var result = element.FindElements(by);
    //    return result.Select(x => new ElementWrapper())
    //    return new ElementWrapper(() => element.FindElements(by));
    //}

    public static IWebElement FindElementAsWrapper(this IWebElement element, By by)
    {
        return new ElementWrapper(() => element.FindElement(by));
    }

    public static IWebElement? FindElementOrDefaultAsWrapper(this IWebElement element, By by)
    {
        var wrapped = new NullableElementWrapper(() => element.FindElementOrDefault(by));
        if (wrapped.IsNull)
            return null;
        else
            return wrapped;
    }

    public static IWebElement? FindElementOrDefault(this IWebElement element, By by)
    {
        try
        {
            return element.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    public static IWebElement? FindElementOrDefault(this IWebDriver element, By by)
    {
        try
        {
            return element.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    public static Range? GetSalaryRange(this string input)
    {
        input = input.TrimStart("Starting at ".ToCharArray());
        var rangeMatch = Regex.Match(input, @"\$(\d+(\.\d+)?)[Kk]/yr\s*-\s*\$(\d+(\.\d+)?)[Kk]/yr");
        var singleRegex = Regex.Match(input, @"\$(\d+(\.\d+)?)[Kk]/yr");
        if (rangeMatch.Success)
        {
            return new Range(decimal.Parse(rangeMatch.Groups[1].Value) * 1000, decimal.Parse(rangeMatch.Groups[3].Value) * 1000);
        }
        else if (singleRegex.Success)
        {
            return new Range(decimal.Parse(singleRegex.Groups[1].Value) * 1000, decimal.Parse(singleRegex.Groups[1].Value) * 1000);
        }
        return null;
    }

    public static Range? GetHourlyRange(this string input)
    {
        input = input.TrimStart("Starting at ".ToCharArray());
        var rangeMatch = Regex.Match(input, @"\$(\d+(\.\d+)?)/hr\s*-\s*\$(\d+(\.\d+)?)/hr");
        var singleMatch = Regex.Match(input, @"\$(\d+(\.\d+)?)/hr");
        if (rangeMatch.Success)
        {
            return new Range(decimal.Parse(rangeMatch.Groups[1].Value), decimal.Parse(rangeMatch.Groups[3].Value));
        }
        else if (singleMatch.Success)
        {
            return new Range(decimal.Parse(singleMatch.Groups[1].Value), decimal.Parse(singleMatch.Groups[1].Value));
        }
        return null;
    }
}

public record Range(decimal min, decimal max);
