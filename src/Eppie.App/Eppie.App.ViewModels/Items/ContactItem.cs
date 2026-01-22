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
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class ContactItem : ObservableObject
    {
        //TODO: TVM-270        
        public static readonly int DefaultAvatarSize = 240;

        private string _fullName = "";
        public string FullName
        {
            get { return _fullName; }
            set
            {
                SetProperty(ref _fullName, value);
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private EmailAddress _email;
        public EmailAddress Email
        {
            get { return _email; }
            set
            {
                SetProperty(ref _email, value);
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string DisplayName => string.IsNullOrEmpty(FullName) ? Email.DisplayName : FullName;

        private ImageInfo _avatarInfo;
        public ImageInfo AvatarInfo
        {
            get { return _avatarInfo; }
            set { SetProperty(ref _avatarInfo, value); }
        }

        private LastMessageData _lastMessageData;
        public LastMessageData LastMessageData
        {
            get { return _lastMessageData; }
            set { SetProperty(ref _lastMessageData, value); }
        }

        private int _unreadMessagesCount;
        public int UnreadMessagesCount
        {
            get { return _unreadMessagesCount; }
            set { SetProperty(ref _unreadMessagesCount, value); }
        }

        public ContactItem()
        {
        }

        public ContactItem(Contact contact)
        {
            if (contact is null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            Email = contact.Email;
            FullName = contact.FullName;
            AvatarInfo = contact.AvatarInfo;
            LastMessageData = contact.LastMessageData;
            UnreadMessagesCount = contact.UnreadCount;
        }

        public ContactItem(EmailAddress email)
        {
            if (email is null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            Email = email;
            FullName = email.Name;
        }

        public void UpdateFrom(ContactItem other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            FullName = other.FullName;
            AvatarInfo = other.AvatarInfo;
            LastMessageData = other.LastMessageData;
            UnreadMessagesCount = other.UnreadMessagesCount;
        }

        public override bool Equals(object obj)
        {
            if (obj is ContactItem other)
            {
                return Email.Equals(other.Email);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }

        public EmailAddress ToEmailAddress()
        {
            return new EmailAddress(Email.Address, FullName);
        }

        public override string ToString()
        {
            return Email?.Address;
        }
    }
}
