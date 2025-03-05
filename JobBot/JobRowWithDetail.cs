

using JobBot.PageObjectModels;

namespace JobBot;
public class JobRowWithDetail
{
    public JobRowWithDetail(RowDto row, JobDetailPane pane)
    {
        JobRow = row;
        JobDetailPane = pane;
    }
    public RowDto JobRow { get; set; }
    public JobDetailPane JobDetailPane { get; set; }
}

public interface IDetail
{
    Header Header { get; }
    HeaderDto HeaderDto { get; }
    JobDetails JobDetails { get; }
}