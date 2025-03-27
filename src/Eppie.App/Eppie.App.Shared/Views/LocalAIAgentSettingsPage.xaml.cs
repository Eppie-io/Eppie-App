using Tuvi.App.ViewModels;

namespace Tuvi.App.Shared.Views
{
    public partial class LocalAIAgentSettingsPageBase : BasePage<LocalAIAgentSettingsPageViewModel, BaseViewModel>
    {
    }

    public sealed partial class LocalAIAgentSettingsPage : LocalAIAgentSettingsPageBase
    {
        public LocalAIAgentSettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
