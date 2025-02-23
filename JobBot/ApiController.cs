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
    public async Task<PaginationResult<JobDto>> Jobs([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var x = _context.JobPostings
                .Select(x => new JobDto(x.JobPostingID, x.CompanyName, x.CompanyLink, x.JobTitle, x.Location, x.OfficeKind, x.SalaryMin, x.SalaryMax, x.NoApplyReason));

        return await x.PaginationResult(filter, ct);
    }

    [HttpPost]
    public async Task<PaginationResult<QuestionDto>> Questions([FromQuery] PaginationFilter filter, [FromBody] QuestionFilter questionFilter, CancellationToken ct)
    {
        var q1 = _context.AutoLines.AsQueryable();
        var q2 = _context.ComboBoxes.AsQueryable();
        var q3 = _context.Radios.AsQueryable();
        var q4 = _context.SingleLines.AsQueryable();

        if (questionFilter.Value is not null)
        {
            q1 = q1.Where(x => x.Value == questionFilter.Value.Value);
            q2 = q2.Where(x => x.Value == questionFilter.Value.Value);
            q3 = q3.Where(x => x.Value == questionFilter.Value.Value);
            q4 = q4.Where(x => x.Value == questionFilter.Value.Value);
        }

        var x = q1
                .Select(x => new { Label = x.Label, Value = x.Value, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.AutoLine, Options = (string[]?)null, InputType = (InputType?)x.InputType })
            .Union(q2
                .Select(x => new { Label = x.Label, Value = x.Value, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.ComboBox, Options = (string[]?)null, InputType = (InputType?)null }))
            .Union(q3
                .Select(x => new { Label = x.Label, Value = x.Value, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.Radio, Options = (string[]?)null, InputType = (InputType?)null }))
            .Union(q4
                .Select(x => new { Label = x.Label, Value = x.Value, QuestionPage = x.QuestionPage, QuestionKind = QuestionKind.SingleLine, Options = (string[]?)null, InputType = (InputType?)x.InputType }))
            .Select(x => new QuestionDto(x.Label, x.Value, x.QuestionPage, x.QuestionKind,
                x.QuestionKind == QuestionKind.ComboBox ? _context.ComboBoxOptions.Where(o => o.Label == x.Label).Select(o => o.Value).ToArray() :
                x.QuestionKind == QuestionKind.Radio ? _context.RadioOptions.Where(o => o.Label == x.Label).Select(o => o.Value).ToArray() : null,
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
        db.Value = question.Value;
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
        if (db.RadioOptions.Any(x => x.Value == question.Value) is false)
            throw new Exception($"{question.Value} is not a valid value for radio {db.Label}");
        db.Value = question.Value;
        await _context.SaveChangesAsync(ct);
    }

    private async Task SaveCombo(QuestionDto question, CancellationToken ct)
    {
        var db = await _context.ComboBoxes
            .Include(x => x.ComboBoxOptions)
            .FirstOrDefaultAsync(x => x.Label == question.Label, ct)
            ?? throw new Exception($"Could not find combo box: {question.Label}");
        if (db.ComboBoxOptions.Any(x => x.Value == question.Value) is false)
            throw new Exception($"{question.Value} is not a valid value for combo box {db.Label}");
        db.Value = question.Value;
        await _context.SaveChangesAsync(ct);
    }
}

public record JobDto(long jobID, string CompanyName, string? CompanyLink, string JobTitle, string Location, OfficeKind OfficeKind, decimal? SalaryMin, decimal? SalaryMax, string? NoApplyReason);

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

public class QuestionFilter
{
    public PropertyFilter? Value { get; set; }
}
public class PropertyFilter
{
    public string? Value { get; set; }
}