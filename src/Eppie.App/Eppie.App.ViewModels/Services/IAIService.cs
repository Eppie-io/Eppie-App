using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tuvi.Core.Entities;

namespace Eppie.App.ViewModels.Services
{
    /// <summary>
    /// Interface for AI service, providing methods for text processing, checking status, and managing the model.
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Event triggered when a local AI agent is added.
        /// </summary>
        event EventHandler<LocalAIAgentEventArgs> AgentAdded;
        /// <summary>
        /// Event triggered when a local AI agent is deleted.
        /// </summary>
        event EventHandler<LocalAIAgentEventArgs> AgentDeleted;
        /// <summary>
        /// Event triggered when a local AI agent is updated.
        /// </summary>
        event EventHandler<LocalAIAgentEventArgs> AgentUpdated;

        /// <summary>
        /// Asynchronously processes text with the specified local AI agent and language.
        /// </summary>
        /// <param name="agent">The local AI agent to be used for processing.</param>
        /// <param name="text">The text to be processed.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="onTextUpdate">An optional action to be called with partial results as they are received.</param>
        /// <returns>A task representing the asynchronous operation, returning the processed text.</returns>
        Task<string> ProcessTextAsync(LocalAIAgent agent, string text, CancellationToken cancellationToken, Action<string> onTextUpdate = null);

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
        Task AddAgentAsync(LocalAIAgent agent);

        /// <summary>
        /// Removes a local AI agent from the service.
        /// </summary>
        /// <param name="agent">The local AI agent to be removed.</param>
        Task RemoveAgentAsync(LocalAIAgent agent);

        /// <summary>
        /// Gets a read-only collection of local AI agents.
        /// </summary>
        /// <returns>A read-only collection of local AI agents.</returns>
        Task<IReadOnlyList<LocalAIAgent>> GetAgentsAsync();

        /// <summary>
        /// Updates a local AI agent in the service.
        /// </summary>
        /// <param name="agent">The local AI agent to be updated.</param>
        Task UpdateAgentAsync(LocalAIAgent agent);
    }
}
