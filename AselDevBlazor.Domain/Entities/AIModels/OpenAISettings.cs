namespace AselDevBlazor.Domain.Entities.AIModels;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string SystemMessage { get; set; } = string.Empty;
}
