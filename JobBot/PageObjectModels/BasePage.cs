using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace JobBot.PageObjectModels;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    private readonly static Random _random = new();
    //protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        //Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    protected async Task Wait(int min = 2, int max = 10)
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
