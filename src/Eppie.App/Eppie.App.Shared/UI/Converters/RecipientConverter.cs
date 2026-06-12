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
using System.Collections.Generic;
using System.Linq;
using Eppie.App.UI.Controls;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
#endif

namespace Eppie.App.UI.Converters
{
    public class RecipientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Account account)
            {
                return RecipientCreator.CreateRecipientItem(account.DisplayEmail);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ListRecipientsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<EmailAddress> source)
            {
                return new List<IRecipientItem>(source.Select(item => RecipientCreator.CreateRecipientItem(item)));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    internal class RecipientCreator
    {
        public class RecipientItem : IRecipientItem
        {
            public string Address { get; set; }
            public string DisplayName { get; set; }
            public bool IsSecure { get; set; }
            public ImageSource Avatar { get; set; }
        }

        public static IRecipientItem CreateRecipientItem(EmailAddress emailAddress)
        {
            return new RecipientItem
            {
                Address = emailAddress?.Address,
                DisplayName = emailAddress?.Name,
                IsSecure = emailAddress?.IsDecentralized ?? false,

                // ToDo: add avatar
                Avatar = null,
            };
        }
    }
}
