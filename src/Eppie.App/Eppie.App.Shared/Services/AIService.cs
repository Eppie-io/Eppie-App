using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eppie.AI;
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
        private Service Service;
        private ITuviMail Core;
        private readonly IAIAgentsStorage Storage;

        public event EventHandler<LocalAIAgentEventArgs> AgentAdded;
        public event EventHandler<LocalAIAgentEventArgs> AgentDeleted;
        public event EventHandler<LocalAIAgentEventArgs> AgentUpdated;

        public AIService(ITuviMail core)
        {
            Core = core;
            Core.MessagesReceived += OnMessagesReceived;

            Storage = Core.GetAIAgentsStorage();
        }

        public Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
            if (Service != null)
            {
                return Service.ProcessTextAsync(agent.SystemPrompt, text, cancellationToken, onTextUpdate);
            }

            return Task.FromResult(string.Empty);
        }

        public async Task<bool> IsEnabledAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.TryGetItemAsync(LocalAIModelFolderName);

            return modelFolder != null;
        }

        public async Task LoadModelIfEnabled()
        {
            if (Service is null && await IsEnabledAsync())
            {
                Service = new Service();

                var localFolder = ApplicationData.Current.LocalFolder;
                var modelFolder = await localFolder.GetFolderAsync(LocalAIModelFolderName);
                var modelPath = modelFolder.Path;

                await Service.LoadModelAsync(modelPath);
            }
        }

        public async Task DeleteModelAsync()
        {
            await DeleteAllAgents();

            Service?.UnloadModel();
            Service = null;

            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.GetFolderAsync(LocalAIModelFolderName);
            await modelFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private async Task DeleteAllAgents()
        {
            var agents = await Storage.GetAIAgentsAsync();
            foreach (var agent in agents)
            {
                await Storage.DeleteAIAgentAsync(agent.Id);
                AgentDeleted?.Invoke(this, new LocalAIAgentEventArgs(agent));
            }
        }

        public async Task ImportModelAsync()
        {
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

                await LoadModelIfEnabled();
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

                if (text == null)
                {
                    text = (await Core.GetMessageBodyAsync(message.Message).ConfigureAwait(false)).TextBody;
                }

                var translatedText = await Service?.ProcessTextAsync(agent.SystemPrompt, text, CancellationToken.None);

                if (agent.IsAllowedToSendingEmail)
                {
                    await ReplyToMessage(message, translatedText).ConfigureAwait(false);
                }
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
