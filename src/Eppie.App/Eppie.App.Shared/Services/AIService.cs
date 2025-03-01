using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eppie.AI;
using Eppie.App.UI.Tools;
using Eppie.App.ViewModels.Services;
using Tuvi.Core;
using Tuvi.Core.Entities;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Eppie.App.Shared.Services
{
    public class AIService : IAIService
    {
        private const string _LocalAIModelFolderName = "local.ai.model";
        private Service _Service;
        private List<LocalAIAgent> _Agents = new List<LocalAIAgent>();
        private ITuviMail _Core;

        public AIService(ITuviMail core)
        {
            _Core = core;
            _Core.MessagesReceived += OnMessagesReceived;
        }

        public Task<string> TranslateTextAsync(string text, string language, CancellationToken cancellationToken, Action<string> onTextUpdate = null)
        {
            if (_Service != null)
            {
                return _Service.TranslateTextAsync(text, language, cancellationToken, onTextUpdate);
            }

            return Task.FromResult(string.Empty);
        }

        public async Task<bool> IsEnabledAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.TryGetItemAsync(_LocalAIModelFolderName);

            return modelFolder != null;
        }

        public async Task LoadModelIfEnabled()
        {
            if (_Service == null && await IsEnabledAsync())
            {
                _Service = new Service();

                var localFolder = ApplicationData.Current.LocalFolder;
                var modelFolder = await localFolder.GetFolderAsync(_LocalAIModelFolderName);
                var modelPath = modelFolder.Path;

                await _Service.LoadModelAsync(modelPath);
            }
        }

        public async Task DeleteModelAsync()
        {
            _Agents.Clear();
            _Service?.UnloadModel();
            _Service = null;

            var localFolder = ApplicationData.Current.LocalFolder;
            var modelFolder = await localFolder.GetFolderAsync(_LocalAIModelFolderName);
            await modelFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
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
                StorageFolder modelFolder = await localFolder.CreateFolderAsync(_LocalAIModelFolderName, CreationCollisionOption.ReplaceExisting);

                await CopyFolderContentsAsync(selectedFolder, modelFolder);

                await LoadModelIfEnabled();
            }
        }

        public void AddAgent(LocalAIAgent agent)
        {
            _Agents.Add(agent);
        }

        public void RemoveAgent(string agentName)
        {
            _Agents.Remove(_Agents.First(x => x.Name == agentName));
        }

        public IReadOnlyList<LocalAIAgent> GetAgents()
        {
            return _Agents;
        }

        private async void OnMessagesReceived(object sender, MessagesReceivedEventArgs e)
        {
            try
            {
                var messages = e.ReceivedMessages;
                foreach (var message in messages)
                {
                    foreach (var agent in _Agents)
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
            if (agent?.Email == message.Email && message.Folder.IsInbox)
            {
                var text = message.Message.TextBody;

                if (text == null)
                {
                    text = (await _Core.GetMessageBodyAsync(message.Message).ConfigureAwait(false)).TextBody;
                }

                var translatedText = await _Service?.TranslateTextAsync(text, "Russian", CancellationToken.None);

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

            await _Core.SendMessageAsync(reply, false, false, CancellationToken.None).ConfigureAwait(false);

            await _Core.MarkMessagesAsReadAsync(new List<Message>() { message.Message }, CancellationToken.None).ConfigureAwait(false);
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
    }
}
