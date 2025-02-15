using Microsoft.AspNetCore.Mvc;

namespace JobBot;

[ApiController]
[Route("[controller]/[action]")]
public class ApiController : ControllerBase
{
    private readonly Service _service;

    public ApiController(Service service)
    {
        _service = service;
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
}
