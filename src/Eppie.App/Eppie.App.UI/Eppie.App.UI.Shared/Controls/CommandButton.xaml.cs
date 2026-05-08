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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
#endif

namespace Eppie.App.UI.Controls
{
    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    public sealed partial class CommandButton : UserControl
    {
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(CommandButton), new PropertyMetadata(null));


        public IconElement Icon
        {
            get { return (IconElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(CommandButton), new PropertyMetadata(null));


        public bool IsCompact
        {
            get { return (bool)GetValue(IsCompactProperty); }
            set { SetValue(IsCompactProperty, value); }
        }

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(CommandButton), new PropertyMetadata(false));


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandButton), new PropertyMetadata(null, OnCommandChanged));

        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is CommandButton commandButton)
            {
                string oldDescription = GetCommandDescription(args.OldValue);
                string newDescription = GetCommandDescription(args.NewValue);

                if (newDescription != null || oldDescription != null)
                {
                    commandButton.UpdateToolTip();
                }
            }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandButton), new PropertyMetadata(null));


        public FlyoutBase Flyout
        {
            get { return (FlyoutBase)GetValue(FlyoutProperty); }
            set { SetValue(FlyoutProperty, value); }
        }

        public static readonly DependencyProperty FlyoutProperty =
            DependencyProperty.Register(nameof(Flyout), typeof(FlyoutBase), typeof(CommandButton), new PropertyMetadata(null));


        public event EventHandler Invoked;

        public CommandButton()
        {
            this.InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Invoked?.Invoke(this, EventArgs.Empty);
        }

        internal static IconElement GetIcon(IconElement icon, ICommand command)
        {
            if (icon != null)
            {
                return icon;
            }

            if (command is XamlUICommand xamlUICommand)
            {
                return new IconSourceElement()
                {
                    IconSource = xamlUICommand.IconSource
                };
            }

            return null;
        }

        internal static string GetLabel(string label, ICommand command)
        {
            if (label != null)
            {
                return label;
            }

            if (command is XamlUICommand xamlUICommand)
            {
                return xamlUICommand.Label;
            }

            return null;
        }

        internal static ICommand GetCommand(ICommand command)
        {
            if (command is XamlUICommand xamlUICommand)
            {
                return xamlUICommand.Command;
            }

            return command;
        }

        private void UpdateToolTip()
        {
            string description = GetCommandDescription(Command);
            ToolTipService.SetToolTip(this, description);
        }

        private static string GetCommandDescription(object command)
        {
            if (command is XamlUICommand xamlUICommand && xamlUICommand.Description != string.Empty)
            {
                return xamlUICommand.Description;
            }

            return null;
        }
    }
}
