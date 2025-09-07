using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace SpinnerNet.Core.Services.AI
{
    public class PersonaChatService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly ICosmosRepository<ConversationDocument> _conversationRepo;
        private readonly ICosmosRepository<PersonaDocument> _personaRepo;
        private readonly ILogger<PersonaChatService> _logger;
        private readonly IConfiguration _configuration;

        public PersonaChatService(
            IConfiguration configuration,
            ICosmosRepository<ConversationDocument> conversationRepo,
            ICosmosRepository<PersonaDocument> personaRepo,
            ILogger<PersonaChatService> logger)
        {
            _configuration = configuration;
            _conversationRepo = conversationRepo;
            _personaRepo = personaRepo;
            _logger = logger;

            var keyVaultName = configuration["Azure:KeyVault:Name"];
            if (string.IsNullOrEmpty(keyVaultName))
            {
                _logger.LogWarning("Azure KeyVault name not configured. Using environment variable for OpenAI key.");
                var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
                    configuration["OpenAI:ApiKey"] ?? 
                    throw new InvalidOperationException("OpenAI API key not configured");

                var builder = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion(
                        modelId: configuration["OpenAI:Model"] ?? "gpt-4",
                        apiKey: openAiKey);

                _kernel = builder.Build();
            }
            else
            {
                var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
                var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                
                try
                {
                    var openAiKey = secretClient.GetSecret("OpenAI-ApiKey").Value.Value;
                    
                    var builder = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            modelId: configuration["OpenAI:Model"] ?? "gpt-4",
                            apiKey: openAiKey);
                    
                    _kernel = builder.Build();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve OpenAI key from KeyVault");
                    throw;
                }
            }

            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
        {
            try
            {
                var conversation = await GetOrCreateConversationAsync(userId);
                
                conversation.messages.Add(new ConversationMessage 
                { 
                    messageId = Guid.NewGuid().ToString(),
                    sender = MessageSender.User,
                    content = message,
                    timestamp = DateTime.UtcNow
                });

                var systemPrompt = CreatePersonaDiscoveryPrompt(conversation);
                
                var chatHistory = new ChatHistory(systemPrompt);
                
                foreach (var msg in conversation.messages)
                {
                    if (msg.sender == MessageSender.User)
                        chatHistory.AddUserMessage(msg.content);
                    else if (msg.sender == MessageSender.Buddy)
                        chatHistory.AddAssistantMessage(msg.content);
                }

                var executionSettings = new OpenAIPromptExecutionSettings 
                {
                    Temperature = double.TryParse(_configuration["OpenAI:Temperature"], out var temp) ? temp : 0.7,
                    MaxTokens = int.TryParse(_configuration["OpenAI:MaxTokens"], out var max) ? max : 500
                };

                var response = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings);

                conversation.messages.Add(new ConversationMessage 
                { 
                    messageId = Guid.NewGuid().ToString(),
                    sender = MessageSender.Buddy,
                    content = response.Content ?? string.Empty,
                    timestamp = DateTime.UtcNow
                });

                conversation.lastMessageAt = DateTime.UtcNow;
                conversation.messageCount = conversation.messages.Count;

                var progress = CalculateProgress(conversation);
                conversation.personaCompletionPercentage = progress;

                if (conversation.messages.Count >= 10 && progress >= 80)
                {
                    await ExtractAndSavePersonaAsync(userId, conversation);
                }

                await _conversationRepo.CreateOrUpdateAsync(conversation, conversation.UserId);

                return new ChatResponse 
                {
                    Message = response.Content ?? string.Empty,
                    PersonaProgress = progress,
                    ConversationId = conversation.conversationId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message for user {UserId}", userId);
                throw;
            }
        }

        private async Task<ConversationDocument> GetOrCreateConversationAsync(string userId)
        {
            var existingConversations = await _conversationRepo.QueryAsync(
                c => c.UserId == userId && 
                     c.purpose == "persona_discovery" && 
                     c.isActive == true,
                userId);

            var conversation = existingConversations.FirstOrDefault();

            if (conversation == null)
            {
                conversation = new ConversationDocument
                {
                    id = $"conversation_{Guid.NewGuid()}",
                    conversationId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    purpose = "persona_discovery",
                    startedAt = DateTime.UtcNow,
                    isActive = true,
                    messages = new List<ConversationMessage>()
                };

                await _conversationRepo.CreateOrUpdateAsync(conversation, userId);
            }

            return conversation;
        }

        private string CreatePersonaDiscoveryPrompt(ConversationDocument conversation)
        {
            var messageCount = conversation.messages.Count;
            var stage = GetConversationStage(messageCount);

            return stage switch
            {
                "initial" => @"You are a friendly AI buddy helping a user discover their unique digital persona. 
                    Your goal is to have a natural, engaging conversation while learning about their personality, 
                    values, goals, and preferences. Start with a warm greeting and ask what brings them here today.
                    Keep responses conversational, warm, and encouraging. Show genuine interest in their responses.",

                "exploration" => @"You are continuing to help the user discover their persona. 
                    Based on what they've shared so far, ask deeper questions about their values, 
                    decision-making style, and what matters most to them. 
                    Make connections between their previous responses and validate their feelings.",

                "interests" => @"Now explore the user's interests, hobbies, and passions. 
                    Ask about what they enjoy doing in their free time, what topics excite them, 
                    and what they could talk about for hours. Be enthusiastic about their interests.",

                "challenges" => @"Gently explore the challenges they're facing and areas where they'd like to grow. 
                    Be supportive and understanding. Focus on their aspirations and how they want to improve.",

                "synthesis" => @"You're nearing the end of the persona discovery. 
                    Start synthesizing what you've learned about them. 
                    Reflect back their key traits, values, and goals to ensure understanding. 
                    Ask if there's anything else important they'd like to share.",

                _ => @"Continue the persona discovery conversation naturally. 
                    Build on previous responses and help the user express their authentic self."
            };
        }

        private string GetConversationStage(int messageCount)
        {
            return messageCount switch
            {
                0 or 1 => "initial",
                >= 2 and < 5 => "exploration",
                >= 5 and < 8 => "interests",
                >= 8 and < 11 => "challenges",
                >= 11 => "synthesis",
                _ => "exploration"
            };
        }

        private double CalculateProgress(ConversationDocument conversation)
        {
            var messageCount = conversation.messages.Count;
            var hasValues = conversation.messages.Any(m => 
                m.content.ToLower().Contains("value") || 
                m.content.ToLower().Contains("important") ||
                m.content.ToLower().Contains("matter"));
            
            var hasGoals = conversation.messages.Any(m => 
                m.content.ToLower().Contains("goal") || 
                m.content.ToLower().Contains("achieve") ||
                m.content.ToLower().Contains("aspir"));
            
            var hasInterests = conversation.messages.Any(m => 
                m.content.ToLower().Contains("enjoy") || 
                m.content.ToLower().Contains("hobby") ||
                m.content.ToLower().Contains("interest"));

            var baseProgress = Math.Min(messageCount * 5, 50);
            var contentProgress = 0.0;

            if (hasValues) contentProgress += 15;
            if (hasGoals) contentProgress += 15;
            if (hasInterests) contentProgress += 20;

            return Math.Min(baseProgress + contentProgress, 100);
        }

        private async Task ExtractAndSavePersonaAsync(string userId, ConversationDocument conversation)
        {
            try
            {
                var extractionService = new PersonaExtractionService(_kernel, _logger);
                var extractedPersona = await extractionService.ExtractPersonaFromConversationAsync(userId, conversation);
                
                if (extractedPersona != null)
                {
                    conversation.personaExtraction = new PersonaExtractionData
                    {
                        traits = extractedPersona.Traits ?? new List<string>(),
                        values = extractedPersona.Values ?? new List<string>(),
                        communicationStyle = extractedPersona.CommunicationStyle ?? string.Empty,
                        decisionMaking = extractedPersona.DecisionMaking ?? string.Empty,
                        goals = extractedPersona.Goals ?? new List<string>(),
                        challenges = extractedPersona.Challenges ?? new List<string>(),
                        interests = extractedPersona.Interests ?? new List<string>(),
                        confidenceScore = 0.85,
                        extractedAt = DateTime.UtcNow
                    };

                    await _personaRepo.CreateOrUpdateAsync(extractedPersona, extractedPersona.UserId);
                    _logger.LogInformation("Persona extracted and saved for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract persona for user {UserId}", userId);
            }
        }
    }

    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public double PersonaProgress { get; set; }
        public string ConversationId { get; set; } = string.Empty;
    }
}