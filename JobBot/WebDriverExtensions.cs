using OpenQA.Selenium;

namespace JobBot;

public static class WebDriverExtensions
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
}
