using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tuvi.App.ViewModels
{
    public class LocalAIAgent
    {

    }

    public enum LocalAIAgentSpecialty
    {
        // Content Creation & Editing
        Writer,             // Creates original content
        Rewriter,           // Rewrites and reformulates text
        Proofreader,        // Checks grammar and spelling
        Summarizer,         // Summarizes emails and documents
        EmailComposer,      // Generates email drafts

        // Language & Communication
        Translator,         // Translates emails between languages
        SentimentAnalyzer,  // Analyzes emotional tone of emails
        PersonalitySimulator, // Simulates writing style of a person

        // Information Processing & Research
        Researcher,         // Finds relevant information
        Analyst,            // Analyzes trends and extracts insights
        DataExtractor,      // Extracts key data from emails and attachments
        NewsAggregator,     // Gathers and summarizes news

        // Organization & Workflow
        Scheduler,          // Plans meetings and sets reminders
        Prioritizer,        // Identifies and ranks important emails
        Classifier,         // Sorts emails into categories
        Archivist,          // Saves and retrieves important documents

        // Decision Making & Compliance
        Jurist,             // Provides legal assistance
        ComplianceChecker,  // Ensures regulatory compliance
        Auditor,            // Verifies data consistency and detects anomalies
        CyberSecurity,      // Detects phishing attempts and ensures data security

        // Communication & Negotiation
        Mediator,           // Facilitates conflict resolution
        Negotiator,         // Assists in negotiations and argumentation

        // Customer & Business Support
        CustomerSupport,    // Handles automated responses and inquiries
        FinanceAdvisor,     // Analyzes financial transactions and expenses
        MarketingAdvisor,   // Provides marketing strategies and A/B testing insights

        // Code & Technical Review
        CodeReviewer,       // Analyzes and suggests improvements to code snippets

        // Email Filtering & Security
        SpamFilter,         // Detects and filters out spam emails
        WhitelistManager    // Manages a whitelist of contacts allowed to send emails
    }

    public enum LanguageModelSkill
    {
        General,
        TextToTable,
        Summarize,
        Rewrite
    }

    public enum SeverityLevel
    {
        None,
        Low,
        Medium,
        High
    }

    public class LocalAIAgentSettings : ObservableObject
    {
        public readonly int defaultTopK = 50;
        public readonly float defaultTopP = 0.9f;
        public readonly float defaultTemperature = 1;
        public readonly int defaultMaxLength = 1024;
        public readonly bool defaultDoSample = true;
        public readonly LanguageModelSkill defaultSkill = LanguageModelSkill.General;
        public readonly SeverityLevel defaultSeverityLevel = SeverityLevel.None;

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
            { LocalAIAgentSpecialty.Translator, "Translate the given email while preserving tone and intent." },
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

        public LanguageModelSkill LanguageModelSkill { get; set; } = LanguageModelSkill.General;

        public List<LanguageModelSkill> LanguageModelSkills { get; } = new List<LanguageModelSkill> { LanguageModelSkill.General, LanguageModelSkill.TextToTable, LanguageModelSkill.Summarize, LanguageModelSkill.Rewrite };

        public SeverityLevel InputModerationLevel { get; set; } = SeverityLevel.None;

        public SeverityLevel OutputModerationLevel { get; set; } = SeverityLevel.None;

        public List<SeverityLevel> SeverityLevels { get; } = new List<SeverityLevel> { SeverityLevel.None, SeverityLevel.Low, SeverityLevel.Medium, SeverityLevel.High };

        public bool IsPhiSilica { get; set; } = true;

        public LocalAIAgentSettings()
        {
        }

        public static LocalAIAgentSettings Create()
        {
            return new LocalAIAgentSettings();
        }

        internal LocalAIAgent ToAgent()
        {
            throw new NotImplementedException();
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

        public override void OnNavigatedTo(object data)
        {
            try
            {
                if (data is LocalAIAgent agentData)
                {
                    InitModel(LocalAIAgentSettings.Create(), false);
                }
                else
                {
                    InitModel(LocalAIAgentSettings.Create(), false);
                }
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
                var agentData = AgentSettingsModel.ToAgent();
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
            //bool existAccount = await Core.ExistsAccountWithEmailAddressAsync(account.Email, cancellationToken).ConfigureAwait(true);

            //if (!existAccount)
            //{
            //    await Core.AddAccountAsync(account, cancellationToken).ConfigureAwait(true);
            //}
            //else
            //{
            //    await Core.UpdateAccountAsync(account, cancellationToken).ConfigureAwait(true);
            //}

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
                    await DeleteLocalAIModelAsync().ConfigureAwait(true);

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
