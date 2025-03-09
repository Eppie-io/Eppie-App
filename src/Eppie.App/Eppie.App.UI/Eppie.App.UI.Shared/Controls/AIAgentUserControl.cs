using Tuvi.Core.Entities;
using System;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Controls
{
    public partial class AIAgentUserControl : BaseUserControl
    {
        public string AIAgentProcessedBody
        {
            get { return (string)GetValue(AIAgentProcessedBodyProperty); }
            set
            {
                SetValue(AIAgentProcessedBodyProperty, value);
                SetValue(HasAIAgentProcessedBodyProperty, !string.IsNullOrEmpty(AIAgentProcessedBody));
                if (HasAIAgentProcessedBody)
                {
                    ShowAIAgentProcessedText();
                }
            }
        }

        public static readonly DependencyProperty AIAgentProcessedBodyProperty =
            DependencyProperty.Register(nameof(AIAgentProcessedBody), typeof(string), typeof(AIAgentUserControl), new PropertyMetadata(null));

        public bool HasAIAgentProcessedBody
        {
            get { return (bool)GetValue(HasAIAgentProcessedBodyProperty); }
            set { SetValue(HasAIAgentProcessedBodyProperty, value); }
        }

        public static readonly DependencyProperty HasAIAgentProcessedBodyProperty =
            DependencyProperty.Register(nameof(HasAIAgentProcessedBody), typeof(bool), typeof(AIAgentUserControl), new PropertyMetadata(null));

        virtual protected void ShowAIAgentProcessedText()
        {
        }
    }
}
