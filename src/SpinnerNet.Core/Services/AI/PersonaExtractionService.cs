using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using SpinnerNet.Shared.Models.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpinnerNet.Core.Services.AI
{
    public class PersonaExtractionService
    {
        private readonly Kernel _kernel;
        private readonly ILogger _logger;

        public PersonaExtractionService(Kernel kernel, ILogger logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<PersonaDocument> ExtractPersonaFromConversationAsync(
            string userId, 
            ConversationDocument conversation)
        {
            try
            {
                var conversationText = FormatConversationForExtraction(conversation);
                
                var extractionPrompt = @"
                Analyze the following conversation and extract key personality traits, 
                values, and preferences to create a user persona profile.

                Conversation:
                {{$conversation}}

                Based on this conversation, extract and provide in JSON format:
                {
                    ""traits"": [/* List of personality traits like ""creative"", ""analytical"", ""empathetic"" */],
                    ""values"": [/* Core values like ""family"", ""growth"", ""authenticity"" */],
                    ""communicationStyle"": /* e.g., ""direct"", ""thoughtful"", ""enthusiastic"" */,
                    ""decisionMaking"": /* e.g., ""data-driven"", ""intuitive"", ""collaborative"" */,
                    ""goals"": [/* Personal or professional goals */],
                    ""challenges"": [/* Pain points or areas for improvement */],
                    ""interests"": [/* Hobbies and topics of interest */],
                    ""primaryMotivation"": /* What drives them */,
                    ""learningStyle"": /* How they prefer to learn */
                }

                Ensure the analysis is based only on what was explicitly shared or can be reasonably inferred.
                Be specific and avoid generic descriptions.";

                var extractionFunction = _kernel.CreateFunctionFromPrompt(
                    extractionPrompt,
                    new OpenAIPromptExecutionSettings 
                    { 
                        Temperature = 0.3,
                        MaxTokens = 1000
                    });

                var result = await extractionFunction.InvokeAsync(
                    _kernel,
                    new KernelArguments { ["conversation"] = conversationText });

                var jsonResult = result.ToString();
                PersonaData? personaData = null;

                try
                {
                    jsonResult = ExtractJsonFromResponse(jsonResult);
                    personaData = JsonSerializer.Deserialize<PersonaData>(jsonResult, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse persona extraction JSON");
                    personaData = ParsePersonaManually(jsonResult);
                }

                if (personaData == null)
                {
                    _logger.LogWarning("Could not extract persona data from conversation");
                    return CreateDefaultPersona(userId, conversation.conversationId);
                }

                var personaDocument = new PersonaDocument
                {
                    Id = $"persona_{userId}_{Guid.NewGuid()}",
                    UserId = userId,
                    PersonaId = Guid.NewGuid().ToString(),
                    DisplayName = GeneratePersonaDisplayName(personaData),
                    Traits = personaData.Traits ?? new List<string>(),
                    Values = personaData.Values ?? new List<string>(),
                    CommunicationStyle = personaData.CommunicationStyle ?? "balanced",
                    DecisionMaking = personaData.DecisionMaking ?? "thoughtful",
                    Goals = personaData.Goals ?? new List<string>(),
                    Challenges = personaData.Challenges ?? new List<string>(),
                    Interests = personaData.Interests ?? new List<string>(),
                    PrimaryMotivation = personaData.PrimaryMotivation ?? "personal growth",
                    LearningStyle = personaData.LearningStyle ?? "mixed",
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    ConversationId = conversation.conversationId
                };

                return personaDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting persona from conversation");
                return CreateDefaultPersona(userId, conversation.conversationId);
            }
        }

        private string FormatConversationForExtraction(ConversationDocument conversation)
        {
            var formattedMessages = conversation.messages
                .Select(m => $"{(m.sender == MessageSender.User ? "User" : "Assistant")}: {m.content}")
                .ToList();

            return string.Join("\n", formattedMessages);
        }

        private string ExtractJsonFromResponse(string response)
        {
            var startIndex = response.IndexOf('{');
            var endIndex = response.LastIndexOf('}');
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return response.Substring(startIndex, endIndex - startIndex + 1);
            }

            return response;
        }

        private PersonaData ParsePersonaManually(string text)
        {
            try
            {
                var personaData = new PersonaData
                {
                    Traits = ExtractListFromText(text, "traits"),
                    Values = ExtractListFromText(text, "values"),
                    Goals = ExtractListFromText(text, "goals"),
                    Challenges = ExtractListFromText(text, "challenges"),
                    Interests = ExtractListFromText(text, "interests"),
                    CommunicationStyle = ExtractStringValue(text, "communicationStyle"),
                    DecisionMaking = ExtractStringValue(text, "decisionMaking"),
                    PrimaryMotivation = ExtractStringValue(text, "primaryMotivation"),
                    LearningStyle = ExtractStringValue(text, "learningStyle")
                };

                return personaData;
            }
            catch
            {
                return new PersonaData();
            }
        }

        private List<string> ExtractListFromText(string text, string fieldName)
        {
            var list = new List<string>();
            var pattern = $@"""{fieldName}"":\s*\[(.*?)\]";
            var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
            
            if (match.Success)
            {
                var content = match.Groups[1].Value;
                var items = content.Split(',')
                    .Select(item => item.Trim().Trim('"'))
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();
                list.AddRange(items);
            }

            return list;
        }

        private string ExtractStringValue(string text, string fieldName)
        {
            var pattern = $@"""{fieldName}"":\s*""([^""]+)""";
            var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
            
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private string GeneratePersonaDisplayName(PersonaData personaData)
        {
            var primaryTrait = personaData.Traits?.FirstOrDefault() ?? "Balanced";
            var primaryValue = personaData.Values?.FirstOrDefault() ?? "Growth";
            
            return $"The {primaryTrait} {primaryValue}-Seeker";
        }

        private PersonaDocument CreateDefaultPersona(string userId, string conversationId)
        {
            return new PersonaDocument
            {
                Id = $"persona_{userId}_{Guid.NewGuid()}",
                UserId = userId,
                PersonaId = Guid.NewGuid().ToString(),
                DisplayName = "Explorer",
                Traits = new List<string> { "curious", "open-minded" },
                Values = new List<string> { "growth", "learning" },
                CommunicationStyle = "balanced",
                DecisionMaking = "thoughtful",
                Goals = new List<string> { "personal development" },
                Challenges = new List<string>(),
                Interests = new List<string>(),
                PrimaryMotivation = "self-improvement",
                LearningStyle = "experiential",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                ConversationId = conversationId
            };
        }

        private class PersonaData
        {
            public List<string>? Traits { get; set; }
            public List<string>? Values { get; set; }
            public string? CommunicationStyle { get; set; }
            public string? DecisionMaking { get; set; }
            public List<string>? Goals { get; set; }
            public List<string>? Challenges { get; set; }
            public List<string>? Interests { get; set; }
            public string? PrimaryMotivation { get; set; }
            public string? LearningStyle { get; set; }
        }
    }
}