using JobBot.Database;
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
    public async Task<QuestionDto[]> Questions(CancellationToken ct)
    {
        var autoLine = await _context.AutoLines
            .Select(x => new QuestionDto(x.Label.Replace(".", ""), x.AutoLineValue, x.QuestionPage, QuestionKind.AutoLine, null, x.InputType))
            .ToArrayAsync(ct);

        var combo = await _context.ComboBoxes
            .Select(x => new QuestionDto(x.Label.Replace(".", ""), x.SelectedOptionValue, x.QuestionPage, QuestionKind.ComboBox, x.ComboBoxOptions.Select(x => x.OptionValue).ToArray(), null))
            .ToArrayAsync(ct);

        var radio = await _context.Radios
            .Select(x => new QuestionDto(x.Label.Replace(".", ""), x.SelectedOptionValue, x.QuestionPage, QuestionKind.Radio, x.RadioOptions.Select(x => x.OptionValue).ToArray(), null))
            .ToArrayAsync(ct);

        var single = await _context.SingleLines
            .Select(x => new QuestionDto(x.Label.Replace(".", ""), x.SingleLineValue, x.QuestionPage, QuestionKind.SingleLine, null, x.InputType))
            .ToArrayAsync(ct);

        return autoLine.Concat(combo).Concat(radio).Concat(single).ToArray();
    }
}

public record QuestionDto(string Label, string? Value, QuestionPage QuestionPage, QuestionKind QuestionKind, string[]? Options, InputType? InputType);

//public class QuestionDto
//{
//    public required string Label { get; set; }
//    public required string? Value { get; set; }
//    public required QuestionPage QuestionPage { get; set; }
//    public required QuestionKind QuestionKind { get; set; }
//    public required string[]? Options { get; set; }
//}

public enum QuestionKind
{
    AutoLine,
    ComboBox,
    Radio,
    SingleLine,
}