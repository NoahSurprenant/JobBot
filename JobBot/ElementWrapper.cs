using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Drawing;

namespace JobBot;
public class ElementWrapper : IWebElement
{
    private IWebElement _element;
    private Func<IWebElement> _func;
    /// <summary>
    /// Use FindElementAsWrapper extension method. Do not create by hand
    /// </summary>
    public ElementWrapper(Func<IWebElement> func)
    {
        _func = func;
        _element = _func();
    }

    private T DoThing<T>(Func<IWebElement, T> func, int count = 0)
    {
        try
        {
            return func(_element);
        }
        catch (StaleElementReferenceException ex)
        {
            count++;
            if (count < 10)
            {
                _element = _func();
                return DoThing(func, count);
            }
            throw;
        }
    }

    private void DoThing(Action<IWebElement> func, int count = 0)
    {
        try
        {
            func(_element);
        }
        catch (StaleElementReferenceException ex)
        {
            count++;
            if (count < 10)
            {
                _element = _func();
                DoThing(func, count);
            }
            throw;
        }
    }

    public string TagName => DoThing(x => x.TagName);

    public string Text => DoThing(x => x.Text);

    public bool Enabled => DoThing(x => x.Enabled);

    public bool Selected => DoThing(x => x.Selected);

    public Point Location => DoThing(x => x.Location);

    public Size Size => DoThing(x => x.Size);

    public bool Displayed => DoThing(x => x.Displayed);

    public void Clear()
    {
        DoThing(x => x.Clear());
    }

    public void Click()
    {
        DoThing(x => x.Click());
    }

    public IWebElement FindElement(By by)
    {
        return DoThing(x => x.FindElement(by));
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        return DoThing(x => x.FindElements(by));
    }

    public string GetAttribute(string attributeName)
    {
        return DoThing(x => x.GetAttribute(attributeName));
    }

    public string GetCssValue(string propertyName)
    {
        return DoThing(x => x.GetCssValue(propertyName));
    }

    public string GetDomAttribute(string attributeName)
    {
        return DoThing(x => x.GetDomAttribute(attributeName));
    }

    public string GetDomProperty(string propertyName)
    {
        return DoThing(x => x.GetDomProperty(propertyName));
    }

    public ISearchContext GetShadowRoot()
    {
        return DoThing(x => x.GetShadowRoot());
    }

    public void SendKeys(string text)
    {
        DoThing(x => x.SendKeys(text));
    }

    public void Submit()
    {
        DoThing(x => x.Submit());
    }
}

public class NullableElementWrapper : IWebElement
{
    public bool IsNull => _element == null;
    private IWebElement? _element;
    private Func<IWebElement?> _func;
    /// <summary>
    /// Use FindElementOrDefaultAsWrapper extension method. Do not create by hand
    /// </summary>
    public NullableElementWrapper(Func<IWebElement?> func)
    {
        _func = func;
        _element = _func();
    }

    private T DoThing<T>(Func<IWebElement?, T> func, int count = 0)
    {
        try
        {
            return func(_element);
        }
        catch (StaleElementReferenceException ex)
        {
            count++;
            if (count < 10)
            {
                _element = _func();
                return DoThing(func, count);
            }
            throw;
        }
    }

    private void DoThing(Action<IWebElement?> func, int count = 0)
    {
        try
        {
            func(_element);
        }
        catch (StaleElementReferenceException ex)
        {
            count++;
            if (count < 10)
            {
                _element = _func();
                DoThing(func, count);
            }
            throw;
        }
    }
#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public string TagName => DoThing(x => x.TagName);

    public string Text => DoThing(x => x.Text);

    public bool Enabled => DoThing(x => x.Enabled);

    public bool Selected => DoThing(x => x.Selected);

    public Point Location => DoThing(x => x.Location);

    public Size Size => DoThing(x => x.Size);

    public bool Displayed => DoThing(x => x.Displayed);

    public void Clear()
    {
        DoThing(x => x.Clear());
    }

    public void Click()
    {
        DoThing(x => x.Click());
    }

    public IWebElement FindElement(By by)
    {
        return DoThing(x => x.FindElement(by));
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {

        return DoThing(x => x.FindElements(by));
    }

    public string GetAttribute(string attributeName)
    {
        return DoThing(x => x.GetAttribute(attributeName));
    }

    public string GetCssValue(string propertyName)
    {
        return DoThing(x => x.GetCssValue(propertyName));
    }

    public string GetDomAttribute(string attributeName)
    {
        return DoThing(x => x.GetDomAttribute(attributeName));
    }

    public string GetDomProperty(string propertyName)
    {
        return DoThing(x => x.GetDomProperty(propertyName));
    }

    public ISearchContext GetShadowRoot()
    {
        return DoThing(x => x.GetShadowRoot());
    }

    public void SendKeys(string text)
    {
        DoThing(x => x.SendKeys(text));
    }

    public void Submit()
    {
        DoThing(x => x.Submit());
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}

