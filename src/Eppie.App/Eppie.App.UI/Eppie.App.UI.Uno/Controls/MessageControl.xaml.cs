using System;
using System.Threading.Tasks;
using Tuvi.App.Shared.Controls;

namespace Eppie.App.UI.Controls
{
    public sealed partial class MessageControl : BaseUserControl
    {
        private async Task UpdateHtmlView()
        {
            await HtmlView.EnsureCoreWebView2Async();

            HtmlView.CoreWebView2.Settings.IsScriptEnabled = false; // ToDo: Uno0001
            HtmlView.NavigateToString(HtmlBody);
        }
    }
}
