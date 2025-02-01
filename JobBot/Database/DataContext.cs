using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBot.Database;
public class DataContext : DbContext
{
    public DataContext() : base()
    {
    }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<JobPosting> JobPostings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
    }
}

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> entity)
    {
        entity.ToTable("JobPostings");

        entity.HasKey(e => e.JobPostingID);

        // Value always comes from LinkedIn
        entity.Property(e => e.JobPostingID).ValueGeneratedNever();
    }
}

public class JobPosting
{
    public long JobPostingID { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? CompanyLink { get; set; }
    public string JobTitle { get; set; } = null!;
    public string Location { get; set; } = null!;
    public bool IsRepost { get; set; }
    public int Amount { get; set; }
    public DurationKind DurationKind;
    public int Applicants;
    public OfficeKind OfficeKind;
    public TimeKind TimeKind;
    public decimal? HourlyMin;
    public decimal? HourlyMax;
    public decimal? SalaryMin;
    public decimal? SalaryMax;
    public bool Has401k { get; set; }
    public bool Dental { get; set; }
    public bool Medical { get; set; }
    public bool Vision { get; set; }
    public bool EasyApply { get; set; }
    public DateTime DatePulled { get; set; }

    public static JobPosting Create(JobRowWithDetail x)
    {
        var y = new JobPosting();
        y.JobPostingID = x.JobRow.JobID;
        y.CompanyName = x.JobRow.CompanyName;
        y.CompanyLink = x.JobDetailPane.Header.CompanyLink;
        y.JobTitle = x.JobRow.JobTitle;
        y.Location = x.JobRow.Location;
        y.IsRepost = x.JobDetailPane.Header.IsRepost;
        y.Amount = x.JobDetailPane.Header.Amount;
        y.DurationKind = x.JobDetailPane.Header.DurationKind;
        y.Applicants = x.JobDetailPane.Header.Applicants;
        y.OfficeKind = x.JobDetailPane.Header.OfficeKind;
        y.TimeKind = x.JobDetailPane.Header.TimeKind;
        y.HourlyMin = x.JobDetailPane.Header.HourlyMin;
        y.HourlyMax = x.JobDetailPane.Header.HourlyMax;
        y.SalaryMin = x.JobDetailPane.Header.SalaryMin;
        y.SalaryMax = x.JobDetailPane.Header.SalaryMax;
        y.Has401k = x.JobRow.Has401k;
        y.Dental = x.JobRow.Dental;
        y.Medical = x.JobRow.Medical;
        y.Vision = x.JobRow.Vision;
        y.EasyApply = x.JobRow.EasyApply;
        y.DatePulled = DateTime.UtcNow;
        return y;
    }
}
