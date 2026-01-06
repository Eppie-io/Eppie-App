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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuvi.App.ViewModels.Validation
{
    public interface IValidatableProperty : INotifyPropertyChanged
    {
        ObservableCollection<string> Errors { get; }
        bool HasErrors { get; }
    }

    public class ValidatableProperty<T> : ObservableObject, IValidatableProperty
    {
        public ValidatableProperty()
        {
            Errors.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(HasErrors));
        }

        public bool HasErrors
        {
            get
            {
                return NeedsValidation && Errors.Any();
            }
        }

        public ObservableCollection<string> Errors { get; private set; } = new ObservableCollection<string>();

        public void SetInitialValue(T value)
        {
            SetProperty(ref _value, value);
        }

        private bool _needValidation;
        public bool NeedsValidation
        {
            get { return _needValidation; }
            set
            {
                SetProperty(ref _needValidation, value);
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                if (SetProperty(ref _value, value))
                {
                    Errors.Clear();
                    NeedsValidation = true;
                }
            }
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
