using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if AI_ENABLED
using Eppie.AI;
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
        private const string LocalAIModelFolderName = "local.ai.model";
#if AI_ENABLED
        private Service Service;
#endif
        private ITuviMail Core;
        private readonly IAIAgentsStorage Storage;
        private Task LoadingModelTask;

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

        public Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
#if AI_ENABLED
            if (Service != null)
            {
                return Service.ProcessTextAsync(agent.SystemPrompt, text, cancellationToken, onTextUpdate);
            }
#endif
            return Task.FromResult(string.Empty);
        }

#if AI_ENABLED
        public async Task<bool> IsEnabledAsync()
        {
            var agents = await Storage.GetAIAgentsAsync();
            return agents.Count > 0 && await IsLocalAIModelImportedAsync() && LoadingModelTask.IsCompleted;
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
                if (Service is null && await IsLocalAIModelImportedAsync())
                {
                    Service = new Service();

                    var localFolder = ApplicationData.Current.LocalFolder;
                    var modelFolder = await localFolder.GetFolderAsync(LocalAIModelFolderName);
                    var modelPath = modelFolder.Path;

                    await Service.LoadModelAsync(modelPath);
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
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder modelFolder = await localFolder.CreateFolderAsync(LocalAIModelFolderName, CreationCollisionOption.ReplaceExisting);

                await CopyFolderContentsAsync(selectedFolder, modelFolder);

                await LoadModelAsync();
            }
        }

        public async Task AddAgentAsync(LocalAIAgent agent)
        {
            await Storage.AddAIAgentAsync(agent);
            AgentAdded?.Invoke(this, new LocalAIAgentEventArgs(agent));
        }

        public async Task RemoveAgentAsync(LocalAIAgent agent)
        {
            await Storage.DeleteAIAgentAsync(agent.Id);
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
                var agents = await Storage.GetAIAgentsAsync();
                foreach (var message in messages)
                {
                    foreach (var agent in agents)
                    {
                        await ProcessMessage(agent, message).ConfigureAwait(false);
                    }
                }
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
#if AI_ENABLED
                var translatedText = await Service?.ProcessTextAsync(agent.SystemPrompt, text, CancellationToken.None);

                if (agent.IsAllowedToSendingEmail)
                {
                    await ReplyToMessage(message, translatedText).ConfigureAwait(false);
                }
#endif
            }
        }

        private async Task ReplyToMessage(ReceivedMessageInfo message, string translatedText)
        {
            var reply = new Message();

            reply.From.Add(message.Email);
            reply.TextBody = translatedText;
            reply.To.AddRange(message.Message.From);
            reply.Subject = message.Message.Subject;

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
            await Storage.UpdateAIAgentAsync(agent);
            AgentUpdated?.Invoke(this, new LocalAIAgentEventArgs(agent));
        }
    }
}
