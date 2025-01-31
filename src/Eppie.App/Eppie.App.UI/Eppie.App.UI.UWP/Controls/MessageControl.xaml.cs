using System.Threading.Tasks;
using Tuvi.App.Shared.Controls;

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : BaseUserControl
    {
        private Task UpdateHtmlView()
        {
            HtmlView.Settings.IsJavaScriptEnabled = false;
            HtmlView.NavigateToString(HtmlBody);

            return Task.CompletedTask;
        }
    }
}
