namespace SpinnerNet.App.Models;

// Supporting models for persona creation components
public class PersonaBasicInfoModel
{
    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string PreferredStyle { get; set; } = string.Empty;
}

public class GeneratedPersonaModel
{
    public string PersonalityTraits { get; set; } = string.Empty;
    public string CommunicationStyle { get; set; } = string.Empty;
    public string Capabilities { get; set; } = string.Empty;
    public bool IsValid => !string.IsNullOrEmpty(PersonalityTraits);
}

public class FinalPersonaModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(SystemPrompt);
}

public class PersonaCreationResult
{
    public bool Success { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public FinalPersonaModel PersonaData { get; set; } = new();
}

public record StyleOption(string Value, string Text);