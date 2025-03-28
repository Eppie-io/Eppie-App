namespace Tuvi.App.ViewModels.Services
{
    /// <summary>
    /// Service interface to store and retrieve local settings
    /// WARNING: Settings are not encrypted!
    /// </summary>
    public interface ILocalSettingsService
    {
        string Language { get; set; }
        string SelectedMailFilterForAllMessagesPage { get; set; }
        string SelectedMailFilterForFolderMessagesPage { get; set; }
        string SelectedMailFilterForContactMessagesPage { get; set; }
    }
}
