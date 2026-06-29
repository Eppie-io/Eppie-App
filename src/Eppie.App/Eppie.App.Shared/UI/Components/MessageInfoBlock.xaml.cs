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
using Eppie.App.UI.Resources;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.UI.Components
{
    public enum MessageInfoBlockMode
    {
        NameAddress,        // displays name, address, message recipients, metadata (date, time, attachments, encryption and signature status)
        NameAddressSubject  // displays name, address, message subject, message recipients, metadata (date, time, attachments, encryption and signature status)
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class MessageInfoBlock : UserControl
    {
        public MessageInfoBlockMode Mode
        {
            get { return (MessageInfoBlockMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(MessageInfoBlockMode), typeof(MessageInfoBlock), new PropertyMetadata(MessageInfoBlockMode.NameAddress));


        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register(nameof(Subject), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(nameof(Address), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public ImageSource Avatar
        {
            get { return (ImageSource)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register(nameof(Avatar), typeof(ImageSource), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public bool IsEncrypted
        {
            get { return (bool)GetValue(IsEncryptedProperty); }
            set { SetValue(IsEncryptedProperty, value); }
        }

        public static readonly DependencyProperty IsEncryptedProperty =
            DependencyProperty.Register(nameof(IsEncrypted), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false, OnIsEncryptedPropertyChanged));

        private static void OnIsEncryptedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageInfoBlock control)
            {
                control.UpdateControlProperties();
            }
        }

        public bool IsSigned
        {
            get { return (bool)GetValue(IsSignedProperty); }
            set { SetValue(IsSignedProperty, value); }
        }

        public static readonly DependencyProperty IsSignedProperty =
            DependencyProperty.Register(nameof(IsSigned), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false, OnIsSignedPropertyChanged));

        private static void OnIsSignedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageInfoBlock control)
            {
                control.UpdateControlProperties();
            }
        }

        public bool HasAttachments
        {
            get { return (bool)GetValue(HasAttachmentsProperty); }
            set { SetValue(HasAttachmentsProperty, value); }
        }

        public static readonly DependencyProperty HasAttachmentsProperty =
            DependencyProperty.Register(nameof(HasAttachments), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false));


        public int AttachmentCount
        {
            get { return (int)GetValue(AttachmentCountProperty); }
            set { SetValue(AttachmentCountProperty, value); }
        }

        public static readonly DependencyProperty AttachmentCountProperty =
            DependencyProperty.Register(nameof(AttachmentCount), typeof(int), typeof(MessageInfoBlock), new PropertyMetadata(0));


        public object Recipients
        {
            get { return (object)GetValue(RecipientsProperty); }
            set { SetValue(RecipientsProperty, value); }
        }

        public static readonly DependencyProperty RecipientsProperty =
            DependencyProperty.Register(nameof(Recipients), typeof(object), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public Account PrimaryRecepient
        {
            get { return (Account)GetValue(PrimaryRecepientProperty); }
            set { SetValue(PrimaryRecepientProperty, value); }
        }

        public static readonly DependencyProperty PrimaryRecepientProperty =
            DependencyProperty.Register(nameof(PrimaryRecepient), typeof(Account), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public string FullDateTime
        {
            get { return (string)GetValue(FullDateTimeProperty); }
            set { SetValue(FullDateTimeProperty, value); }
        }

        public static readonly DependencyProperty FullDateTimeProperty =
            DependencyProperty.Register(nameof(FullDateTime), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public string Date
        {
            get { return (string)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(nameof(Time), typeof(string), typeof(MessageInfoBlock), new PropertyMetadata(null));


        public bool IsToday
        {
            get { return (bool)GetValue(IsTodayProperty); }
            set { SetValue(IsTodayProperty, value); }
        }

        public static readonly DependencyProperty IsTodayProperty =
            DependencyProperty.Register(nameof(IsToday), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false));


        public bool IsDecentralized
        {
            get { return (bool)GetValue(IsDecentralizedProperty); }
            set { SetValue(IsDecentralizedProperty, value); }
        }


        public static readonly DependencyProperty IsDecentralizedProperty =
            DependencyProperty.Register(nameof(IsDecentralized), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false));


        public bool IsRead
        {
            get { return (bool)GetValue(IsReadProperty); }
            set { SetValue(IsReadProperty, value); }
        }

        public static readonly DependencyProperty IsReadProperty =
            DependencyProperty.Register(nameof(IsRead), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false));

        public bool IsTextSelectionEnabled
        {
            get { return (bool)GetValue(IsTextSelectionEnabledProperty); }
            set { SetValue(IsTextSelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register(nameof(IsTextSelectionEnabled), typeof(bool), typeof(MessageInfoBlock), new PropertyMetadata(false));


        private bool IsSecure { get; set; }
        private string SecureTooltip { get; set; }

        private string EncryptedAndSignedTooltip { get; set; }
        private string EncryptedTooltip { get; set; }
        private string SignedTooltip { get; set; }
        private string NotSecureTooltip { get; set; }

        public MessageInfoBlock()
        {
            InitializeTooltipStrings();
            InitializeComponent();
            UpdateControlProperties();
        }

        private void InitializeTooltipStrings()
        {
            StringProvider stringProvider = StringProvider.GetInstance();

            EncryptedAndSignedTooltip = stringProvider.GetString("MessageEncryptedAndSigned/Tooltip");
            EncryptedTooltip = stringProvider.GetString("MessageEncrypted/Tooltip");
            SignedTooltip = stringProvider.GetString("MessageSigned/Tooltip");
            NotSecureTooltip = stringProvider.GetString("MessageNotSecure/Tooltip");
        }

        private void UpdateControlProperties()
        {
            IsSecure = IsEncrypted || IsSigned;

            if (IsEncrypted && IsSigned)
            {
                SecureTooltip = EncryptedAndSignedTooltip;
            }
            else if (IsEncrypted)
            {
                SecureTooltip = EncryptedTooltip;
            }
            else if (IsSigned)
            {
                SecureTooltip = SignedTooltip;
            }
            else
            {
                SecureTooltip = NotSecureTooltip;
            }

            Bindings.Update();
        }
    }
}
