using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Tuvi.App.Services;
using Tuvi.App.ViewModels.Services;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Behaviors
{
    public class FileBehavior : Behavior<Button>, IFileOperationProvider
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(FileBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public Task<IEnumerable<AttachedFileInfo>> LoadFilesAsync()
        {
            return FilePickerService.SelectAndLoadFilesDataAsync();
        }

        public async Task SaveToFileAsync(byte[] data, string fileName)
        {
            await FilePickerService.SaveDataToFileAsync(fileName, data).ConfigureAwait(true);
        }

        public async Task SaveToTempFileAndOpenAsync(byte[] data, string fileName)
        {
            await FilePickerService.SaveDataToTempFileAndOpenAsync(fileName, data).ConfigureAwait(true);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
        }

        protected virtual void OnClick(object sender, RoutedEventArgs e)
        {
            Command?.Execute(this);
        }
    }
}
