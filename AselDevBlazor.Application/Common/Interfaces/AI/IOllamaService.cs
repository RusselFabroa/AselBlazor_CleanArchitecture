using AselDevBlazor.Domain.Entities.AIModels.Ollama;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Common.Interfaces.AI;


/// <summary>
/// Contract for communicating with a locally hosted Ollama instance.
/// Abstracts the HTTP transport so callers only deal with domain models.
///
/// Clean Architecture layer: Application (Interface)
/// Implementation lives in: Infrastructure
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Sends a chat message to Ollama and returns the model's reply.
    ///
    /// The service handles:
    ///   - Prepending the system message (if provided)
    ///   - Building the full message array from history + current message
    ///   - Returning updated history so the caller can persist it
    ///
    /// Usage — intent detection bot:
    ///   var response = await _ollamaService.ChatAsync(new OllamaChatRequest
    ///   {
    ///       Model         = "hr-intent",
    ///       SystemMessage = intentSystemPrompt,
    ///       ChatHistory   = session.History,
    ///       UserMessage   = userInput,
    ///       Temperature   = 0.1,
    ///       NumPredict    = 300,
    ///       Think         = false
    ///   });
    ///
    /// Usage — general chat bot:
    ///   var response = await _ollamaService.ChatAsync(new OllamaChatRequest
    ///   {
    ///       Model         = "qwen3:8b",
    ///       SystemMessage = "You are Kiko, reply in Taglish.",
    ///       ChatHistory   = session.History,
    ///       UserMessage   = userInput,
    ///       Temperature   = 0.6
    ///   });
    /// </summary>
    /// <param name="request">Chat request with message, history, and generation parameters.</param>
    /// <param name="cancellationToken">Cancellation token for long-running requests.</param>
    /// <returns>
    /// <see cref="OllamaChatResponse"/> with the reply and updated history.
    /// Check <see cref="OllamaChatResponse.IsSuccess"/> before using the reply.
    /// </returns>
    Task<OllamaChatResponse> ChatAsync(
        OllamaChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the Ollama server is reachable and responding.
    /// Useful for health checks and startup validation.
    /// </summary>
    /// <returns>True if Ollama is running and accessible.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}
