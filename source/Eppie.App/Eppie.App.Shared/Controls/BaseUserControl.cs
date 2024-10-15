using Tuvi.Core.Entities;
using System;
using Windows.UI.Xaml.Controls;

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
