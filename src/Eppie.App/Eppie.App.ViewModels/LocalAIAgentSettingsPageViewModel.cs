using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuvi.App.ViewModels.Extensions;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class LocalAIAgentSettings : ObservableObject
    {
        private const string DefaultLanguage = "English";

        private int _topK = 50;
        public int TopK
        {
            get => _topK;
            set => SetProperty(ref _topK, value);
        }

        private float _topP = 0.9f;
        public float TopP
        {
            get => _topP;
            set => SetProperty(ref _topP, value);
        }

        private float _temperature = 1;
        public float Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private LocalAIAgentSpecialty _agentSpecialty;
        public LocalAIAgentSpecialty AgentSpecialty
        {
            get => _agentSpecialty;
            set
            {
                if (SetProperty(ref _agentSpecialty, value))
                {
                    UpdateSystemPrompt();
                    OnPropertyChanged(nameof(IsLanguageVisible));
                }
            }
        }

        private void UpdateSystemPrompt()
        {
            if (Prompts.TryGetValue(AgentSpecialty, out var prompt))
            {
                if (AgentSpecialty == LocalAIAgentSpecialty.Translator)
                {
                    prompt += string.Format(" You only translate into {0} language.", Language);
                }

                SystemPrompt = prompt;
            }
        }

        public List<LocalAIAgentSpecialty> Specialties { get; } = new List<LocalAIAgentSpecialty>
        {
             LocalAIAgentSpecialty.Writer,
             LocalAIAgentSpecialty.Rewriter,
             LocalAIAgentSpecialty.Proofreader,
             LocalAIAgentSpecialty.Summarizer,
             LocalAIAgentSpecialty.EmailComposer,
             LocalAIAgentSpecialty.Translator,
             LocalAIAgentSpecialty.SentimentAnalyzer,
             LocalAIAgentSpecialty.PersonalitySimulator,
             LocalAIAgentSpecialty.Researcher,
             LocalAIAgentSpecialty.Analyst,
             LocalAIAgentSpecialty.DataExtractor,
             LocalAIAgentSpecialty.NewsAggregator,
             LocalAIAgentSpecialty.Scheduler,
             LocalAIAgentSpecialty.Prioritizer,
             LocalAIAgentSpecialty.Classifier,
             LocalAIAgentSpecialty.Archivist,
             LocalAIAgentSpecialty.Jurist,
             LocalAIAgentSpecialty.ComplianceChecker,
             LocalAIAgentSpecialty.Auditor,
             LocalAIAgentSpecialty.CyberSecurity,
             LocalAIAgentSpecialty.Mediator,
             LocalAIAgentSpecialty.Negotiator,
             LocalAIAgentSpecialty.CustomerSupport,
             LocalAIAgentSpecialty.FinanceAdvisor,
             LocalAIAgentSpecialty.MarketingAdvisor,
             LocalAIAgentSpecialty.CodeReviewer,
             LocalAIAgentSpecialty.SpamFilter,
             LocalAIAgentSpecialty.WhitelistManager
        };

        public readonly Dictionary<LocalAIAgentSpecialty, string> Prompts = new Dictionary<LocalAIAgentSpecialty, string>
        {
            // Content Creation & Editing
            { LocalAIAgentSpecialty.Writer, "Write a well-structured and engaging email on the given topic." },
            { LocalAIAgentSpecialty.Rewriter, "Rephrase the given text while keeping its original meaning." },
            { LocalAIAgentSpecialty.Proofreader, "Check the given email for grammar, spelling, and punctuation errors." },
            { LocalAIAgentSpecialty.Summarizer, "Summarize the key points of the given email in a concise manner. Always begin your response with the sentence, 'Here is a summary of the email:'." },
            { LocalAIAgentSpecialty.EmailComposer, "Compose a well-structured and engaging email based on the provided context. If a received email is provided, generate an appropriate and thoughtful response that aligns with the tone and intent of the original message." },

            // Language & Communication
            { LocalAIAgentSpecialty.Translator, "You are processing incoming emails and translating their content. Your response must contain only the translated text—no explanations, comments, notes, or interpretations. The translation must be as accurate as possible, with no additions, omissions, or modifications. Preserve the exact structure of the original text, including formatting, paragraphs, lists, and punctuation. Maintain a neutral tone without rephrasing." },
            { LocalAIAgentSpecialty.SentimentAnalyzer, "Analyze the emotional tone of this email and classify it as positive, neutral, or negative." },
            { LocalAIAgentSpecialty.PersonalitySimulator, "Rewrite the email in the style of the specified person." },

            // Information Processing & Research
            { LocalAIAgentSpecialty.Researcher, "Find relevant information related to the given email topic." },
            { LocalAIAgentSpecialty.Analyst, "Analyze this email thread and provide key insights." },
            { LocalAIAgentSpecialty.DataExtractor, "Extract important details such as names, dates, and key points from this email." },
            { LocalAIAgentSpecialty.NewsAggregator, "Gather and summarize the latest relevant news related to this email topic." },

            // Organization & Workflow
            { LocalAIAgentSpecialty.Scheduler, "Suggest suitable time slots for a meeting based on the provided information." },
            { LocalAIAgentSpecialty.Prioritizer, "Identify and rank the most important emails in this inbox." },
            { LocalAIAgentSpecialty.Classifier, "Categorize this email based on its content and context." },
            { LocalAIAgentSpecialty.Archivist, "Determine if this email should be archived and tag it accordingly." },

            // Decision Making & Compliance
            { LocalAIAgentSpecialty.Jurist, "Provide legal insights related to the content of this email." },
            { LocalAIAgentSpecialty.ComplianceChecker, "Check if this email follows the required regulations and compliance standards." },
            { LocalAIAgentSpecialty.Auditor, "Review this email for any inconsistencies or suspicious elements." },
            { LocalAIAgentSpecialty.CyberSecurity, "Analyze this email for potential phishing attempts or security threats." },

            // Communication & Negotiation
            { LocalAIAgentSpecialty.Mediator, "Provide a balanced resolution strategy for the conflict described in this email thread." },
            { LocalAIAgentSpecialty.Negotiator, "Suggest effective negotiation points based on this email conversation." },

            // Customer & Business Support
            { LocalAIAgentSpecialty.CustomerSupport, "Generate an appropriate response to this customer inquiry." },
            { LocalAIAgentSpecialty.FinanceAdvisor, "Analyze financial data within this email and provide insights." },
            { LocalAIAgentSpecialty.MarketingAdvisor, "Suggest an optimized marketing strategy based on this email content." },

            // Code & Technical Review
            { LocalAIAgentSpecialty.CodeReviewer, "Review the code in this email and suggest improvements." },

            // Email Filtering & Security
            { LocalAIAgentSpecialty.SpamFilter, "Determine whether this email is spam and justify the decision." },
            { LocalAIAgentSpecialty.WhitelistManager, "Manage the whitelist of approved contacts and decide whether this sender should be added or blocked." }
        };

        private string _systemPrompt;
        public string SystemPrompt
        {
            get
            {
                if (string.IsNullOrEmpty(_systemPrompt))
                {
                    UpdateSystemPrompt();
                }

                return _systemPrompt;
            }
            set => SetProperty(ref _systemPrompt, value);
        }

        private bool _isAllowedToSendingEmails;
        public bool IsAllowedToSendingEmails
        {
            get => _isAllowedToSendingEmails;
            set => SetProperty(ref _isAllowedToSendingEmails, value);
        }

        public bool IsLanguageVisible
        {
            get { return AgentSpecialty == LocalAIAgentSpecialty.Translator; }
        }

        private string _language = DefaultLanguage;
        public string Language
        {
            get { return _language; }
            set
            {
                if (SetProperty(ref _language, value))
                {
                    UpdateSystemPrompt();
                }
            }
        }

        public readonly List<string> Languages = new List<string>
        {
            "Afrikaans",
            "Arabic",
            "Belarusian",
            "Chinese",
            "Czech",
            "Danish",
            "Dutch",
            "English",
            "Filipino",
            "Finnish",
            "French",
            "German",
            "Greek",
            "Hindi",
            "Indonesian",
            "Italian",
            "Japanese",
            "Korean",
            "Mandarin",
            "Polish",
            "Portuguese",
            "Romanian",
            "Russian",
            "Serbian",
            "Slovak",
            "Spanish",
            "Thai",
            "Turkish",
            "Ukrainian",
            "Vietnamese"
        };

        public LocalAIAgent CurrentAgent { get; }

        public LocalAIAgentSettings()
        {
            CurrentAgent = new LocalAIAgent();
        }

        public LocalAIAgentSettings(LocalAIAgent agent)
        {
            CurrentAgent = agent;

            Name = CurrentAgent.Name;
            AgentSpecialty = CurrentAgent.AgentSpecialty;
            SystemPrompt = CurrentAgent.SystemPrompt;
            IsAllowedToSendingEmails = CurrentAgent.IsAllowedToSendingEmail;
            TopK = CurrentAgent.TopK;
            TopP = CurrentAgent.TopP;
            Temperature = CurrentAgent.Temperature;

            if (AgentSpecialty == LocalAIAgentSpecialty.Translator)
            {
                Language = GetLanguage(SystemPrompt);
            }
        }

        public static LocalAIAgentSettings Create()
        {
            return new LocalAIAgentSettings();
        }

        public static LocalAIAgentSettings Create(LocalAIAgent agent)
        {
            return new LocalAIAgentSettings(agent);
        }

        internal LocalAIAgent ToAIAgent(EmailAddress linkedAccount)
        {
            CurrentAgent.Name = Name;
            CurrentAgent.AgentSpecialty = AgentSpecialty;
            CurrentAgent.SystemPrompt = SystemPrompt;
            CurrentAgent.Email = linkedAccount;
            CurrentAgent.IsAllowedToSendingEmail = IsAllowedToSendingEmails && linkedAccount != null;
            CurrentAgent.TopK = TopK;
            CurrentAgent.TopP = TopP;
            CurrentAgent.Temperature = Temperature;

            return CurrentAgent;
        }

        private string GetLanguage(string systemPrompt)
        {
            if (string.IsNullOrEmpty(systemPrompt))
            {
                return DefaultLanguage;
            }

            var words = systemPrompt.Split(' ');
            var languageIndex = Array.FindIndex(words, word => word.IndexOf("language", StringComparison.OrdinalIgnoreCase) >= 0);

            if (languageIndex > 0)
            {
                var potentialLanguage = words[languageIndex - 1];
                if (Languages.Contains(potentialLanguage, StringComparer.OrdinalIgnoreCase))
                {
                    return potentialLanguage;
                }
            }

            return DefaultLanguage;
        }
    }

    public class LocalAIAgentSettingsPageViewModel : BaseViewModel, IDisposable
    {
        private LocalAIAgentSettings _agentSettingsModel;
        public LocalAIAgentSettings AgentSettingsModel
        {
            get => _agentSettingsModel;
            set => SetProperty(ref _agentSettingsModel, value, true);
        }

        private bool _isWaitingResponse;
        public bool IsWaitingResponse
        {
            get { return _isWaitingResponse; }
            set
            {
                SetProperty(ref _isWaitingResponse, value);
                ApplySettingsCommand.NotifyCanExecuteChanged();
                RemoveAgentCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _isCreatingAgentMode = true;
        public bool IsCreatingAgentMode
        {
            get { return _isCreatingAgentMode; }
            private set { SetProperty(ref _isCreatingAgentMode, value); }
        }

        private bool _isImportAIModelButtonVisible;
        public bool IsImportAIModelButtonVisible
        {
            get => _isImportAIModelButtonVisible;
            private set => SetProperty(ref _isImportAIModelButtonVisible, value);
        }

        private bool _isDeleteAIModelButtonVisible;
        public bool IsDeleteAIModelButtonVisible
        {
            get => _isDeleteAIModelButtonVisible;
            private set => SetProperty(ref _isDeleteAIModelButtonVisible, value);
        }

        private bool _isAIProgressRingVisible;
        public bool IsAIProgressRingVisible
        {
            get { return _isAIProgressRingVisible; }
            set { SetProperty(ref _isAIProgressRingVisible, value); }
        }

        public ObservableCollection<EmailAddress> AccountsList { get; } = new ObservableCollection<EmailAddress>();

        private EmailAddress _linkedAccount;
        public EmailAddress LinkedAccount
        {
            get { return _linkedAccount; }
            set { SetProperty(ref _linkedAccount, value); }
        }

        public ObservableCollection<LocalAIAgent> AIAgentsList { get; } = new ObservableCollection<LocalAIAgent>();

        private LocalAIAgent _preprocessorAIAgent;
        public LocalAIAgent PreprocessorAIAgent
        {
            get { return _preprocessorAIAgent; }
            set { SetProperty(ref _preprocessorAIAgent, value); }
        }

        private LocalAIAgent _postprocessorAIAgent;
        public LocalAIAgent PostprocessorAIAgent
        {
            get { return _postprocessorAIAgent; }
            set { SetProperty(ref _postprocessorAIAgent, value); }
        }

        public IRelayCommand ApplySettingsCommand { get; }

        public IRelayCommand RemoveAgentCommand { get; }

        public IRelayCommand ImportLocalAIModelCommand { get; }

        public IRelayCommand DeleteLocalAIModelCommand { get; }

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public LocalAIAgentSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => !IsWaitingResponse);
            RemoveAgentCommand = new AsyncRelayCommand(RemoveAgentAndGoBackAsync, () => !IsWaitingResponse);
            ImportLocalAIModelCommand = new AsyncRelayCommand(ImportLocalAIModelAsync, () => !IsWaitingResponse);
            DeleteLocalAIModelCommand = new AsyncRelayCommand(DeleteLocalAIModelAsync, () => !IsWaitingResponse);

            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is LocalAIAgent agentData)
                {
                    InitModel(LocalAIAgentSettings.Create(agentData), false);
                }
                else
                {
                    InitModel(LocalAIAgentSettings.Create(), true);
                }

                await ToggleAIButtons();
                await UpdateEmailAccountsListAsync().ConfigureAwait(true);
                await UpdateAIAgentsListAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public async Task DeleteLocalAIModelAsync()
        {
            ShowProgressRing();
            try
            {
                await AIService.DeleteModelAsync();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                HideProgressRing();
                await ToggleAIButtons();
            }
        }

        public async Task ImportLocalAIModelAsync()
        {
            ShowProgressRing();
            try
            {
                await AIService.ImportModelAsync();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                HideProgressRing();
                await ToggleAIButtons();
            }
        }

        private void ShowProgressRing()
        {
            IsAIProgressRingVisible = true;

            IsImportAIModelButtonVisible = false;
            IsDeleteAIModelButtonVisible = false;
        }

        private void HideProgressRing()
        {
            IsAIProgressRingVisible = false;
        }

        private async Task ToggleAIButtons()
        {
            if (await AIService.IsLocalAIModelImportedAsync())
            {
                IsImportAIModelButtonVisible = false;
                IsDeleteAIModelButtonVisible = true;
            }
            else
            {
                IsImportAIModelButtonVisible = true;
                IsDeleteAIModelButtonVisible = false;
            }
        }

        private async Task UpdateEmailAccountsListAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            AccountsList.SetItems(accounts.SelectMany(account => account.Addresses));

            LinkedAccount = AgentSettingsModel.CurrentAgent.Email;
        }

        private async Task UpdateAIAgentsListAsync()
        {
            var agents = await AIService.GetAgentsAsync().ConfigureAwait(true);
            var currentAgentId = AgentSettingsModel.CurrentAgent.Id;

            if (IsCreatingAgentMode)
            {
                AIAgentsList.SetItems(agents);
            }
            else
            {
                var filteredAgents = agents.Where(agent =>
                    agent.Id != currentAgentId &&
                    agent.PreProcessorAgentId != currentAgentId &&
                    agent.PostProcessorAgentId != currentAgentId);

                AIAgentsList.SetItems(filteredAgents);
            }

            PreprocessorAIAgent = AIAgentsList.FirstOrDefault(agent => agent.Id == AgentSettingsModel.CurrentAgent.PreProcessorAgentId);
            PostprocessorAIAgent = AIAgentsList.FirstOrDefault(agent => agent.Id == AgentSettingsModel.CurrentAgent.PostProcessorAgentId);
        }

        private void InitModel(LocalAIAgentSettings accountSettingsModel, bool isCreatingMode)
        {
            IsCreatingAgentMode = isCreatingMode;
            AgentSettingsModel = accountSettingsModel;
        }

        private async Task ApplySettingsAndGoBackAsync()
        {
            IsWaitingResponse = true;
            try
            {
                var agentData = AgentSettingsModel.ToAIAgent(LinkedAccount);
                agentData.PreProcessorAgent = PreprocessorAIAgent;
                agentData.PostProcessorAgent = PostprocessorAIAgent;

                var result = await ApplyAgentSettingsAsync(agentData).ConfigureAwait(true);
                if (result)
                {
                    NavigateFromCurrentPage();
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                IsWaitingResponse = false;
            }
        }

        CancellationTokenSource _cts;

        private async Task<bool> ApplyAgentSettingsAsync(LocalAIAgent accountData)
        {
            _cts = new CancellationTokenSource();
            try
            {
                await ProcessAgentDataAsync(accountData, _cts.Token).ConfigureAwait(true);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private async Task ProcessAgentDataAsync(LocalAIAgent agent, CancellationToken cancellationToken = default)
        {
            if (IsCreatingAgentMode)
            {
                await AIService.AddAgentAsync(agent).ConfigureAwait(true);
            }
            else
            {
                await AIService.UpdateAgentAsync(agent).ConfigureAwait(true);
            }

            await BackupIfNeededAsync().ConfigureAwait(true);
        }

        private void NavigateFromCurrentPage()
        {
            if (IsCreatingAgentMode)
            {
                NavigationService?.GoBackToOrNavigate(nameof(MainPageViewModel));
            }
            else
            {
                NavigationService?.GoBackOrNavigate(nameof(MainPageViewModel));
            }
        }

        private void DoCancel()
        {
            if (_isWaitingResponse)
            {
                CancelAsyncOperation();
            }
            else
            {
                GoBack();
            }
        }

        private void GoBack()
        {
            NavigationService?.GoBack();
        }

        private void CancelAsyncOperation()
        {
            _cts?.Cancel();
        }

        private async Task RemoveAgentAndGoBackAsync()
        {
            try
            {
                IsWaitingResponse = true;

                bool isConfirmed = await MessageService.ShowRemoveAIAgentDialogAsync().ConfigureAwait(true);

                if (isConfirmed)
                {
                    await AIService.RemoveAgentAsync(AgentSettingsModel.CurrentAgent).ConfigureAwait(true);

                    await BackupIfNeededAsync().ConfigureAwait(true);

                    GoBack();
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                IsWaitingResponse = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _cts?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
