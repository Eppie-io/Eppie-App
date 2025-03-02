using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eppie.App.ViewModels.Services;
using Tuvi.App.ViewModels.Extensions;
using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

namespace Tuvi.App.ViewModels
{
    public class LocalAIAgentSettings : ObservableObject
    {
        public readonly int defaultTopK = 50;
        public readonly float defaultTopP = 0.9f;
        public readonly float defaultTemperature = 1;

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
                }
            }
        }

        private void UpdateSystemPrompt()
        {
            if (Prompts.TryGetValue(AgentSpecialty, out var prompt))
            {
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
            { LocalAIAgentSpecialty.Summarizer, "Summarize the key points of the given email in a concise manner." },
            { LocalAIAgentSpecialty.EmailComposer, "Generate a professional and context-appropriate email draft." },

            // Language & Communication
            { LocalAIAgentSpecialty.Translator, "You translate only the user-provided text. Your response must contain nothing except the translated text itself. Do not add explanations, notes, interpretations, or any other content. Just translate." },
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
        }

        public static LocalAIAgentSettings Create()
        {
            return new LocalAIAgentSettings();
        }

        public static LocalAIAgentSettings Create(LocalAIAgent agent)
        {
            return new LocalAIAgentSettings(agent);
        }

        internal LocalAIAgent ToAIAgent(EmailAddress linkedAccount, string language)
        {
            var systemPrompt = SystemPrompt;
            if (AgentSpecialty == LocalAIAgentSpecialty.Translator)
            {
                systemPrompt += string.Format(" You only translate into {0} language.", language);
            }

            CurrentAgent.Name = Name;
            CurrentAgent.AgentSpecialty = AgentSpecialty;
            CurrentAgent.SystemPrompt = systemPrompt;
            CurrentAgent.Email = linkedAccount;
            CurrentAgent.IsAllowedToSendingEmail = IsAllowedToSendingEmails && linkedAccount != null;

            return CurrentAgent;
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

        public ICommand CancelSettingsCommand => new RelayCommand(DoCancel);

        public ICommand HandleErrorCommand => new RelayCommand<object>(ex => OnError(ex as Exception));

        public LocalAIAgentSettingsPageViewModel()
        {
            ApplySettingsCommand = new AsyncRelayCommand(ApplySettingsAndGoBackAsync, () => !IsWaitingResponse);
            RemoveAgentCommand = new AsyncRelayCommand(RemoveAgentAndGoBackAsync, () => !IsWaitingResponse);
            ImportLocalAIModelCommand = new AsyncRelayCommand(ImportLocalAIModelAsync, () => !IsWaitingResponse);

            ErrorsChanged += (sender, e) => ApplySettingsCommand.NotifyCanExecuteChanged();
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

            //IsEnableAIButtonVisible = false;
            //IsDisableAIButtonVisible = false;
        }

        private void HideProgressRing()
        {
            IsAIProgressRingVisible = false;
        }

        private async Task ToggleAIButtons()
        {
            //if (await AIService.IsEnabledAsync())
            //{
            //    IsEnableAIButtonVisible = false;
            //    IsDisableAIButtonVisible = true;
            //}
            //else
            //{
            //    IsEnableAIButtonVisible = true;
            //    IsDisableAIButtonVisible = false;
            //}
        }

        private async Task UpdateEmailAccountsListAsync()
        {
            var accounts = await Core.GetCompositeAccountsAsync().ConfigureAwait(true);
            AccountsList.SetItems(accounts.SelectMany(account => account.Addresses));
            OnPropertyChanged(nameof(LinkedAccount));
        }

        private async Task UpdateAIAgentsListAsync()
        {
            var agents = await AIService.GetAgentsAsync().ConfigureAwait(true);
            AIAgentsList.SetItems(agents.Where(agent => agent.Id != AgentSettingsModel.CurrentAgent.Id));

            OnPropertyChanged(nameof(PreprocessorAIAgent));
            OnPropertyChanged(nameof(PostprocessorAIAgent));
        }

        public override async void OnNavigatedTo(object data)
        {
            try
            {
                if (data is LocalAIAgent agentData)
                {
                    InitModel(LocalAIAgentSettings.Create(agentData), false);
                    LinkedAccount = agentData.Email;
                }
                else
                {
                    InitModel(LocalAIAgentSettings.Create(), true);
                }

                await UpdateEmailAccountsListAsync().ConfigureAwait(true);
                await UpdateAIAgentsListAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                OnError(e);
            }
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
                var agentData = AgentSettingsModel.ToAIAgent(LinkedAccount, LocalSettingsService.Language);
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
                    //await DeleteLocalAIModelAsync().ConfigureAwait(true);
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
