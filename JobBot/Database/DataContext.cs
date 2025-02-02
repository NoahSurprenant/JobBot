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
    public DbSet<ComboBox> ComboBoxes { get; set; }
    public DbSet<SingleLine> SingleLines { get; set; }
    public DbSet<JobPostingComboBox> JobPostingComboBoxes { get; set; }
    public DbSet<JobPostingSingleLine> JobPostingSingleLines { get; set; }
    public DbSet<ComboBoxOption> ComboBoxOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
        modelBuilder.ApplyConfiguration(new ComboBoxConfiguration());
        modelBuilder.ApplyConfiguration(new SingleLineConfiguration());
        modelBuilder.ApplyConfiguration(new JobPostingComboBoxConfiguration());
        modelBuilder.ApplyConfiguration(new JobPostingSingleLineConfiguration());
        modelBuilder.ApplyConfiguration(new ComboBoxOptionConfiguration());
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

public class JobPostingComboBoxConfiguration : IEntityTypeConfiguration<JobPostingComboBox>
{
    public void Configure(EntityTypeBuilder<JobPostingComboBox> entity)
    {
        entity.ToTable("JobPostingComboBoxes");

        entity.HasKey(e => new { e.JobPostingID, e.Key });

        entity.HasOne(x => x.ComboBox)
            .WithMany(x => x.JobPostingComboBoxes)
            .HasForeignKey(x => x.Key);

        entity.HasOne(x => x.JobPosting)
            .WithMany(x => x.JobPostingComboBoxes)
            .HasForeignKey(x => x.JobPostingID);
    }
}

public class JobPostingSingleLineConfiguration : IEntityTypeConfiguration<JobPostingSingleLine>
{
    public void Configure(EntityTypeBuilder<JobPostingSingleLine> entity)
    {
        entity.ToTable("JobPostingSingleLines");

        entity.HasKey(e => new { e.JobPostingID, e.Key });

        entity.HasOne(x => x.SingleLine)
            .WithMany(x => x.JobPostingSingleLines)
            .HasForeignKey(x => x.Key);

        entity.HasOne(x => x.JobPosting)
            .WithMany(x => x.JobPostingSingleLines)
            .HasForeignKey(x => x.JobPostingID);
    }
}

public class ComboBoxOptionConfiguration : IEntityTypeConfiguration<ComboBoxOption>
{
    public void Configure(EntityTypeBuilder<ComboBoxOption> entity)
    {
        entity.ToTable("ComboBoxOptions");

        entity.HasKey(e => new { e.OptionValue, e.Key });

        entity.HasOne(x => x.ComboBox)
            .WithMany(x => x.ComboBoxOptions)
            .HasForeignKey(x => x.Key);

        entity.HasOne(x => x.SelectedComboBox)
            .WithOne(x => x.SelectedComboBoxOption)
            .HasForeignKey<ComboBox>(x => new { x.SelectedOptionValue, x.Key })
            .IsRequired(false);
    }
}

public class ComboBoxConfiguration : IEntityTypeConfiguration<ComboBox>
{
    public void Configure(EntityTypeBuilder<ComboBox> entity)
    {
        entity.ToTable("ComboBoxes");

        entity.HasKey(e => e.Key);

        // Value always comes from LinkedIn
        entity.Property(e => e.Key).ValueGeneratedNever();
    }
}

public class SingleLineConfiguration : IEntityTypeConfiguration<SingleLine>
{
    public void Configure(EntityTypeBuilder<SingleLine> entity)
    {
        entity.ToTable("SingleLines");

        entity.HasKey(e => e.Key);

        // Value always comes from LinkedIn
        entity.Property(e => e.Key).ValueGeneratedNever();
    }
}

// Link table
public class JobPostingComboBox
{
    public long JobPostingID { get; set; }
    public string Key { get; set; } = null!;
    public JobPosting JobPosting { get; set; } = null!;
    public ComboBox ComboBox { get; set; } = null!;
}

// Link table
public class JobPostingSingleLine
{
    public long JobPostingID { get; set; }
    public string Key { get; set; } = null!;
    public JobPosting JobPosting { get; set; } = null!;
    public SingleLine SingleLine { get; set; } = null!;
}

public class ComboBox
{
    public ComboBox()
    {
        JobPostingComboBoxes = new HashSet<JobPostingComboBox>();
        ComboBoxOptions = new HashSet<ComboBoxOption>();
    }
    public string Key { get; set; } = null!;
    public string? SelectedOptionValue { get; set; }
    public ComboBoxOption? SelectedComboBoxOption { get; set; }
    public HashSet<JobPostingComboBox> JobPostingComboBoxes { get; set; }
    public HashSet<ComboBoxOption> ComboBoxOptions { get; set; }
}

public class ComboBoxOption
{
    public string Key { get; set; } = null!;
    public string OptionValue { get; set; } = null!;
    public ComboBox ComboBox { get; set; } = null!;
    public ComboBox? SelectedComboBox { get; set; }
}

public class SingleLine
{
    public SingleLine()
    {
        JobPostingSingleLines = new HashSet<JobPostingSingleLine>();
    }
    public string Key { get; set; } = null!;
    public string? SingleLineValue { get; set; } = null!;
    public HashSet<JobPostingSingleLine> JobPostingSingleLines { get; set; }
}

public class JobPosting
{
    public JobPosting()
    {
        JobPostingComboBoxes = new HashSet<JobPostingComboBox>();
        JobPostingSingleLines = new HashSet<JobPostingSingleLine>();
    }
    public HashSet<JobPostingComboBox> JobPostingComboBoxes { get; set; }
    public HashSet<JobPostingSingleLine> JobPostingSingleLines { get; set; }
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
