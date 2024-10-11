namespace TogglTimesheet.Timesheet
{
    public interface ITaskGenerator
    {
        string GenerateTask(string description, string project);
    }
}
