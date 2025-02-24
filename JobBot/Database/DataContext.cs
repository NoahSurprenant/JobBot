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
    public DbSet<JobPostingDetail> JobPostingDetails { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<JobPostingQuestion> JobPostingQuestions { get; set; }
    public DbSet<Option> Options { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
        modelBuilder.ApplyConfiguration(new JobPostingDetailConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionConfiguration());
        modelBuilder.ApplyConfiguration(new JobPostingQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new OptionConfiguration());
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

        entity.HasOne(x => x.JobPostingDetail)
            .WithOne(x => x.JobPosting)
            .HasForeignKey<JobPostingDetail>(x => x.JobPostingID)
            .IsRequired(false);
    }
}

public class JobPostingDetailConfiguration : IEntityTypeConfiguration<JobPostingDetail>
{
    public void Configure(EntityTypeBuilder<JobPostingDetail> entity)
    {
        entity.ToTable("JobPostingDetails");

        entity.HasKey(e => e.JobPostingID);

        // Value always comes from LinkedIn
        entity.Property(e => e.JobPostingID).ValueGeneratedNever();
    }
}

public class JobPostingQuestionConfiguration : IEntityTypeConfiguration<JobPostingQuestion>
{
    public void Configure(EntityTypeBuilder<JobPostingQuestion> entity)
    {
        entity.ToTable("JobPostingQuestions");

        entity.HasKey(e => new { e.JobPostingID, e.Label, e.QuestionKind });

        entity.HasOne(x => x.Question)
            .WithMany(x => x.JobPostingQuestions)
            .HasForeignKey(x => new { x.Label, x.QuestionKind });

        entity.HasOne(x => x.JobPosting)
            .WithMany(x => x.JobPostingQuestions)
            .HasForeignKey(x => x.JobPostingID);
    }
}

public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> entity)
    {
        entity.ToTable("Options");

        entity.HasKey(e => new { e.Value, e.Label, e.QuestionKind });

        entity.HasOne(x => x.Question)
            .WithMany(x => x.Options)
            .HasForeignKey(x => new { x.Label, x.QuestionKind });

        entity.HasOne(x => x.SelectedQuestion)
            .WithOne(x => x.Option)
            .HasForeignKey<Question>(x => new { x.Value, x.Label, x.QuestionKind })
            .IsRequired(false);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> entity)
    {
        entity.ToTable("Questions");

        entity.HasKey(e => new { e.Label, e.QuestionKind });

        // Value always comes from LinkedIn
        entity.Property(e => e.Label).ValueGeneratedNever();
    }
}


// Link table
public class JobPostingQuestion
{
    public long JobPostingID { get; set; }
    public string Label { get; set; } = null!;
    public QuestionKind QuestionKind { get; set; }
    public QuestionPage QuestionPage { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

public class Question
{
    public Question()
    {
        JobPostingQuestions = new HashSet<JobPostingQuestion>();
        Options = new HashSet<Option>();
    }
    /// <summary>
    /// Currently only set for Single Line and Auto Line to text or number. Otherwise not used and defaults to text.
    /// </summary>
    public InputType InputType { get; set; }
    public QuestionKind QuestionKind { get; set; }
    public string Label { get; set; } = null!;
    public string? Value { get; set; }
    /// <summary>
    /// May not be null on Radio or ComboBox
    /// </summary>
    public Option? Option { get; set; }
    public HashSet<JobPostingQuestion> JobPostingQuestions { get; set; }
    /// <summary>
    /// Should have values for Radio or ComboBox
    /// </summary>
    public HashSet<Option> Options { get; set; }
}

public class Option
{
    public string Label { get; set; } = null!;
    public QuestionKind QuestionKind { get; set; }
    public string Value { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Question? SelectedQuestion { get; set; }
}

public enum InputType
{
    text,
    tel,
    url,
    number,
    email,
    password,
}

public enum QuestionKind
{
    AutoLine,
    ComboBox,
    Radio,
    SingleLine,
}

public class JobPostingDetail
{
    public long JobPostingID { get; set; }
    public string Details { get; set; } = null!;
    public JobPosting JobPosting { get; set; } = null!;
}


public class JobPosting
{
    public JobPosting()
    {
        JobPostingQuestions = new HashSet<JobPostingQuestion>();
    }
    public HashSet<JobPostingQuestion> JobPostingQuestions { get; set; }
    public JobPostingDetail? JobPostingDetail { get; set; }
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
    public bool Applied { get; set; }
    public string? NoApplyReason { get; set; }
    public DateTime DatePulled { get; set; }
    public DateTime LastDatePulled { get; set; }

    public static JobPosting Create(JobRowWithDetail x)
    {
        var y = new JobPosting();
        y.JobPostingDetail = new JobPostingDetail()
        {
            Details = x.JobDetailPane.JobDetails.Details,
        };
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
        y.Applied = x.JobRow.Applied;
        y.DatePulled = DateTime.UtcNow;
        y.LastDatePulled = y.DatePulled;
        return y;
    }

    public JobPosting Update(JobRowWithDetail x)
    {
        if (JobPostingDetail is null)
            JobPostingDetail = new JobPostingDetail();
        JobPostingDetail.Details = x.JobDetailPane.JobDetails.Details;
        JobPostingID = x.JobRow.JobID;
        CompanyName = x.JobRow.CompanyName;
        CompanyLink = x.JobDetailPane.Header.CompanyLink;
        JobTitle = x.JobRow.JobTitle;
        Location = x.JobRow.Location;
        IsRepost = x.JobDetailPane.Header.IsRepost;
        Amount = x.JobDetailPane.Header.Amount;
        DurationKind = x.JobDetailPane.Header.DurationKind;
        Applicants = x.JobDetailPane.Header.Applicants;
        OfficeKind = x.JobDetailPane.Header.OfficeKind;
        TimeKind = x.JobDetailPane.Header.TimeKind;
        HourlyMin = x.JobDetailPane.Header.HourlyMin;
        HourlyMax = x.JobDetailPane.Header.HourlyMax;
        SalaryMin = x.JobDetailPane.Header.SalaryMin;
        SalaryMax = x.JobDetailPane.Header.SalaryMax;
        Has401k = x.JobRow.Has401k;
        Dental = x.JobRow.Dental;
        Medical = x.JobRow.Medical;
        Vision = x.JobRow.Vision;
        EasyApply = x.JobRow.EasyApply;
        Applied = x.JobRow.Applied;
        LastDatePulled = DateTime.UtcNow;
        return this;
    }
}

public enum QuestionPage
{
    ContactInfo,
    AdditionalQuestions,
    WorkAuthorization,
}