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
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    /// <summary>
    /// Immutable decision value for external content processing.
    /// </summary>
    public readonly struct ExternalContentDecision
    {
        public bool ShouldBlock { get; }
        public bool ShowBanner { get; }
        public ExternalContentDecision(bool shouldBlock, bool showBanner)
        {
            ShouldBlock = shouldBlock;
            ShowBanner = showBanner;
        }

        public static ExternalContentDecision Allow() => new ExternalContentDecision(false, false);
        public static ExternalContentDecision Block(bool showBanner) => new ExternalContentDecision(true, showBanner);
    }

    /// <summary>
    /// Policy-based decider for external content.
    /// </summary>
    public sealed class ExternalContentDecider
    {
        public ExternalContentPolicy Policy { get; set; } = ExternalContentPolicy.AlwaysAllow;

        public ExternalContentDecider() { }
        public ExternalContentDecider(ExternalContentPolicy policy) { Policy = policy; }

        /// <summary>
        /// Decide based on current Policy.
        /// </summary>
        public ExternalContentDecision Decide(string uri, bool allowOnce)
        {
            if (Policy == ExternalContentPolicy.AlwaysAllow || allowOnce)
            {
                return ExternalContentDecision.Allow();
            }

            if (string.IsNullOrEmpty(uri))
            {
                return ExternalContentDecision.Block(Policy == ExternalContentPolicy.AskEachTime);
            }

            if (IsEmbeddedScheme(uri))
            {
                return ExternalContentDecision.Allow();
            }

            return ExternalContentDecision.Block(Policy == ExternalContentPolicy.AskEachTime);
        }

        private static bool IsEmbeddedScheme(string uri)
        {
            return uri.StartsWith("blob:", StringComparison.OrdinalIgnoreCase) ||
                   uri.StartsWith("cid:", StringComparison.OrdinalIgnoreCase) ||
                   uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                   uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase);
        }
    }
}
