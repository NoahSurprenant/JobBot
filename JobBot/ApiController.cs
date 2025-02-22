using JobBot.Database;
using JobBot.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBot;

[ApiController]
[Route("[controller]/[action]")]
public class ApiController : ControllerBase
{
    private readonly Service _service;
    private readonly DataContext _context;

    public ApiController(Service service, DataContext context)
    {
        _service = service;
        _context = context;
    }

    /// <summary>
    /// Searches for and applies to Easy Apply jobs
    /// </summary>
    /// <param name="job">Job to search for</param>
    /// <param name="location">Location to search for</param>
    /// <returns>Task</returns>
    [HttpGet]
    public async Task Execute(string job, string location, CancellationToken ct)
    {
        await _service.ExecuteAsync(job, location, ct);
    }

    [HttpGet]
    public async Task<PaginationResult<QuestionDto>> Questions(CancellationToken ct)
    {
        var x = _context.AutoLines
                .Select(x => new { Label = x.Label, Value = x.AutoLineValue, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.AutoLine, Options = (string[]?)null, InputType = (InputType?)x.InputType })
            .Union(_context.ComboBoxes
                .Select(x => new { Label = x.Label, Value = x.SelectedOptionValue, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.ComboBox, Options = (string[]?)null, InputType = (InputType?)null }))
            .Union(_context.Radios
                .Select(x => new { Label = x.Label, Value = x.SelectedOptionValue, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.Radio, Options = (string[]?)null, InputType = (InputType?)null }))
            .Union(_context.SingleLines
                .Select(x => new { Label = x.Label, Value = x.SingleLineValue, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.SingleLine, Options = (string[]?)null, InputType = (InputType?)x.InputType }))
            .Select(x => new QuestionDto(x.Label, x.Value, x.QuestionPage, x.QuestionKind,
                x.QuestionKind == QuestionKind.ComboBox ? _context.ComboBoxOptions.Where(o => o.Label == x.Label).Select(o => o.OptionValue).ToArray() :
                x.QuestionKind == QuestionKind.Radio ? _context.RadioOptions.Where(o => o.Label == x.Label).Select(o => o.OptionValue).ToArray() : null,
                x.InputType));

        return await x.PaginationResult(new PaginationFilter(1, 250), ct);
    }
}

public record QuestionDto(string Label, string? Value, QuestionPage QuestionPage, QuestionKind QuestionKind, string[]? Options, InputType? InputType);

//public class QuestionDto
//{
//    public QuestionDto(string label, string? value, QuestionPage questionPage, QuestionKind questionKind, string[]? options, InputType? inputType)
//    {
//        Label = label;
//        Value = value;
//        QuestionPage = questionPage;
//        QuestionKind = questionKind;
//        Options = options;
//        InputType = inputType;
//    }

//    public string Label { get; set; }
//    public string? Value { get; set; }
//    public QuestionPage QuestionPage { get; set; }
//    public QuestionKind QuestionKind { get; set; }
//    public string[]? Options { get; set; }
//    public InputType? InputType { get; set; }
//}

public enum QuestionKind
{
    AutoLine,
    ComboBox,
    Radio,
    SingleLine,
}