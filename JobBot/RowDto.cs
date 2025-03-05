namespace JobBot;
public class RowDto
{
    public required long JobID;
    public required string JobTitle;
    public required string CompanyName;
    public required string Location;
    public required decimal? HourlyMin;
    public required decimal? HourlyMax;
    public required decimal? SalaryMin;
    public required decimal? SalaryMax;
    public required bool Has401k;
    public required bool Dental;
    public required bool Medical;
    public required bool Vision;
    public required int OtherBenefitsCount;
    public required bool Applied;
    public required bool Viewed;
    public required bool Promoted;
    public required bool EasyApply;
}
