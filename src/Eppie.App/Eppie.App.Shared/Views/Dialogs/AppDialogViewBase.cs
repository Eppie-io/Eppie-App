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
using Eppie.App.Helpers;
using Eppie.App.UI.Controls;
using Tuvi.App.ViewModels;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.Views.Dialogs
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]

    public abstract partial class AppDialogViewBase<TViewModel> : UserControl, IAppDialogView, IAppDialogLifecycle
        where TViewModel : DialogViewModel, new()
    {
        protected TViewModel ViewModel { get; private set; }

        public event EventHandler CloseRequested;

        protected AppDialogViewBase()
        {
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ViewModel = new TViewModel();
            DataContext = ViewModel;
            ViewModel.Initialize();
        }

        public virtual void OnOpened()
        {
            ViewModel?.OnOpened();
        }

        public virtual void OnClosing(ContentDialogResult result, out bool cancel)
        {
            cancel = false;
            ViewModel?.OnClosing(ConvertResult(result), out cancel);
        }

        public virtual void OnClosed(ContentDialogResult result)
        {
            ViewModel?.OnClosed(ConvertResult(result));
        }

        protected void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private static DialogViewModel.Result ConvertResult(ContentDialogResult result)
        {
            switch (result)
            {
                case ContentDialogResult.Primary:
                    return DialogViewModel.Result.Primary;
                case ContentDialogResult.Secondary:
                    return DialogViewModel.Result.Secondary;
                default:
                    return DialogViewModel.Result.None;
            }
        }
    }

    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]

    public abstract partial class AppDialogViewBase<TViewModel, TData> : AppDialogViewBase<TViewModel>, IAppDialogInitializer<TData>
        where TViewModel : DialogViewModel<TData>, new()
    {
        protected AppDialogViewBase() : base()
        {
        }

        public void Initialize(TData data)
        {
            ViewModel?.InitializeData(data);
        }
    }
}
