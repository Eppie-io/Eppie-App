// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class AttachmentFileControl : UserControl
    {
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(nameof(FileName), typeof(string), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public ImageInfo FileThumbnail
        {
            get { return (ImageInfo)GetValue(FileThumbnailProperty); }
            set { SetValue(FileThumbnailProperty, value); }
        }

        public static readonly DependencyProperty FileThumbnailProperty =
            DependencyProperty.Register(nameof(FileThumbnail), typeof(ImageInfo), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public string FileSize
        {
            get { return (string)GetValue(FileSizeProperty); }
            set { SetValue(FileSizeProperty, value); }
        }

        public static readonly DependencyProperty FileSizeProperty =
            DependencyProperty.Register(nameof(FileSize), typeof(string), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public string FileExtension
        {
            get { return (string)GetValue(FileExtensionProperty); }
            set { SetValue(FileExtensionProperty, value); }
        }

        public static readonly DependencyProperty FileExtensionProperty =
            DependencyProperty.Register(nameof(FileExtension), typeof(string), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public bool IsFileEmpty
        {
            get { return (bool)GetValue(IsFileEmptyProperty); }
            set { SetValue(IsFileEmptyProperty, value); }
        }

        public static readonly DependencyProperty IsFileEmptyProperty =
            DependencyProperty.Register(nameof(IsFileEmpty), typeof(bool), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public ICommand OpenFileCommand
        {
            get { return (ICommand)GetValue(OpenFileCommandProperty); }
            set { SetValue(OpenFileCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenFileCommandProperty =
            DependencyProperty.Register(nameof(OpenFileCommand), typeof(ICommand), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public object OpenFileCommandParameter
        {
            get { return (object)GetValue(OpenFileCommandParameterProperty); }
            set { SetValue(OpenFileCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty OpenFileCommandParameterProperty =
            DependencyProperty.Register(nameof(OpenFileCommandParameter), typeof(object), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public ICommand SaveFileCommand
        {
            get { return (ICommand)GetValue(SaveFileCommandProperty); }
            set { SetValue(SaveFileCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveFileCommandProperty =
            DependencyProperty.Register(nameof(SaveFileCommand), typeof(ICommand), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public object SaveFileCommandParameter
        {
            get { return (object)GetValue(SaveFileCommandParameterProperty); }
            set { SetValue(SaveFileCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty SaveFileCommandParameterProperty =
            DependencyProperty.Register(nameof(SaveFileCommandParameter), typeof(object), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public ICommand DeleteFileCommand
        {
            get { return (ICommand)GetValue(DeleteFileCommandProperty); }
            set { SetValue(DeleteFileCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteFileCommandProperty =
            DependencyProperty.Register(nameof(DeleteFileCommand), typeof(ICommand), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public object DeleteFileCommandParameter
        {
            get { return (object)GetValue(DeleteFileCommandParameterProperty); }
            set { SetValue(DeleteFileCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty DeleteFileCommandParameterProperty =
            DependencyProperty.Register(nameof(DeleteFileCommandParameter), typeof(object), typeof(AttachmentFileControl), new PropertyMetadata(null));


        public AttachmentFileControl()
        {
            this.InitializeComponent();
        }

        private void OnInvoked(object sender, EventArgs e)
        {
            if (Resources.TryGetValue("CommandMenu", out object resource) && resource is MenuFlyout menuFlyout)
            {
                menuFlyout.ShowAt(this);
            }
        }
    }
}
