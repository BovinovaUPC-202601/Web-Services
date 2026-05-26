namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Configuration;

public class LocalModelSettings
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public string Model { get; set; } = "google/gemma-4-e4b";
    public string ApiKey { get; set; } = "lm-studio";
    public decimal Temperature { get; set; } = 0.2m;
}
