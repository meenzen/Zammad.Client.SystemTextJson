namespace Zammad.Client.Resources;

/// <summary>
/// The 500 most recent data privacy tasks, grouped by state. Newest first.
/// </summary>
public sealed class DataPrivacyTasksByState
{
    public List<DataPrivacyTask> InProcess { get; set; } = [];
    public List<DataPrivacyTask> Failed { get; set; } = [];
    public List<DataPrivacyTask> Completed { get; set; } = [];
}
