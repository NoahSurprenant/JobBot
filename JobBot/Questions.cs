using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace JobBot;
public class Questions
{
    public List<SingleDto> Singles = new();
    public List<ComboDto> Combos = new();
    public List<AutoDto> Autos = new();
    public List<RadioDto> Radios = new();

    public Questions(IWebDriver driver, long JobID, bool skipDiv)
    {
        var form = driver.FindElement(By.XPath("//form[1]"));

        // First is contact div, unless additional questions. Rest are div with class like qKaYReXtKxWInYNdGaWKNtCOycgtsjDHelqpS
        var divs = form.FindElements(By.XPath("./div[1]/div[1]/div")).Skip(skipDiv ? 1 : 0).ToList();
        

        foreach (var y in divs)
        {
            // inner div has class fb-dash-form-element and jbVwHDGaAjptFLyyeygpxWihBejjeLeVv and each has differetn mt1 etc
            // style="width:100%" tabindex="-1" data-test-form-element=""
            // second inner div has data-test-text-entity-list-form-component="" OR data-test-single-line-text-form-component="" data-live-test-single-line-text-form-component=""
            var inner = y.FindElement(By.XPath("./div/div"));
            var fieldset = y.FindElementOrDefault(By.XPath("./div/fieldset"));

            var combo = fieldset is null && inner.GetDomAttribute("data-test-text-entity-list-form-component") is not null;
            var single = fieldset is null && inner.GetDomAttribute("data-test-single-line-text-form-component") is not null && inner.GetDomAttribute("data-live-test-single-line-text-form-component") is not null;
            var auto = fieldset is null && inner.GetDomAttribute("data-test-single-typeahead-entity-form-component") is not null;
            var radio = fieldset is not null;

            if (combo)
            {
                Combos.Add(new(inner, JobID));
            }
            else if (single)
            {
                Singles.Add(new(inner, JobID));
            }
            else if (auto)
            {
                Autos.Add(new(inner, JobID));
            }
            else if (radio)
            {
                Radios.Add(new(fieldset, JobID));
            }
            else
            {
                throw new NotImplementedException("Shit!");
            }
        }
    }

    public List<SingleDto> UnansweredSingles()
    {
        return Singles.Where(x => x.Input is null).ToList();
    }
    public List<ComboDto> UnansweredCombos()
    {
        return Combos.Where(x => x.Input is null).ToList();
    }
    public List<AutoDto> UnansweredAutos()
    {
        return Autos.Where(x => x.Input is null).ToList();
    }
    public List<RadioDto> UnansweredRadios()
    {
        return Radios.Where(x => x.Input is null).ToList();
    }
    public bool AnyUnanswered()
    {
        return UnansweredSingles().Count != 0 || UnansweredCombos().Count != 0 || UnansweredAutos().Count != 0 || UnansweredRadios().Count != 0;
    }
}

public class AutoDto
{
    //private const string _const = "single-typeahead-entity-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-";
    //private readonly long jobID;
    public string ForAttribute { get; private set; }
    //public string Key { get; private set; }
    public string Label { get; private set; }
    public string? Input { get; private set; }

    public AutoDto(IWebElement inner, long jobID)
    {
        var label = inner.FindElement(By.XPath("./label"));
        Label = label.FindElement(By.XPath("./span[2]")).Text;
        var input = inner.FindElement(By.XPath("./div[1]/input"));
        Input = input.GetAttribute("value");
        if (Input == string.Empty)
            Input = null;
        ForAttribute = label.GetDomAttribute("for") ?? throw new Exception("Missing for attribute");
        //Key = ForAttribute.Replace(_const + jobID + "-", "");
    }
}

public class SingleDto
{
    //private const string _const = "single-line-text-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-";
    //private readonly long jobID;
    public string ForAttribute { get; private set; }
    //public string Key { get; private set; }
    public string Label { get; private set; }
    public string? Input { get; private set; }

    public SingleDto(IWebElement inner, long jobID)
    {
        //this.jobID = jobID;
        // First div appears to be what we want
        // Second appears to be errors
        var firstDiv = inner.FindElement(By.XPath("./div[1]/div"));
        var label = firstDiv.FindElement(By.XPath("./label")); // Mobile phone number
        Label = label.Text;
        var input = firstDiv.FindElement(By.XPath("./input"));
        Input = input.GetAttribute("value");
        if (Input == string.Empty)
            Input = null;
        ForAttribute = label.GetDomAttribute("for") ?? throw new Exception("Missing for attribute");
        //Key = ForAttribute.Replace(_const + jobID + "-", "");
    }
}

public class ComboDto
{
    //private const string _const = "text-entity-list-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-";
    //private readonly long jobID;
    public string ForAttribute { get; private set; }
    //public string Key { get; private set; }
    public string Label { get; private set; }
    public string? Input { get; private set; }
    public string[] Options { get; private set; }

    public ComboDto(IWebElement inner, long jobID)
    {
        //this.jobID = jobID;
        var label = inner.FindElement(By.XPath("./label"));
        Label = label.FindElement(By.XPath("./span[2]")).Text;
        var select = inner.FindElement(By.XPath("./select"));
        var selectObj = new SelectElement(select);
        if (selectObj.IsMultiple)
            throw new NotImplementedException("Currently not supporting multiple selections!");
        Input = selectObj.SelectedOption.Text;
        if (Input == string.Empty || Input == "Select an option")
            Input = null;
        var options = select.FindElements(By.XPath("./option"));
        Options = options.Skip(1).Select(x => x.GetDomAttribute("value")).ToArray();
        if (Options.Any() is false)
            throw new Exception("Failed to get options");
        ForAttribute = label.GetDomAttribute("for") ?? throw new Exception("Missing for attribute");
        //Key = ForAttribute.Replace(_const + jobID + "-", "");
    }
}


public class RadioDto
{
    //private readonly long jobID;
    public string IDAttribute { get; private set; }
    //public string Key { get; private set; }
    public string Label { get; private set; }
    public string? Input { get; private set; }
    public string[] Options { get; private set; }

    public RadioDto(IWebElement fieldset, long jobID)
    {
        //this.jobID = jobID;
        var span = fieldset.FindElement(By.XPath("./legend[1]/span[1]/span[1]"));
        Label = span.Text;

        //Input = selectObj.SelectedOption.Text;
        //if (Input == string.Empty)
        //    Input = null;

        var options = fieldset.FindElements(By.XPath("./div/input"));
        Options = options.Select(x => x.GetDomAttribute("value")).ToArray();
        if (Options.Any() is false)
            throw new Exception("Failed to get options");
        IDAttribute = fieldset.GetDomAttribute("id") ?? throw new Exception("Missing id attribute");
        //Key = ForAttribute.Replace(_const + jobID + "-", "");

        var selected = options.FirstOrDefault(x => x.Selected);
        if (selected is not null)
            Input = selected.GetDomAttribute("value") ?? throw new Exception("Failed to get value of selected radio");
        if (Input == string.Empty)
            Input = null;
    }
}
