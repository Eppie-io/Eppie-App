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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class PageViewModel : BaseViewModel
    {
        public string TwitterPostLink
        {
            get
            {
                var twitterHandle = BrandService.GetTwitterHandle();
                var githubUrl = BrandService.GetGitHub();
                var text = GetLocalizedString("WhatsNewTwitPostText");
                text = string.Format(CultureInfo.InvariantCulture, text, twitterHandle);

                var encodedText = Uri.EscapeDataString(text);
                var encodedGithubUrl = Uri.EscapeDataString(githubUrl);

                return $"https://twitter.com/intent/tweet?text={encodedText}&url={encodedGithubUrl}";
            }
        }

        public ICommand OpenAllPgpKeysCommand => new RelayCommand(() => NavigationService?.NavigateToListPgpKeys());

        public bool IsPreviewAvailable { get; }

        virtual public void OnNavigatedTo(object data)
        {
        }

        virtual public void OnNavigatedFrom()
        {
        }

        protected async Task AIAgentProcessMessageAsync(LocalAIAgent agent, MessageInfo message)
        {
            if (agent is null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var text = message.HasTextBody ? message.MessageTextBody : Core.GetTextUtils().GetTextFromHtml(message.MessageHtmlBody);

            message.AIAgentProcessedBody = GetLocalizedString("ThinkingMessage");
            var thinking = true;

            message.AIAgentProcessedBody = await AIService.ProcessTextAsync
            (
                agent,
                text,
                CancellationToken.None,
                textPart => DispatcherService.RunAsync(() =>
                {
                    if (thinking)
                    {
                        message.AIAgentProcessedBody = string.Empty;
                        thinking = false;
                    }
                    message.AIAgentProcessedBody += textPart;
                })
            ).ConfigureAwait(true);

            try
            {
                await Core.UpdateMessageProcessingResultAsync(message.MessageData, message.AIAgentProcessedBody).ConfigureAwait(true);
            }
            catch (MessageIsNotExistException)
            {
                // Message is deleted
            }
        }

        public virtual Task CreateAIAgentsMenuAsync(Action<string, Action<IList<object>>> action)
        {
            return Task.CompletedTask;
        }

        public virtual Task CreateAIAgentsMenuAsync(Action<string, Action> action)
        {
            return Task.CompletedTask;
        }

        public async void ShowPreview()
        {
            try
            {
                // ToDo: Here you can add a preview of your UI controls.
                // And change the `IsPreviewAvailable` property to true.

                await MessageService.ShowInvitationDialogAsync();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
