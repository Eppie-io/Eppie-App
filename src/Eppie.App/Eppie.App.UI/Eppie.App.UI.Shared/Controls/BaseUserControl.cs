using Tuvi.Core.Entities;
using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public partial class BaseUserControl : UserControl
    {
        public event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        public virtual void OnError(Exception ex)
        {
            ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(ex));
        }
    }
}
