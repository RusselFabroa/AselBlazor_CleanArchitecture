using AselDevBlazor.Application.Common.Interfaces.AI;
using AselDevBlazor.Domain.Entities.AIModels.Ollama;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Features.AI;

/// <summary>
/// Communicates with a locally hosted Ollama instance via its REST API.
///
/// Clean Architecture layer: Infrastructure
/// Registered in: DependencyInjection.cs (Infrastructure)
/// Config section: "Ollama" in appsettings.json
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _http;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public OllamaService(
        HttpClient http,
        IOptions<OllamaSettings> settings,
        ILogger<OllamaService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<OllamaChatResponse> ChatAsync(
        OllamaChatRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Build the full message list sent to Ollama
            //    Order: [system?] + [history] + [current user message]
            var messages = BuildMessages(request);

            // 2. Build the Ollama API payload
            var payload = BuildPayload(request, messages);

            // 3. Serialize and POST
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending to Ollama [{Model}] — {MessageCount} messages",
                request.Model, messages.Count);

            var httpResponse = await _http.PostAsync(
                "/api/chat", httpContent, cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            // 4. Parse the response
            var raw = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var reply = ParseReply(raw);
            var model = ParseModel(raw) ?? request.Model;

            _logger.LogDebug("Ollama replied [{Model}]: {Preview}",
                model, reply.Length > 80 ? reply[..80] + "…" : reply);

            // 5. Build updated history (caller stores this for next turn)
            var updatedHistory = BuildUpdatedHistory(request, reply);

            return new OllamaChatResponse
            {
                IsSuccess = true,
                Reply = reply,
                Model = model,
                UpdatedHistory = updatedHistory
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ollama HTTP error for model [{Model}]", request.Model);
            return Fail($"Could not reach Ollama server: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Ollama request timed out for model [{Model}]", request.Model);
            return Fail("Request timed out. The model may be loading or the response is too long.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama [{Model}]", request.Model);
            return Fail($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.GetAsync("/", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds the ordered message list: system → history → current user message.
    /// </summary>
    private static List<MessageDto> BuildMessages(OllamaChatRequest request)
    {
        var messages = new List<MessageDto>();

        // Prepend system message if provided
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            messages.Add(new MessageDto("system", request.SystemMessage));
        }

        // Append conversation history
        foreach (var h in request.ChatHistory)
        {
            messages.Add(new MessageDto(h.Role, h.Content));
        }

        // Append the current user message
        messages.Add(new MessageDto("user", request.UserMessage));

        return messages;
    }

    /// <summary>
    /// Builds the raw Ollama API payload object.
    /// </summary>
    private static object BuildPayload(OllamaChatRequest request, List<MessageDto> messages)
    {
        return new
        {
            model = request.Model,
            messages = messages,
            stream = false,
            think = request.Think,
            options = new
            {
                temperature = request.Temperature,
                top_k = request.TopK,
                top_p = request.TopP,
                repeat_penalty = request.RepeatPenalty,
                num_predict = request.NumPredict,
                num_ctx = request.NumCtx,
                stop = new[] { "<|im_start|>", "<|im_end|>" }
            }
        };
    }

    /// <summary>
    /// Parses the reply text from the Ollama JSON response.
    /// </summary>
    private static string ParseReply(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        return doc.RootElement
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()
                  ?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Parses the model name from the Ollama JSON response.
    /// </summary>
    private static string? ParseModel(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            return doc.RootElement.TryGetProperty("model", out var m)
                ? m.GetString()
                : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Returns the updated chat history ready for the next call.
    /// Appends the current user message and the assistant reply.
    /// The system message is NOT included in history — it is always
    /// re-injected at the start of the next call via SystemMessage.
    /// </summary>
    private static List<OllamaChatMessage> BuildUpdatedHistory(
        OllamaChatRequest request, string reply)
    {
        var history = new List<OllamaChatMessage>(request.ChatHistory)
        {
            new() { Role = "user",      Content = request.UserMessage },
            new() { Role = "assistant", Content = reply               }
        };
        return history;
    }

    /// <summary>
    /// Creates a failed response with an error message.
    /// </summary>
    private static OllamaChatResponse Fail(string message) => new()
    {
        IsSuccess = false,
        Reply = string.Empty,
        ErrorMessage = message,
        UpdatedHistory = new()
    };

    // ── Internal DTOs (only used for serialization) ───────────────────────────

    private record MessageDto(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);


    public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(raw);

            var models = doc.RootElement
                            .GetProperty("models")
                            .EnumerateArray()
                            .Select(m => m.GetProperty("name").GetString() ?? string.Empty)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList();

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch available models from Ollama");
            return new List<string>();
        }
    }
}
