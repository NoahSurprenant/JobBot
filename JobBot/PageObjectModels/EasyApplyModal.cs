using OpenQA.Selenium;

namespace JobBot.PageObjectModels;

public class EasyApplyModal(IWebDriver driver) : BasePage(driver)
{
    private readonly By ByDimiss = By.XPath("//button[@aria-label='Dismiss']");
    private readonly By ByDimissConfirm = By.XPath("//button[@data-control-name='discard_application_confirm_btn']");

    /// <summary>
    /// Dimiss and close out of the easy apply modal
    /// </summary>
    public async Task Dismiss()
    {
        var closeBtn = driver.FindElement(ByDimiss);
        closeBtn.Click();
        await Wait(); // This should be smarter

        var discard = driver.FindElement(ByDimissConfirm);
        discard.Click();
        await Wait(); // This should be smarter
    }

    /// <summary>
    /// Gets the title header of the current step
    /// </summary>
    public string GetHeader()
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
        return header.Text;
    }

    /// <summary>
    /// Returns true if submit. False if just continuing to next step
    /// </summary>
    public async Task<bool> ClickContinue(bool demoMode)
    {
        var continueBtn = Driver.FindElementOrDefault(By.XPath("//button[@aria-label='Continue to next step']"))
            ?? driver.FindElementOrDefault(By.XPath("//button[@aria-label='Review your application']"));
        if (continueBtn is not null)
        {
            continueBtn.Click();
            return false;
        }
        else
        {
            var sumbitBtn = Driver.FindElementOrDefault(By.XPath("//button[@aria-label='Submit application']"));
            if (sumbitBtn is null)
                throw new Exception("Cannot continue to next step or submit");


            if (demoMode)
            {
                // For testing purposes we will just close the dialog instead and pretend we submit
                await Dismiss();

                return true;
            }
            else
            {
                sumbitBtn.Click();
                return true;
            }
        }
    }

    public void UncheckFollow()
    {
        var follow = Driver.FindElementOrDefault(By.XPath("//*[@id=\"follow-company-checkbox\"]"));
        if (follow is not null)
        {
            var followLabel = Driver.FindElement(By.XPath("//label[@for='follow-company-checkbox']"));
            var before = follow.Selected;
            if (follow.Selected is true)
            {
                followLabel.Click();
            }
            var after = follow.Selected;
        }
    }
}
