using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;
using System.Text;

namespace SpinnerNet.Core.Features.Buddies;

/// <summary>
/// Vertical slice for chatting with AI buddy companions
/// Handles real-time conversation with personality-aware AI assistants
/// </summary>
public static class ChatWithBuddy
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("buddyId")]
        public string BuddyId { get; init; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("conversationId")]
        public string? ConversationId { get; init; }

        [JsonPropertyName("messageType")]
        public MessageType MessageType { get; init; } = MessageType.Text;

        [JsonPropertyName("context")]
        public ChatContext? Context { get; init; }

        [JsonPropertyName("streamResponse")]
        public bool StreamResponse { get; init; } = false;
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("conversationId")]
        public string? ConversationId { get; init; }

        [JsonPropertyName("messageId")]
        public string? MessageId { get; init; }

        [JsonPropertyName("buddyResponse")]
        public BuddyResponse? BuddyResponse { get; init; }

        [JsonPropertyName("conversationState")]
        public ConversationState? ConversationState { get; init; }

        [JsonPropertyName("suggestedActions")]
        public List<SuggestedChatAction> SuggestedActions { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string conversationId, string messageId, BuddyResponse response, ConversationState state, List<SuggestedChatAction> actions) =>
            new() 
            { 
                Success = true, 
                ConversationId = conversationId,
                MessageId = messageId,
                BuddyResponse = response,
                ConversationState = state,
                SuggestedActions = actions
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public record ChatContext
    {
        [JsonPropertyName("currentLocation")]
        public string? CurrentLocation { get; init; }

        [JsonPropertyName("timeOfDay")]
        public string? TimeOfDay { get; init; }

        [JsonPropertyName("recentTasks")]
        public List<string> RecentTasks { get; init; } = new();

        [JsonPropertyName("currentMood")]
        public string? CurrentMood { get; init; }

        [JsonPropertyName("deviceType")]
        public string DeviceType { get; init; } = "web";
    }

    public record BuddyResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("emotion")]
        public string Emotion { get; init; } = "neutral";

        [JsonPropertyName("responseType")]
        public ResponseType ResponseType { get; init; } = ResponseType.Conversational;

        [JsonPropertyName("confidence")]
        public double Confidence { get; init; } = 1.0;

        [JsonPropertyName("processingTime")]
        public TimeSpan ProcessingTime { get; set; }

        [JsonPropertyName("usedCapabilities")]
        public List<string> UsedCapabilities { get; init; } = new();

        [JsonPropertyName("followUpQuestions")]
        public List<string> FollowUpQuestions { get; init; } = new();
    }

    public record ConversationState
    {
        [JsonPropertyName("messageCount")]
        public int MessageCount { get; init; }

        [JsonPropertyName("currentTopic")]
        public string? CurrentTopic { get; init; }

        [JsonPropertyName("buddyMood")]
        public string BuddyMood { get; init; } = "helpful";

        [JsonPropertyName("contextAwareness")]
        public Dictionary<string, object> ContextAwareness { get; init; } = new();

        [JsonPropertyName("lastActiveAt")]
        public DateTime LastActiveAt { get; init; } = DateTime.UtcNow;
    }

    public record SuggestedChatAction
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; init; } = string.Empty;

        [JsonPropertyName("priority")]
        public int Priority { get; init; } = 1;
    }


    // 4. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");

            RuleFor(x => x.BuddyId)
                .NotEmpty().WithMessage("Buddy ID is required")
                .MaximumLength(100).WithMessage("Buddy ID must not exceed 100 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MinimumLength(1).WithMessage("Message must not be empty")
                .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");

            RuleFor(x => x.ConversationId)
                .MaximumLength(100).WithMessage("Conversation ID must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.ConversationId));

            RuleFor(x => x.MessageType)
                .IsInEnum().WithMessage("Invalid message type");
        }
    }

    // 5. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<BuddyDocument> _buddyRepository;
        private readonly ICosmosRepository<PersonaDocument> _personaRepository;
        private readonly ICosmosRepository<TaskDocument> _taskRepository;
        private readonly ICosmosRepository<ConversationDocument> _conversationRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<BuddyDocument> buddyRepository,
            ICosmosRepository<PersonaDocument> personaRepository,
            ICosmosRepository<TaskDocument> taskRepository,
            ICosmosRepository<ConversationDocument> conversationRepository,
            ILogger<Handler> logger)
        {
            _buddyRepository = buddyRepository;
            _personaRepository = personaRepository;
            _taskRepository = taskRepository;
            _conversationRepository = conversationRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing chat message for user: {UserId}, Buddy: {BuddyId}", 
                    request.UserId, request.BuddyId);

                // 1. Verify buddy exists and belongs to user
                var buddies = await _buddyRepository.QueryAsync(
                    b => b.BuddyId == request.BuddyId && b.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!buddies.Any())
                {
                    _logger.LogWarning("Chat attempted with non-existent buddy: {BuddyId}", request.BuddyId);
                    return Result.Failure("Buddy not found or doesn't belong to user");
                }

                var buddy = buddies.First();

                // 2. Get or create conversation
                var conversation = await GetOrCreateConversation(request, buddy, cancellationToken);

                // 3. Get persona for context
                var personas = await _personaRepository.QueryAsync(
                    p => p.PersonaId == buddy.PersonaId && p.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                var persona = personas.FirstOrDefault();

                // 4. Get user's recent tasks for context
                var recentTasks = await GetRecentUserTasks(request.UserId, cancellationToken);

                // 5. Build conversation context
                var conversationContext = await BuildConversationContext(
                    buddy, persona, recentTasks, request.Context, conversation, cancellationToken);

                // 6. Generate buddy response
                var buddyResponse = await GenerateBuddyResponse(
                    request.Message, 
                    buddy, 
                    persona, 
                    conversationContext, 
                    request.MessageType,
                    cancellationToken);

                // 7. Store conversation messages
                var messageId = await StoreConversationMessages(
                    conversation, request.Message, buddyResponse, request.MessageType, cancellationToken);

                // 8. Update buddy's last active time and learning data
                await UpdateBuddyActivity(buddy, request.Message, buddyResponse, cancellationToken);

                // 9. Generate suggested actions
                var suggestedActions = GenerateSuggestedActions(buddyResponse, buddy, recentTasks);

                // 10. Create conversation state
                var conversationState = CreateConversationState(conversation, buddy, conversationContext);

                var processingTime = DateTime.UtcNow - startTime;
                buddyResponse.ProcessingTime = processingTime;

                _logger.LogInformation("Chat processed successfully: {MessageId} for conversation: {ConversationId} in {ProcessingTime}ms", 
                    messageId, conversation.ConversationId, processingTime.TotalMilliseconds);

                return Result.SuccessResult(conversation.ConversationId, messageId, buddyResponse, conversationState, suggestedActions);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat for user: {UserId}, Buddy: {BuddyId}", 
                    request.UserId, request.BuddyId);
                return Result.Failure("An error occurred processing your message. Please try again.");
            }
        }

        private async Task<ConversationDocument> GetOrCreateConversation(Command request, BuddyDocument buddy, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                var conversations = await _conversationRepository.QueryAsync(
                    c => c.ConversationId == request.ConversationId && c.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (conversations.Any())
                {
                    return conversations.First();
                }
            }

            // Create new conversation
            var conversationId = $"conv_{request.UserId}_{buddy.BuddyId}_{Guid.NewGuid():N}";
            var conversation = new ConversationDocument
            {
                Id = $"conversation_{conversationId}",
                UserId = request.UserId,
                ConversationId = conversationId,
                BuddyId = buddy.BuddyId,
                StartedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                Messages = new List<ConversationMessage>(),
                IsActive = true
            };

            await _conversationRepository.CreateOrUpdateAsync(conversation, request.UserId, cancellationToken);
            return conversation;
        }

        private async Task<List<TaskDocument>> GetRecentUserTasks(string userId, CancellationToken cancellationToken)
        {
            var tasks = await _taskRepository.QueryAsync(
                t => t.UserId == userId && (t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Pending || t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.InProgress),
                maxResults: 5,
                cancellationToken: cancellationToken);

            return tasks.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
        }

        private async Task<Dictionary<string, object>> BuildConversationContext(
            BuddyDocument buddy, 
            PersonaDocument? persona, 
            List<TaskDocument> recentTasks,
            ChatContext? userContext,
            ConversationDocument conversation,
            CancellationToken cancellationToken)
        {
            var context = new Dictionary<string, object>
            {
                ["buddy_name"] = buddy.BasicInfo.Name,
                ["buddy_type"] = buddy.BuddyType,
                ["buddy_archetype"] = buddy.Personality.Archetype,
                ["conversation_length"] = conversation.Messages.Count,
                ["recent_tasks_count"] = recentTasks.Count,
                ["user_language"] = persona?.BasicInfo.Languages.Preferred ?? "en"
            };

            // Add personality traits
            context["personality_friendliness"] = buddy.Personality.Traits.Friendliness;
            context["personality_professionalism"] = buddy.Personality.Traits.Professionalism;
            context["personality_proactiveness"] = buddy.Personality.Traits.Proactiveness;
            context["personality_formality"] = buddy.Personality.Traits.Formality;

            // Add communication style
            context["communication_tone"] = buddy.Personality.CommunicationStyle.Tone;
            context["communication_verbosity"] = buddy.Personality.CommunicationStyle.Verbosity;
            context["use_emojis"] = buddy.Personality.CommunicationStyle.UseEmojis;
            context["use_humor"] = buddy.Personality.CommunicationStyle.UseHumor;

            // Add user context if provided
            if (userContext != null)
            {
                context["user_location"] = userContext.CurrentLocation ?? "unknown";
                context["time_of_day"] = userContext.TimeOfDay ?? GetTimeOfDay();
                context["user_mood"] = userContext.CurrentMood ?? "neutral";
                context["device_type"] = userContext.DeviceType;
            }

            // Add persona context
            if (persona != null)
            {
                context["user_age"] = persona.BasicInfo.Age;
                context["user_interests"] = persona.BasicInfo.Interests;
                context["user_occupation"] = persona.BasicInfo.Occupation;
                context["ui_complexity"] = persona.TypeLeapConfig.UIComplexityLevel;
            }

            // Add task context
            if (recentTasks.Any())
            {
                context["pending_tasks"] = recentTasks.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Pending).Select(t => t.Title).ToList();
                context["in_progress_tasks"] = recentTasks.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.InProgress).Select(t => t.Title).ToList();
                context["overdue_tasks"] = recentTasks.Where(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed).Select(t => t.Title).ToList();
            }

            return await Task.FromResult(context);
        }

        private async Task<BuddyResponse> GenerateBuddyResponse(
            string message, 
            BuddyDocument buddy, 
            PersonaDocument? persona,
            Dictionary<string, object> context,
            MessageType messageType,
            CancellationToken cancellationToken)
        {
            // In a full implementation, this would use AI/LLM to generate responses
            // For now, we'll use rule-based responses with personality adaptation

            var response = await ProcessMessageWithPersonality(message, buddy, context, messageType, cancellationToken);
            
            return new BuddyResponse
            {
                Message = response.message,
                Emotion = response.emotion,
                ResponseType = response.responseType,
                Confidence = response.confidence,
                UsedCapabilities = response.usedCapabilities,
                FollowUpQuestions = response.followUpQuestions
            };
        }

        private async Task<(string message, string emotion, ResponseType responseType, double confidence, List<string> usedCapabilities, List<string> followUpQuestions)> ProcessMessageWithPersonality(
            string message, 
            BuddyDocument buddy, 
            Dictionary<string, object> context,
            MessageType messageType,
            CancellationToken cancellationToken)
        {
            var messageLower = message.ToLowerInvariant();
            var language = context.GetValueOrDefault("user_language", "en")?.ToString() ?? "en";
            var usedCapabilities = new List<string>();
            var followUpQuestions = new List<string>();

            // Determine response based on message type and content
            string responseMessage;
            string emotion = "helpful";
            ResponseType responseType = ResponseType.Conversational;
            double confidence = 0.8;

            if (messageType == MessageType.Greeting || ContainsGreeting(messageLower, language))
            {
                responseMessage = GenerateGreetingResponse(buddy, context, language);
                emotion = "friendly";
                followUpQuestions = GenerateGreetingFollowUps(buddy, context, language);
            }
            else if (ContainsTaskKeywords(messageLower, language))
            {
                responseMessage = GenerateTaskResponse(message, buddy, context, language);
                responseType = ResponseType.TaskCreated;
                emotion = "helpful";
                usedCapabilities.Add("task_management");
                followUpQuestions = GenerateTaskFollowUps(buddy, context, language);
            }
            else if (ContainsQuestionKeywords(messageLower, language))
            {
                responseMessage = GenerateQuestionResponse(message, buddy, context, language);
                responseType = ResponseType.Informational;
                emotion = "knowledgeable";
                followUpQuestions = GenerateQuestionFollowUps(buddy, context, language);
            }
            else if (ContainsHelpKeywords(messageLower, language))
            {
                responseMessage = GenerateHelpResponse(buddy, context, language);
                responseType = ResponseType.Suggestion;
                emotion = "supportive";
                followUpQuestions = GenerateHelpFollowUps(buddy, context, language);
            }
            else
            {
                responseMessage = GenerateGeneralResponse(message, buddy, context, language);
                followUpQuestions = GenerateGeneralFollowUps(buddy, context, language);
            }

            // Apply personality modifications
            responseMessage = ApplyPersonalityToResponse(responseMessage, buddy, context);

            return await Task.FromResult((responseMessage, emotion, responseType, confidence, usedCapabilities, followUpQuestions));
        }

        private bool ContainsGreeting(string message, string language)
        {
            var greetings = language switch
            {
                "de" => new[] { "hallo", "hi", "guten tag", "guten morgen", "guten abend" },
                "es" => new[] { "hola", "buenos días", "buenas tardes", "buenas noches" },
                _ => new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening" }
            };

            return greetings.Any(greeting => message.Contains(greeting));
        }

        private bool ContainsTaskKeywords(string message, string language)
        {
            var taskKeywords = language switch
            {
                "de" => new[] { "erinnere mich", "aufgabe", "todo", "muss", "sollte", "plan" },
                "es" => new[] { "recuérdame", "tarea", "debo", "tengo que", "planificar" },
                _ => new[] { "remind me", "task", "todo", "need to", "have to", "schedule", "plan" }
            };

            return taskKeywords.Any(keyword => message.Contains(keyword));
        }

        private bool ContainsQuestionKeywords(string message, string language)
        {
            var questionKeywords = language switch
            {
                "de" => new[] { "was", "wie", "wann", "wo", "warum", "wer", "?" },
                "es" => new[] { "qué", "cómo", "cuándo", "dónde", "por qué", "quién", "?" },
                _ => new[] { "what", "how", "when", "where", "why", "who", "?" }
            };

            return questionKeywords.Any(keyword => message.Contains(keyword));
        }

        private bool ContainsHelpKeywords(string message, string language)
        {
            var helpKeywords = language switch
            {
                "de" => new[] { "hilfe", "unterstützung", "hilf mir", "kannst du" },
                "es" => new[] { "ayuda", "apoyo", "ayúdame", "puedes" },
                _ => new[] { "help", "support", "assist", "can you", "could you" }
            };

            return helpKeywords.Any(keyword => message.Contains(keyword));
        }

        private string GenerateGreetingResponse(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            var timeOfDay = context.GetValueOrDefault("time_of_day", "").ToString();
            var buddyName = buddy.BasicInfo.Name;

            var greetings = language switch
            {
                "de" => timeOfDay switch
                {
                    "morning" => $"Guten Morgen! Ich bin {buddyName}, dein {buddy.BuddyType}. Wie kann ich dir heute helfen?",
                    "evening" => $"Guten Abend! Schön dich zu sehen. Ich bin {buddyName}, bereit dir zu helfen.",
                    _ => $"Hallo! Ich bin {buddyName}, dein persönlicher Assistent. Womit kann ich dir behilflich sein?"
                },
                "es" => timeOfDay switch
                {
                    "morning" => $"¡Buenos días! Soy {buddyName}, tu {buddy.BuddyType}. ¿Cómo puedo ayudarte hoy?",
                    "evening" => $"¡Buenas tardes! Qué bueno verte. Soy {buddyName}, listo para ayudarte.",
                    _ => $"¡Hola! Soy {buddyName}, tu asistente personal. ¿En qué puedo ayudarte?"
                },
                _ => timeOfDay switch
                {
                    "morning" => $"Good morning! I'm {buddyName}, your {buddy.BuddyType}. How can I help you today?",
                    "evening" => $"Good evening! Great to see you. I'm {buddyName}, ready to assist you.",
                    _ => $"Hello! I'm {buddyName}, your personal assistant. What can I help you with?"
                }
            };

            return greetings;
        }

        private string GenerateTaskResponse(string message, BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            var responses = language switch
            {
                "de" => new[]
                {
                    "Verstanden! Ich erstelle diese Aufgabe für dich. Möchtest du eine Erinnerung dafür setzen?",
                    "Perfekt! Ich kümmere mich darum. Soll ich dir auch ähnliche Aufgaben vorschlagen?",
                    "Alles klar! Die Aufgabe ist erstellt. Brauchst du Hilfe bei der Planung?"
                },
                "es" => new[]
                {
                    "¡Entendido! Creo esta tarea para ti. ¿Quieres que configure un recordatorio?",
                    "¡Perfecto! Me encargo de eso. ¿Te sugiero tareas similares también?",
                    "¡Muy bien! Tarea creada. ¿Necesitas ayuda con la planificación?"
                },
                _ => new[]
                {
                    "Got it! I'll create this task for you. Would you like me to set a reminder?",
                    "Perfect! I'll take care of that. Should I also suggest similar tasks?",
                    "Understood! Task created. Do you need help with planning this?"
                }
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        private string GenerateQuestionResponse(string message, BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            var responses = language switch
            {
                "de" => new[]
                {
                    "Das ist eine interessante Frage! Lass mich dir dabei helfen.",
                    "Gerne erkläre ich dir das. Basierend auf deinen Interessen...",
                    "Tolle Frage! Hier ist was ich weiß und was dir helfen könnte:"
                },
                "es" => new[]
                {
                    "¡Esa es una pregunta interesante! Déjame ayudarte con eso.",
                    "Con gusto te explico eso. Basándome en tus intereses...",
                    "¡Excelente pregunta! Esto es lo que sé y lo que podría ayudarte:"
                },
                _ => new[]
                {
                    "That's an interesting question! Let me help you with that.",
                    "I'd be happy to explain that. Based on your interests...",
                    "Great question! Here's what I know and what might help you:"
                }
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        private string GenerateHelpResponse(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            var capabilities = new List<string>();
            if (buddy.Capabilities.TaskManagement.Enabled) capabilities.Add("task management");
            if (buddy.Capabilities.EmailManagement.Enabled) capabilities.Add("email organization");
            if (buddy.Capabilities.CalendarIntegration.Enabled) capabilities.Add("calendar planning");

            var capabilitiesText = string.Join(", ", capabilities);

            return language switch
            {
                "de" => $"Natürlich helfe ich dir gerne! Ich kann dir mit {capabilitiesText} helfen. Was brauchst du zuerst?",
                "es" => $"¡Por supuesto que te ayudo! Puedo asistirte con {capabilitiesText}. ¿Qué necesitas primero?",
                _ => $"Of course I'd love to help! I can assist you with {capabilitiesText}. What do you need first?"
            };
        }

        private string GenerateGeneralResponse(string message, BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            var responses = language switch
            {
                "de" => new[]
                {
                    "Ich verstehe! Erzähl mir mehr darüber.",
                    "Das klingt interessant. Wie kann ich dir dabei helfen?",
                    "Verstehe ich richtig, dass...? Lass mich wissen, wie ich unterstützen kann."
                },
                "es" => new[]
                {
                    "¡Entiendo! Cuéntame más sobre eso.",
                    "Eso suena interesante. ¿Cómo puedo ayudarte con eso?",
                    "¿Entiendo correctamente que...? Déjame saber cómo puedo apoyarte."
                },
                _ => new[]
                {
                    "I understand! Tell me more about that.",
                    "That sounds interesting. How can I help you with that?",
                    "I see what you mean. Let me know how I can support you with this."
                }
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        private List<string> GenerateGreetingFollowUps(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            return language switch
            {
                "de" => new List<string> { "Wie war dein Tag bisher?", "Hast du Pläne für heute?", "Kann ich dir bei etwas helfen?" },
                "es" => new List<string> { "¿Cómo ha estado tu día?", "¿Tienes planes para hoy?", "¿Puedo ayudarte con algo?" },
                _ => new List<string> { "How has your day been?", "Do you have any plans today?", "Is there something I can help you with?" }
            };
        }

        private List<string> GenerateTaskFollowUps(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            return language switch
            {
                "de" => new List<string> { "Soll ich eine Erinnerung setzen?", "Brauchst du Hilfe bei der Zeitplanung?", "Gibt es ähnliche Aufgaben?" },
                "es" => new List<string> { "¿Configuro un recordatorio?", "¿Necesitas ayuda con la planificación?", "¿Hay tareas similares?" },
                _ => new List<string> { "Should I set a reminder?", "Do you need help with timing?", "Are there similar tasks?" }
            };
        }

        private List<string> GenerateQuestionFollowUps(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            return language switch
            {
                "de" => new List<string> { "Brauchst du weitere Informationen?", "Soll ich das erklären?", "Hast du noch Fragen?" },
                "es" => new List<string> { "¿Necesitas más información?", "¿Te explico eso?", "¿Tienes más preguntas?" },
                _ => new List<string> { "Do you need more information?", "Should I explain that?", "Do you have more questions?" }
            };
        }

        private List<string> GenerateHelpFollowUps(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            return language switch
            {
                "de" => new List<string> { "Womit soll ich anfangen?", "Was ist am wichtigsten?", "Brauchst du eine Schritt-für-Schritt Anleitung?" },
                "es" => new List<string> { "¿Por dónde empiezo?", "¿Qué es lo más importante?", "¿Necesitas una guía paso a paso?" },
                _ => new List<string> { "Where should I start?", "What's most important?", "Do you need step-by-step guidance?" }
            };
        }

        private List<string> GenerateGeneralFollowUps(BuddyDocument buddy, Dictionary<string, object> context, string language)
        {
            return language switch
            {
                "de" => new List<string> { "Erzähl mir mehr!", "Wie fühlst du dich dabei?", "Was sind deine Gedanken dazu?" },
                "es" => new List<string> { "¡Cuéntame más!", "¿Cómo te sientes al respecto?", "¿Cuáles son tus pensamientos?" },
                _ => new List<string> { "Tell me more!", "How do you feel about that?", "What are your thoughts on this?" }
            };
        }

        private string ApplyPersonalityToResponse(string response, BuddyDocument buddy, Dictionary<string, object> context)
        {
            var personality = buddy.Personality;
            
            // Apply formality
            if (personality.Traits.Formality > 0.7)
            {
                response = MakeFormal(response);
            }
            else if (personality.Traits.Formality < 0.3)
            {
                response = MakeCasual(response);
            }

            // Apply emojis if enabled and friendliness is high
            if (personality.CommunicationStyle.UseEmojis && personality.Traits.Friendliness > 0.7)
            {
                response = AddEmojis(response, personality.CommunicationStyle.Tone);
            }

            // Apply humor if enabled
            if (personality.CommunicationStyle.UseHumor && personality.Traits.Friendliness > 0.8)
            {
                response = AddHumor(response);
            }

            return response;
        }

        private string MakeFormal(string response)
        {
            return response
                .Replace("I'm", "I am")
                .Replace("you're", "you are")
                .Replace("can't", "cannot")
                .Replace("won't", "will not");
        }

        private string MakeCasual(string response)
        {
            return response
                .Replace("I am", "I'm")
                .Replace("you are", "you're")
                .Replace("cannot", "can't")
                .Replace("will not", "won't");
        }

        private string AddEmojis(string response, string tone)
        {
            var emoji = tone switch
            {
                "friendly" => " 😊",
                "enthusiastic" => " 🎉",
                "helpful" => " 👍",
                "supportive" => " 🤗",
                "motivating" => " 💪",
                _ => " 😊"
            };

            return response + emoji;
        }

        private string AddHumor(string response)
        {
            // Simple humor addition - in a full implementation this would be more sophisticated
            if (response.Contains("task"))
            {
                return response + " (Don't worry, I won't judge if you procrastinate a little! 😉)";
            }
            return response;
        }

        private async Task<string> StoreConversationMessages(
            ConversationDocument conversation,
            string userMessage,
            BuddyResponse buddyResponse,
            MessageType messageType,
            CancellationToken cancellationToken)
        {
            var messageId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;

            // Add user message
            conversation.Messages.Add(new ConversationMessage
            {
                MessageId = $"{messageId}_user",
                Sender = MessageSender.User,
                Content = userMessage,
                MessageType = messageType,
                Timestamp = timestamp
            });

            // Add buddy response
            conversation.Messages.Add(new ConversationMessage
            {
                MessageId = $"{messageId}_buddy",
                Sender = MessageSender.Buddy,
                Content = buddyResponse.Message,
                MessageType = MapResponseTypeToMessageType(buddyResponse.ResponseType),
                Timestamp = timestamp.AddMilliseconds(100),
                Metadata = new Dictionary<string, object>
                {
                    ["emotion"] = buddyResponse.Emotion,
                    ["confidence"] = buddyResponse.Confidence,
                    ["used_capabilities"] = buddyResponse.UsedCapabilities,
                    ["follow_up_questions"] = buddyResponse.FollowUpQuestions
                }
            });

            conversation.LastMessageAt = timestamp;
            conversation.MessageCount = conversation.Messages.Count;

            await _conversationRepository.CreateOrUpdateAsync(conversation, conversation.UserId, cancellationToken);

            return messageId;
        }

        private async Task UpdateBuddyActivity(
            BuddyDocument buddy,
            string userMessage,
            BuddyResponse buddyResponse,
            CancellationToken cancellationToken)
        {
            buddy.LastActiveAt = DateTime.UtcNow;

            // Simple learning: update topic interests
            var messageWords = userMessage.ToLowerInvariant().Split(' ');
            foreach (var word in messageWords.Where(w => w.Length > 3))
            {
                if (!buddy.LearningData.UserPreferences.TopicInterests.Contains(word))
                {
                    buddy.LearningData.UserPreferences.TopicInterests.Add(word);
                }
            }

            // Add adaptation record
            buddy.LearningData.AdaptationHistory.Add(new AdaptationRecord
            {
                Timestamp = DateTime.UtcNow,
                Adaptation = $"Processed {buddyResponse.ResponseType} message",
                Reason = "Conversation interaction"
            });

            // Keep only recent adaptations (last 100)
            if (buddy.LearningData.AdaptationHistory.Count > 100)
            {
                buddy.LearningData.AdaptationHistory = buddy.LearningData.AdaptationHistory
                    .OrderByDescending(a => a.Timestamp)
                    .Take(100)
                    .ToList();
            }

            await _buddyRepository.CreateOrUpdateAsync(buddy, buddy.UserId, cancellationToken);
        }

        private List<SuggestedChatAction> GenerateSuggestedActions(BuddyResponse buddyResponse, BuddyDocument buddy, List<TaskDocument> recentTasks)
        {
            var actions = new List<SuggestedChatAction>();

            // Task-related suggestions
            if (recentTasks.Any() && buddy.Capabilities.TaskManagement.Enabled)
            {
                actions.Add(new SuggestedChatAction
                {
                    Type = "view_tasks",
                    Title = "View My Tasks",
                    Action = "show_task_list",
                    Priority = 1
                });

                if (recentTasks.Any(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow))
                {
                    actions.Add(new SuggestedChatAction
                    {
                        Type = "overdue_tasks",
                        Title = "Review Overdue Tasks",
                        Action = "show_overdue_tasks",
                        Priority = 2
                    });
                }
            }

            // Response-specific suggestions
            if (buddyResponse.ResponseType == ResponseType.TaskCreated)
            {
                actions.Add(new SuggestedChatAction
                {
                    Type = "set_reminder",
                    Title = "Set Reminder",
                    Action = "configure_reminder",
                    Priority = 1
                });
            }

            return actions;
        }

        private ConversationState CreateConversationState(ConversationDocument conversation, BuddyDocument buddy, Dictionary<string, object> context)
        {
            return new ConversationState
            {
                MessageCount = conversation.Messages.Count,
                CurrentTopic = DetermineCurrentTopic(conversation.Messages.TakeLast(5)),
                BuddyMood = buddy.Personality.CommunicationStyle.Tone,
                ContextAwareness = context,
                LastActiveAt = DateTime.UtcNow
            };
        }

        private string? DetermineCurrentTopic(IEnumerable<ConversationMessage> recentMessages)
        {
            var messages = recentMessages.ToList();
            if (!messages.Any()) return null;

            var content = string.Join(" ", messages.Select(m => m.Content)).ToLowerInvariant();

            if (content.Contains("task") || content.Contains("reminder")) return "tasks";
            if (content.Contains("email")) return "email";
            if (content.Contains("schedule") || content.Contains("calendar")) return "scheduling";
            if (content.Contains("help") || content.Contains("question")) return "assistance";

            return "general";
        }

        private string GetTimeOfDay()
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                >= 5 and < 12 => "morning",
                >= 12 and < 17 => "afternoon",
                >= 17 and < 21 => "evening",
                _ => "night"
            };
        }

        private static MessageType MapResponseTypeToMessageType(ResponseType responseType)
        {
            return responseType switch
            {
                ResponseType.TaskCreated => MessageType.TaskCreated,
                ResponseType.Suggestion => MessageType.Suggestion,
                ResponseType.Informational => MessageType.Text,
                ResponseType.Conversational => MessageType.Text,
                ResponseType.Error => MessageType.System,
                _ => MessageType.Text // Default fallback
            };
        }
    }

    // 6. Endpoint (HTTP API)
    [ApiController]
    [Route("api/buddies")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Send a message to an AI buddy
        /// </summary>
        /// <param name="command">Chat message parameters</param>
        /// <returns>Buddy response with conversation state</returns>
        [HttpPost("chat")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> ChatWithBuddy([FromBody] Command command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(result);
                }

                if (result.ValidationErrors != null)
                {
                    return BadRequest(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

