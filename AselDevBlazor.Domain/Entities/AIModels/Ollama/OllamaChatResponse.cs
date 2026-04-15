using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities.AIModels.Ollama;


public class OllamaChatResponse
{
 
    public bool IsSuccess { get; set; }

    public string Reply { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public List<OllamaChatMessage> UpdatedHistory { get; set; } = new();
}
