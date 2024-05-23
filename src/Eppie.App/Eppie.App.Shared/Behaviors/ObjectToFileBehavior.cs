using Tuvi.Core.Entities;
using System;
using Tuvi.App.ViewModels.Services;
using Windows.UI.Xaml;

namespace Tuvi.App.Shared.Behaviors
{
    public class AttachmentFileBehavior : ObjectToFileBehavior<Attachment> { }

    public class ObjectToFileBehavior<TObject> : FileBehavior
    {
        protected override void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is TObject objectToSave)
            {
                Command?.Execute(new Tuple<IFileOperationProvider, TObject>(this, objectToSave));
            }
        }
    }
}
