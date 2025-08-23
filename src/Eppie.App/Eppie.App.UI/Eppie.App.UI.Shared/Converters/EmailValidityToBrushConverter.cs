// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2025 Eppie (https://eppie.io)                                    //
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
using EmailValidation;
using Tuvi.Core.Entities;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public class EmailValidityToStyleConverter : IValueConverter
    {
        public Style ValidEmailStyle { get; set; }
        public Style InvalidEmailStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var email = value as EmailAddress;
            if (email != null && EmailValidator.Validate(email.StandardAddress, allowTopLevelDomains: true))
            {
                return ValidEmailStyle;
            }

            return InvalidEmailStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
