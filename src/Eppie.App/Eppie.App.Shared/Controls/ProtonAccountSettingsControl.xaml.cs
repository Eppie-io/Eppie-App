﻿using Tuvi.App.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Controls
{
    public sealed partial class ProtonAccountSettingsControl : UserControl
    {
        public ProtonAccountSettingsModel AccountSettingsModel
        {
            get { return (ProtonAccountSettingsModel)GetValue(AccountSettingsModelProperty); }
            set { SetValue(AccountSettingsModelProperty, value); }
        }
        public static readonly DependencyProperty AccountSettingsModelProperty =
            DependencyProperty.Register(nameof(AccountSettingsModel), typeof(ProtonAccountSettingsModel), typeof(ProtonAccountSettingsControl), new PropertyMetadata(null));

        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(ProtonAccountSettingsControl), new PropertyMetadata(false));

        public bool IsEmailReadOnly
        {
            get { return (bool)GetValue(IsEmailReadOnlyProperty); }
            set { SetValue(IsEmailReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsEmailReadOnlyProperty =
            DependencyProperty.Register(nameof(IsEmailReadOnly), typeof(bool), typeof(ProtonAccountSettingsControl), new PropertyMetadata(false));


        public ProtonAccountSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
