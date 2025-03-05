using JobBot.PageObjectModels;

namespace JobBot;

public class HeaderDto
{
    public required string CompanyName { get; set; }
    public required string? CompanyLink { get; set; }
    public required long JobID { get; set; }
    public required string JobTitle { get; set; }
    public required string Location { get; set; }
    public required bool IsRepost { get; set; }
    public required int Amount { get; set; }
    public required DurationKind DurationKind { get; set; }
    public required int Applicants { get; set; }
    public required OfficeKind OfficeKind { get; set; }
    public required TimeKind TimeKind { get; set; }
    public required decimal? SalaryMin { get; set; }
    public required decimal? SalaryMax { get; set; }
    public required decimal? HourlyMin { get; set; }
    public required decimal? HourlyMax { get; set; }

}