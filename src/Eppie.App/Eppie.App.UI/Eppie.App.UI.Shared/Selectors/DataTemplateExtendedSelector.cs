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
using System.Linq;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Selectors
{
    public class DataTemplateRule
    {
        public IDataTemplateCondition Condition { get; set; }
        public DataTemplate DataTemplate { get; set; }

        public bool IsCorrect(object item, DependencyObject container, object options)
        {
            return Condition?.IsTrue(item, container, options) ?? false;
        }
    }

    public partial class DataTemplateExtendedSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }

        public ICollection<DataTemplateRule> Rules { get; set; } = new List<DataTemplateRule>();

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            object options = GetOptions(item, container);
            DataTemplateRule correctRule = Rules.FirstOrDefault(rule => rule.IsCorrect(item, container, options));

            return correctRule?.DataTemplate ?? DefaultTemplate ?? base.SelectTemplateCore(item, container);
        }

        protected virtual object GetOptions(object item, DependencyObject container)
        {
            return null;
        }
    }
}
