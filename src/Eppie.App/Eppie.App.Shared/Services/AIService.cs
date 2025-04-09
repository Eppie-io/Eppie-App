// ---------------------------------------------------------------------------- //
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if AI_ENABLED
using Eppie.AI;
using Microsoft.Extensions.AI;
#endif
using Eppie.App.UI.Tools;
using Eppie.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.Core.DataStorage;
using Tuvi.Core.Entities;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Eppie.App.Shared.Services
{
    public class AIService : IAIService
    {
#if AI_ENABLED
        private Service Service;
#endif

        private const string LocalAIModelFolderName = "local.ai.model";
        private ITuviMail Core;
        private readonly IAIAgentsStorage Storage;
        private Task LoadingModelTask;
        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        public event EventHandler<LocalAIAgentEventArgs> AgentAdded;
        public event EventHandler<LocalAIAgentEventArgs> AgentDeleted;
        public event EventHandler<LocalAIAgentEventArgs> AgentUpdated;
        public event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        public AIService(ITuviMail core)
        {
            Core = core;
            Core.MessagesReceived += OnMessagesReceived;

            Storage = Core.GetAIAgentsStorage();
            LoadingModelTask = LoadModelAsync();
        }
#if AI_ENABLED

        public async Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
            var result = string.Empty;

            if (Service != null && !string.IsNullOrEmpty(text))
            {
                await Semaphore.WaitAsync();

                try
                {
                    if (agent.PreProcessorAgent != null)
                    {
                        text = await Service.ProcessTextAsync
                            (
                                agent.PreProcessorAgent.SystemPrompt,
                                text,
                                GetAgentOptions(agent.PreProcessorAgent),
                                cancellationToken,
                                onTextUpdate
                            ).ConfigureAwait(false);
                    }

                    result = await Service.ProcessTextAsync
                        (
                            agent.SystemPrompt,
                            text,
                            GetAgentOptions(agent),
                            cancellationToken,
                            onTextUpdate
                        ).ConfigureAwait(false);

                    if (agent.PostProcessorAgent != null)
                    {
                        result = await Service.ProcessTextAsync
                            (
                                agent.PostProcessorAgent.SystemPrompt,
                                result,
                                GetAgentOptions(agent.PostProcessorAgent),
                                cancellationToken,
                                onTextUpdate
                            ).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }

            return result;
        }

        private static ChatOptions GetAgentOptions(LocalAIAgent agent)
        {
            const string minLenght = "min_length";
            const string doSample = "do_sample";

            return new ChatOptions
            {
                TopP = agent.TopP,
                TopK = agent.TopK,
                Temperature = agent.Temperature,
                AdditionalProperties = new AdditionalPropertiesDictionary
                {
                    { minLenght, 0 },
                    { doSample, agent.DoSample },
                }
            };
        }
#else
        public Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
            return Task.FromResult(string.Empty);
        }
#endif

#if AI_ENABLED
        public bool IsAvailable()
        {
            return Environment.ProcessorCount > 1;
        }
#else
        public bool IsAvailable()
        {
            return false;
        }
#endif

#if AI_ENABLED
        public async Task<bool> IsEnabledAsync()
        {
            var agents = await Storage.GetAIAgentsAsync().ConfigureAwait(false);
            return agents.Count > 0 && await IsLocalAIModelImportedAsync().ConfigureAwait(false) && LoadingModelTask.IsCompleted;
        }
#else
        public Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(false);
        }
#endif

        public async Task<bool> IsLocalAIModelImportedAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.TryGetItemAsync(LocalAIModelFolderName);

            return modelFolder != null;
        }

