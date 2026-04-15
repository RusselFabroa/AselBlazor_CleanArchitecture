using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities.AIModels.Ollama;


/// <summary>
/// Strongly-typed configuration for the Ollama service.
/// Bound from the "Ollama" section in appsettings.json.
/// </summary>
public class OllamaSettings
{
    public const string Section = "Ollama";

    public string BaseUrl { get; set; } = "http://10.111.3.211:8006";

    /// <summary>
    /// HTTP timeout in seconds. Should be >= 120 for CPU-only inference.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Default model to use when none is specified in the request.
    /// </summary>
    public string DefaultModel { get; set; } = "qwen3:7b";
}
