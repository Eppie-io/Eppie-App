using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Services
{
    /// <summary>
    /// Interface for AI service, providing methods for text translation, checking status, and managing the model.
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Asynchronously translates text to the specified language.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="language">The target language for translation.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="onTextUpdate">An optional action to be called with partial results as they are received.</param>
        /// <returns>A task representing the asynchronous operation, returning the translated text.</returns>
        Task<string> TranslateTextAsync(string text, string language, CancellationToken cancellationToken, Action<string> onTextUpdate = null);

        /// <summary>
        /// Asynchronously checks if the local AI is enabled.
        /// </summary>        
        Task<bool> IsEnabledAsync();

        /// <summary>
        /// Asynchronously deletes the local AI model.
        /// </summary>
        Task DeleteModelAsync();

        /// <summary>
        /// Asynchronously imports the local AI model.
        /// </summary>
        Task ImportModelAsync();

        /// <summary>
        /// Adds a local AI agent to the service.
        /// </summary>
        /// <param name="agent">The local AI agent to be added.</param>
        void AddAgent(LocalAIAgent agent);

        /// <summary>
        /// Remove a local AI agent from the service.
        /// </summary>
        /// <param name="agentName">The local AI agent to be removed.</param>
        void RemoveAgent(string agentName);

        /// <summary>
        /// Gets a read-only collection of local AI agents.
        /// </summary>
        /// <returns>A read-only collection of local AI agents.</returns>
        IReadOnlyList<LocalAIAgent> GetAgents();
    }
}
