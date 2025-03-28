﻿// ---------------------------------------------------------------------------- //
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
