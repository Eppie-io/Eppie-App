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

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
#endif

namespace Eppie.App.UI.Controls
{
    public enum SidePanePlacement
    {
        Left,
        Right
    }

    public enum LayoutState
    {
        Normal,
        Narrow,
        Compact,
    }

    public class LayoutStateChangedEventArgs : EventArgs
    {
        public LayoutState NewState { get; }

        public LayoutState PreviousState { get; }

        internal LayoutStateChangedEventArgs(LayoutState previousState, LayoutState newState)
            : base()
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }


    [SuppressMessage("Design", "CA1010:Generic collections should implement generic interface", Justification = "ContentControl implements IEnumerable for XAML infrastructure")]
    [ContentProperty(Name = nameof(Child))]
    public sealed partial class SidePane : UserControl
    {
        public SidePanePlacement PanePlacement
        {
            get { return (SidePanePlacement)GetValue(PanePlacementProperty); }
            set { SetValue(PanePlacementProperty, value); }
        }

        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register(nameof(PanePlacement), typeof(SidePanePlacement), typeof(SidePane), new PropertyMetadata(SidePanePlacement.Left));


        public bool IsPaneOpen
        {
            get { return (bool)GetValue(IsPaneOpenProperty); }
            set { SetValue(IsPaneOpenProperty, value); }
        }

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(SidePane), new PropertyMetadata(true));


