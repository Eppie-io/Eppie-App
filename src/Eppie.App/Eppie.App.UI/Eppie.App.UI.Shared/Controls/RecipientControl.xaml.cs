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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.UI.Controls
{
    public interface IRecipientItem
    {
        string Address { get; }
        string DisplayName { get; }
        bool IsSecure { get; }
        ImageSource Avatar { get; }
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class RecipientControl : UserControl
    {
        public IRecipientItem PrimaryRecepient
        {
            get { return (IRecipientItem)GetValue(PrimaryRecepientProperty); }
            set { SetValue(PrimaryRecepientProperty, value); }
        }

        public static readonly DependencyProperty PrimaryRecepientProperty =
            DependencyProperty.Register(nameof(PrimaryRecepient), typeof(IRecipientItem), typeof(RecipientControl), new PropertyMetadata(null));


        public IReadOnlyCollection<IRecipientItem> ItemsSource
        {
            get { return (IReadOnlyCollection<IRecipientItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IReadOnlyCollection<IRecipientItem>), typeof(RecipientControl), new PropertyMetadata(null));


        public RecipientControl()
        {
            this.InitializeComponent();
        }

        public static ImageSource GetPrimaryRecepientAvatar(IRecipientItem primaryRecipient, IReadOnlyCollection<IRecipientItem> collection)
        {
            return GetPrimaryRecepient(primaryRecipient, collection)?.Avatar;
        }

        public static string GetPrimaryRecepientName(IRecipientItem primaryRecipient, IReadOnlyCollection<IRecipientItem> collection)
        {
            return GetPrimaryRecepient(primaryRecipient, collection)?.DisplayName;
        }

        public static string GetPrimaryRecepientAddress(IRecipientItem primaryRecipient, IReadOnlyCollection<IRecipientItem> collection)
        {
            return GetPrimaryRecepient(primaryRecipient, collection)?.Address;
        }

        public static bool IsPrimaryRecepientSecure(IRecipientItem primaryRecipient, IReadOnlyCollection<IRecipientItem> collection)
        {
            return GetPrimaryRecepient(primaryRecipient, collection)?.IsSecure ?? false;
        }

        public static int GetAdditionalRecipientCount(IReadOnlyCollection<IRecipientItem> collection)
        {
            const int primaryRecipientCount = 1;
            int count = collection?.Count ?? 0;

            // The result is the number of all recipients excluding the primary recipient(s).
            return count > primaryRecipientCount ? count - primaryRecipientCount : 0;
        }

        public static Visibility GetAdditionalRecipientVisibility(IReadOnlyCollection<IRecipientItem> collection)
        {
            return GetAdditionalRecipientCount(collection) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static IRecipientItem GetPrimaryRecepient(IRecipientItem primaryRecipient, IReadOnlyCollection<IRecipientItem> collection)
        {
            return primaryRecipient ?? collection?.FirstOrDefault();
        }
    }
}
