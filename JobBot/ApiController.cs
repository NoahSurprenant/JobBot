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
    public async Task<PaginationResult<QuestionDto>> Questions([FromQuery] PaginationFilter filter, CancellationToken ct)
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

        return await x.PaginationResult(filter, ct);
    }

    [HttpPost]
    public async Task SaveQuestions([FromBody] QuestionDto[] questions, CancellationToken ct)
    {
        foreach(var question in questions)
        {
            var task = question.QuestionKind switch
            {
                QuestionKind.SingleLine => SaveSingleLine(question, ct),
                QuestionKind.AutoLine => SaveAutoLine(question, ct),
                QuestionKind.Radio => SaveRadio(question, ct),
                QuestionKind.ComboBox => SaveCombo(question, ct),
                _ => throw new ArgumentOutOfRangeException()
            };
            await task;
        }
    }

    private async Task SaveSingleLine(QuestionDto question, CancellationToken ct)
    {
        var db = await _context.SingleLines.FirstOrDefaultAsync(x => x.Label == question.Label, ct)
            ?? throw new Exception($"Could not find single line: {question.Label}");
        db.SingleLineValue = question.Value;
        await _context.SaveChangesAsync(ct);
    }

    private async Task SaveAutoLine(QuestionDto question, CancellationToken ct)
    {
        throw new NotImplementedException("Saving auto line is currently not supported");
    }

    private async Task SaveRadio(QuestionDto question, CancellationToken ct)
    {
        var db = await _context.Radios
            .Include(x => x.RadioOptions)
            .FirstOrDefaultAsync(x => x.Label == question.Label, ct)
            ?? throw new Exception($"Could not find radio: {question.Label}");
        if (db.RadioOptions.Any(x => x.OptionValue == question.Value) is false)
            throw new Exception($"{question.Value} is not a valid value for radio {db.Label}");
        db.SelectedOptionValue = question.Value;
        await _context.SaveChangesAsync(ct);
    }

    private async Task SaveCombo(QuestionDto question, CancellationToken ct)
    {
        var db = await _context.ComboBoxes
            .Include(x => x.ComboBoxOptions)
            .FirstOrDefaultAsync(x => x.Label == question.Label, ct)
            ?? throw new Exception($"Could not find combo box: {question.Label}");
        if (db.ComboBoxOptions.Any(x => x.OptionValue == question.Value) is false)
            throw new Exception($"{question.Value} is not a valid value for combo box {db.Label}");
        db.SelectedOptionValue = question.Value;
        await _context.SaveChangesAsync(ct);
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