#if AI_ENABLED
        private async Task LoadModelAsync()
        {
            try
            {
                if (Service is null && await IsLocalAIModelImportedAsync().ConfigureAwait(false))
                {
                    var service = new Service();

                    var localFolder = ApplicationData.Current.LocalFolder;
                    var modelRootFolder = await localFolder.GetFolderAsync(LocalAIModelFolderName);
                    var modelFolder = (await modelRootFolder.GetFoldersAsync()).FirstOrDefault();
                    var modelPath = modelFolder.Path;

                    await service.LoadModelAsync(modelPath).ConfigureAwait(false);
                    Service = service;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
#else
        private Task LoadModelAsync()
        {
            return Task.CompletedTask;
        }
#endif

        public async Task DeleteModelAsync()
        {
#if AI_ENABLED
            Service?.UnloadModel();
            Service = null;
#endif
            var localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                var modelFolder = await localFolder.GetFolderAsync(LocalAIModelFolderName);
                await modelFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (FileNotFoundException)
            {
            }
        }

#if AI_ENABLED
        public async Task ImportModelAsync()
        {
            await DeleteModelAsync();

            FolderPicker folderPicker = FolderPickerBuilder.CreateBuilder(App.MainWindow)
                                                           .Configure((picker) =>
                                                           {
                                                               picker.SuggestedStartLocation = PickerLocationId.Desktop;
                                                               picker.FileTypeFilter.Add("*");
                                                           })
                                                           .Build();

            StorageFolder selectedFolder = await folderPicker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var modelRootFolder = await localFolder.CreateFolderAsync(LocalAIModelFolderName, CreationCollisionOption.ReplaceExisting);
                var modelFolder = await modelRootFolder.CreateFolderAsync(Service.GetModelName(selectedFolder.Path), CreationCollisionOption.ReplaceExisting);

                await CopyFolderContentsAsync(selectedFolder, modelFolder).ConfigureAwait(false);

                await LoadModelAsync().ConfigureAwait(false);
            }
        }
#else
        public Task ImportModelAsync()
        {
            return Task.CompletedTask;
        }
#endif

        public async Task AddAgentAsync(LocalAIAgent agent)
        {
            await Storage.AddAIAgentAsync(agent).ConfigureAwait(false);
            AgentAdded?.Invoke(this, new LocalAIAgentEventArgs(agent));
        }

        public async Task RemoveAgentAsync(LocalAIAgent agent)
        {
            await Storage.DeleteAIAgentAsync(agent.Id).ConfigureAwait(false);
            AgentDeleted?.Invoke(this, new LocalAIAgentEventArgs(agent));
        }

        public Task<IReadOnlyList<LocalAIAgent>> GetAgentsAsync()
        {
            return Storage.GetAIAgentsAsync();
        }

        private async void OnMessagesReceived(object sender, MessagesReceivedEventArgs e)
        {
            try
            {
                var messages = e.ReceivedMessages;
                var agents = await Storage.GetAIAgentsAsync().ConfigureAwait(false);
                var tasks = messages.SelectMany(message => agents.Select(agent => ProcessMessage(agent, message))).ToList();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private async Task ProcessMessage(LocalAIAgent agent, ReceivedMessageInfo message)
        {
            if (agent?.Email == message.Email && message.Folder.IsInbox && !message.Message.From.Any(x => x == agent?.Email))
            {
                var text = message.Message.TextBody;

                if (text is null)
                {
                    text = (await Core.GetMessageBodyAsync(message.Message).ConfigureAwait(false)).TextBody;
                }

                var result = await ProcessTextAsync(agent, text, CancellationToken.None).ConfigureAwait(false);

                try
                {
                    await Core.UpdateMessageProcessingResultAsync(message.Message, result).ConfigureAwait(false);
                }
                catch (MessageIsNotExistException)
                {
                    // Message is deleted
                }

                if (agent.IsAllowedToSendingEmail)
                {
                    await ReplyToMessage(message, result).ConfigureAwait(false);
                }
            }
        }

        private async Task ReplyToMessage(ReceivedMessageInfo message, string text)
        {
            var reply = new Message();

            reply.From.Add(message.Email);
            reply.TextBody = text;
            reply.To.AddRange(message.Message.From);
            reply.Subject = "RE(AI): " + message.Message.Subject;

            await Core.SendMessageAsync(reply, false, false, CancellationToken.None).ConfigureAwait(false);

            await Core.MarkMessagesAsReadAsync(new List<Message>() { message.Message }, CancellationToken.None).ConfigureAwait(false);
        }

        private void OnError(Exception ex)
        {
            ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(ex));
        }

        private async Task CopyFolderContentsAsync(StorageFolder sourceFolder, StorageFolder destinationFolder)
        {
            IReadOnlyList<StorageFile> files = await sourceFolder.GetFilesAsync();
            var config = await sourceFolder.TryGetItemAsync("genai_config.json");

            if (files.Count > 0 && config != null)
            {
                foreach (StorageFile file in files)
                {
                    await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting);
                }
            }
            else
            {
                await destinationFolder.DeleteAsync();

                throw new Exception("No AI model files found in the selected folder.");
            }
        }

        public async Task UpdateAgentAsync(LocalAIAgent agent)
        {
            await Storage.UpdateAIAgentAsync(agent).ConfigureAwait(false);
            AgentUpdated?.Invoke(this, new LocalAIAgentEventArgs(agent));
        }
    }
}
