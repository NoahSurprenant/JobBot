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
    /// <param name="maxApplyCount">The maximum number of jobs to apply to</param>
    /// <param name="maxReadCount">The maximum number of jobs to read</param>
    /// <returns>Task</returns>
    [HttpGet]
    public async Task Execute(string job, string location, int maxApplyCount, int maxReadCount, CancellationToken ct)
    {
        await _service.ExecuteAsync(job, location, maxApplyCount, maxReadCount, ct);
    }

    [HttpGet]
    public async Task Apply(long jobID, CancellationToken ct)
    {
        await _service.Apply(jobID, ct);
    }

    [HttpGet]
    public async Task<string?> JobDetails(long jobID, CancellationToken ct)
    {
        return await _context.JobPostingDetails
            .Where(x => x.JobPostingID == jobID)
            .Select(x => x.Details)
            .FirstOrDefaultAsync(ct);
    }

    [HttpPost]
    public async Task<PaginationResult<JobDto>> Jobs([FromQuery] PaginationFilter filter, [FromBody] JobFilter jobFilter, CancellationToken ct)
    {
        var x = _context.JobPostings.AsQueryable();

        if (jobFilter.QuestionKind is not null && jobFilter.Label is not null)
        {
            x = x.Where(x => x.JobPostingQuestions.Any(x => x.Label == jobFilter.Label && x.QuestionKind == jobFilter.QuestionKind));
        }

        if (jobFilter.Applied is not null)
        {
            x = x.Where(x => x.Applied == jobFilter.Applied);
        }

        var final = x.OrderByDescending(x => x.LastDatePulled).Select(x => new JobDto(x.JobPostingID, x.CompanyName, x.CompanyLink, x.JobTitle, x.Location, x.OfficeKind, x.SalaryMin, x.SalaryMax, x.NoApplyReason));

        return await final.PaginationResult(filter, ct);
    }

    [HttpPost]
    public async Task<PaginationResult<QuestionDto>> Questions([FromQuery] PaginationFilter filter, [FromBody] QuestionFilter questionFilter, CancellationToken ct)
    {
        var q = _context.Questions.AsQueryable();

        if (questionFilter.Value is not null)
        {
            q = q.Where(x => x.Value == questionFilter.Value.Value);
        }

        if (questionFilter.JobID is not null)
        {
            q = q.Where(x => x.JobPostingQuestions.Any(x => x.JobPostingID == questionFilter.JobID));
        }

        var x = q.Select(x => new { x.Label, x.Value, x.QuestionKind, Options = (string[]?)null, InputType = (InputType?)x.InputType, x.JobPostingQuestions.Count })
                .Select(x => new QuestionDto(x.Label, x.Value, x.QuestionKind,
                x.QuestionKind == QuestionKind.ComboBox || x.QuestionKind == QuestionKind.Radio ? _context.Options.Where(o => o.Label == x.Label && o.QuestionKind == x.QuestionKind).Select(o => o.Value).ToArray() : null,
                x.InputType, x.Count));

        return await x.PaginationResult(filter, ct);
    }

    [HttpPost]
    public async Task SaveQuestions([FromBody] QuestionDto[] questions, CancellationToken ct)
    {
        foreach(var question in questions)
        {
            if (question.QuestionKind is QuestionKind.AutoLine)
                throw new NotImplementedException("Saving auto line is currently not supported");

            var db = await _context.Questions
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Label == question.Label && x.QuestionKind == question.QuestionKind, ct)
                ?? throw new Exception($"Could not find question: {question.Label} | {question.QuestionKind}");

            if (question.Value is not null && db.QuestionKind is QuestionKind.ComboBox or QuestionKind.Radio)
            {
                if (db.Options.Any(x => x.Value == question.Value) is false)
                    throw new Exception($"{question.Value} is not a valid value for {db.Label} | {question.QuestionKind}");
            }
            
            db.Value = question.Value;
            await _context.SaveChangesAsync(ct);
        }
    }
}

public record JobDto(long jobID, string CompanyName, string? CompanyLink, string JobTitle, string Location, OfficeKind OfficeKind, decimal? SalaryMin, decimal? SalaryMax, string? NoApplyReason);

public record QuestionDto(string Label, string? Value, QuestionKind QuestionKind, string[]? Options, InputType? InputType, int AttachedJobs);

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

public class QuestionFilter
{
    public PropertyFilter? Value { get; set; }
    public long? JobID { get; set; }
}
public class PropertyFilter
{
    public string? Value { get; set; }
}

public class JobFilter
{
    public string? Label { get; set; }
    public QuestionKind? QuestionKind { get; set; }
    public bool? Applied { get; set; }
}