        public double OpenPaneLength
        {
            get { return (double)GetValue(OpenPaneLengthProperty); }
            set { SetValue(OpenPaneLengthProperty, value); }
        }

        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(SidePane), new PropertyMetadata(300.0, OnOpenPaneLengthChanged));

        private static void OnOpenPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePane sidePane)
            {
                sidePane.OpenPaneLength = Clamp(sidePane.OpenPaneLength, sidePane.OpenPaneMinLength, sidePane.OpenPaneMaxLength);
            }
        }

        public double OpenPaneMaxLength
        {
            get { return (double)GetValue(OpenPaneMaxLengthProperty); }
            set { SetValue(OpenPaneMaxLengthProperty, value); }
        }

        public static readonly DependencyProperty OpenPaneMaxLengthProperty =
            DependencyProperty.Register(nameof(OpenPaneMaxLength), typeof(double), typeof(SidePane), new PropertyMetadata(320.0, OnOpenPaneMaxLengthChanged));

        private static void OnOpenPaneMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePane sidePane)
            {
                sidePane.OpenPaneMinLength = Math.Min(sidePane.OpenPaneMinLength, sidePane.OpenPaneMaxLength);
                sidePane.UpdateSidePane();
            }
        }

        public double OpenPaneMinLength
        {
            get { return (double)GetValue(OpenPaneMinLengthProperty); }
            set { SetValue(OpenPaneMinLengthProperty, value); }
        }

        public static readonly DependencyProperty OpenPaneMinLengthProperty =
            DependencyProperty.Register(nameof(OpenPaneMinLength), typeof(double), typeof(SidePane), new PropertyMetadata(280.0, OnOpenPaneMinLengthChanged));

        private static void OnOpenPaneMinLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePane sidePane)
            {
                sidePane.OpenPaneMaxLength = Math.Max(sidePane.OpenPaneMinLength, sidePane.OpenPaneMaxLength);
                sidePane.UpdateSidePane();
            }
        }

        public double ChildMinWidth
        {
            get { return (double)GetValue(ChildMinWidthProperty); }
            set { SetValue(ChildMinWidthProperty, value); }
        }

        public static readonly DependencyProperty ChildMinWidthProperty =
            DependencyProperty.Register(nameof(ChildMinWidth), typeof(double), typeof(SidePane), new PropertyMetadata(0.0, OnChildMinWidthChanged));

        private static void OnChildMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePane sidePane)
            {
                sidePane.UpdateSidePane();
            }
        }

        public double BoundaryLineSize
        {
            get { return (double)GetValue(BoundaryLineSizeProperty); }
            set { SetValue(BoundaryLineSizeProperty, value); }
        }

        public static readonly DependencyProperty BoundaryLineSizeProperty =
            DependencyProperty.Register(nameof(BoundaryLineSize), typeof(double), typeof(SidePane), new PropertyMetadata(4.0, OnBoundaryLineSizeChanged));

        private static void OnBoundaryLineSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePane sidePane)
            {
                sidePane.UpdateSidePane();
            }
        }

        public bool IsThumbVisible
        {
            get { return (bool)GetValue(IsThumbVisibleProperty); }
            set { SetValue(IsThumbVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsThumbVisibleProperty =
            DependencyProperty.Register(nameof(IsThumbVisible), typeof(bool), typeof(SidePane), new PropertyMetadata(false));


        public UIElement Pane
        {
            get { return (UIElement)GetValue(PaneProperty); }
            set { SetValue(PaneProperty, value); }
        }

        public static readonly DependencyProperty PaneProperty =
            DependencyProperty.Register(nameof(Pane), typeof(UIElement), typeof(SidePane), new PropertyMetadata(null));


        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(SidePane), new PropertyMetadata(null));


        public event EventHandler<LayoutStateChangedEventArgs> LayoutStateChanged;
        public event EventHandler<SizeChangedEventArgs> SidePaneSizeChanged;


        private LayoutState _currentState = LayoutState.Normal;
        private double? _sidePaneLengthCache;
        private bool? _isPaneOpenCache;


        public SidePane()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSidePane();
        }

        private void OnSidePaneSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_currentState != LayoutState.Compact)
            {
                OpenPaneLength = SidePaneContent.Width;
            }

            SidePaneSizeChanged?.Invoke(this, e);
        }

        private void OnLayoutStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            LayoutState previousState = LayoutState.Normal;
            switch (e?.OldState?.Name)
            {
                case nameof(LayoutState.Compact):
                    previousState = LayoutState.Compact;
                    break;
                case nameof(LayoutState.Narrow):
                    previousState = LayoutState.Narrow;
                    break;
                case nameof(LayoutState.Normal):
                    previousState = LayoutState.Normal;
                    break;
            }

            switch (e?.NewState?.Name)
            {
                case nameof(LayoutState.Compact):
                    _currentState = LayoutState.Compact;
                    StoreSidePane();
                    break;
                case nameof(LayoutState.Narrow):
                    _currentState = LayoutState.Narrow;
                    break;
                case nameof(LayoutState.Normal):
                    _currentState = LayoutState.Normal;
                    break;
            }

            UpdateSidePane();

            if (previousState == LayoutState.Compact)
            {
                RestoreSidePane();
            }

            LayoutStateChanged?.Invoke(this, new LayoutStateChangedEventArgs(previousState, _currentState));
        }

        private void RestoreSidePane()
        {
            if (_sidePaneLengthCache.HasValue)
            {
                SidePaneContent.Width = _sidePaneLengthCache.Value;
                _sidePaneLengthCache = null;
            }

            if (_isPaneOpenCache.HasValue)
            {
                IsPaneOpen = _isPaneOpenCache.Value || IsPaneOpen;
                _isPaneOpenCache = null;
            }
        }

        private void StoreSidePane()
        {
            if (!_sidePaneLengthCache.HasValue)
            {
                _sidePaneLengthCache = SidePaneContent.Width;
            }

            if (!_isPaneOpenCache.HasValue)
            {
                _isPaneOpenCache = IsPaneOpen;
                IsPaneOpen = false;
            }
        }

        private void UpdateSidePane()
        {
            if (_currentState == LayoutState.Compact)
            {
                SidePaneContent.MaxWidth = double.PositiveInfinity;
                SidePaneContent.Width = RootGrid.ActualWidth;
            }

            if (_currentState == LayoutState.Narrow)
            {
                const double gap = 1.0;
                SidePaneContent.MaxWidth = Math.Min(RootGrid.ActualWidth - ChildMinWidth - BoundaryLineSize - gap, OpenPaneMaxLength);
            }

            if (_currentState == LayoutState.Normal)
            {
                SidePaneContent.MaxWidth = OpenPaneMaxLength;
            }
        }

        private static double GetCompactLayoutWidth(double openPaneMinLength, double childMinWidth, double boundaryLineSize)
        {
            const double gap = 1.0;
            return openPaneMinLength + childMinWidth + boundaryLineSize + gap;
        }

        private static double GetNormalLayoutWidth(double openPaneMaxLength, double childMinWidth, double boundaryLineSize)
        {
            const double gap = 1.0;
            return openPaneMaxLength + childMinWidth + boundaryLineSize + gap;
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
    }
}
