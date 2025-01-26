

namespace JobBot;
public class JobRowWithDetail
{
    public JobRowWithDetail(JobRow row, JobDetailPane pane)
    {
        JobRow = row;
        JobDetailPane = pane;
    }
    public JobRow JobRow { get; set; }
    public JobDetailPane JobDetailPane { get; set; }
}
