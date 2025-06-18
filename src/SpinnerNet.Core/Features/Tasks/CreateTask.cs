using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SpinnerNet.Core.Features.Tasks;

/// <summary>
/// Vertical slice for creating tasks from natural language input
/// Core Zeitsparkasse feature: "Remind me to call mom tomorrow" → structured task
/// </summary>
public static class CreateTask
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("personaId")]
        public string? PersonaId { get; init; }

        [JsonPropertyName("input")]
        public string Input { get; init; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; init; } = "en";

        [JsonPropertyName("timezone")]
        public string Timezone { get; init; } = "UTC";

        [JsonPropertyName("context")]
        public TaskContext? Context { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("taskId")]
        public string? TaskId { get; init; }

        [JsonPropertyName("task")]
        public TaskDocument? Task { get; init; }

        [JsonPropertyName("aiInsights")]
        public TaskAIInsights? AIInsights { get; init; }

        [JsonPropertyName("suggestedActions")]
        public List<SuggestedAction> SuggestedActions { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string taskId, TaskDocument task, TaskAIInsights insights, List<SuggestedAction> actions) =>
            new() 
            { 
                Success = true, 
                TaskId = taskId, 
                Task = task,
                AIInsights = insights,
                SuggestedActions = actions
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public record TaskContext
    {
        [JsonPropertyName("currentLocation")]
        public string? CurrentLocation { get; init; }

        [JsonPropertyName("currentActivity")]
        public string? CurrentActivity { get; init; }

        [JsonPropertyName("relatedGoals")]
        public List<string> RelatedGoals { get; init; } = new();

        [JsonPropertyName("deviceType")]
        public string DeviceType { get; init; } = "web";
    }

    public record TaskAIInsights
    {
        [JsonPropertyName("extractedTitle")]
        public string ExtractedTitle { get; set; } = string.Empty;

        [JsonPropertyName("detectedDueDate")]
        public DateTime? DetectedDueDate { get; set; }

        [JsonPropertyName("detectedPriority")]
        public TaskPriority DetectedPriority { get; set; } = TaskPriority.Medium;

        [JsonPropertyName("suggestedCategory")]
        public string SuggestedCategory { get; set; } = "general";

        [JsonPropertyName("estimatedDuration")]
        public int? EstimatedDurationMinutes { get; set; }

        [JsonPropertyName("detectedEntities")]
        public List<string> DetectedEntities { get; set; } = new();

        [JsonPropertyName("suggestedTags")]
        public List<string> SuggestedTags { get; set; } = new();

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; } = 1.0;

        [JsonPropertyName("processingTime")]
        public TimeSpan ProcessingTime { get; set; }
    }

    public record SuggestedAction
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("actionData")]
        public Dictionary<string, object> ActionData { get; init; } = new();
    }

    // 4. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");

            RuleFor(x => x.Input)
                .NotEmpty().WithMessage("Task input is required")
                .MinimumLength(3).WithMessage("Task input must be at least 3 characters")
                .MaximumLength(1000).WithMessage("Task input must not exceed 1000 characters");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required")
                .Length(2, 5).WithMessage("Language must be a valid language code");

            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("Timezone is required")
                .MaximumLength(50).WithMessage("Timezone must not exceed 50 characters");

            RuleFor(x => x.PersonaId)
                .MaximumLength(100).WithMessage("Persona ID must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.PersonaId));
        }
    }

    // 5. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<TaskDocument> _taskRepository;
        private readonly ICosmosRepository<UserDocument> _userRepository;
        private readonly ICosmosRepository<PersonaDocument> _personaRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<TaskDocument> taskRepository,
            ICosmosRepository<UserDocument> userRepository,
            ICosmosRepository<PersonaDocument> personaRepository,
            ILogger<Handler> logger)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _personaRepository = personaRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Creating task from natural language for user: {UserId}, Input: {Input}", 
                    request.UserId, request.Input);

                // 1. Verify user exists
                var users = await _userRepository.QueryAsync(
                    u => u.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!users.Any())
                {
                    _logger.LogWarning("Task creation attempted for non-existent user: {UserId}", request.UserId);
                    return Result.Failure("User not found");
                }

                var user = users.First();

                // 2. Get persona if specified
                PersonaDocument? persona = null;
                if (!string.IsNullOrEmpty(request.PersonaId))
                {
                    var personas = await _personaRepository.QueryAsync(
                        p => p.PersonaId == request.PersonaId && p.UserId == request.UserId,
                        maxResults: 1,
                        cancellationToken: cancellationToken);
                    
                    persona = personas.FirstOrDefault();
                }

                // 3. Process natural language input with AI
                var aiInsights = await ProcessNaturalLanguageInput(
                    request.Input, 
                    request.Language, 
                    request.Timezone,
                    persona,
                    cancellationToken);

                // 4. Create task document
                var taskId = $"task_{request.UserId}_{Guid.NewGuid():N}";
                var task = CreateTaskFromInsights(taskId, request, aiInsights, user);

                // 5. Save task to Cosmos DB
                await _taskRepository.CreateOrUpdateAsync(
                    task,
                    request.UserId,
                    cancellationToken);

                // 6. Generate suggested actions
                var suggestedActions = GenerateSuggestedActions(task, aiInsights, request.Language);

                var processingTime = DateTime.UtcNow - startTime;
                aiInsights.ProcessingTime = processingTime;

                _logger.LogInformation("Task created successfully: {TaskId} for user: {UserId} in {ProcessingTime}ms", 
                    taskId, request.UserId, processingTime.TotalMilliseconds);

                return Result.SuccessResult(taskId, task, aiInsights, suggestedActions);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for user: {UserId}, Input: {Input}", 
                    request.UserId, request.Input);
                return Result.Failure("An error occurred creating the task. Please try again.");
            }
        }

        private async Task<TaskAIInsights> ProcessNaturalLanguageInput(
            string input, 
            string language, 
            string timezone,
            PersonaDocument? persona,
            CancellationToken cancellationToken)
        {
            // In a full implementation, this would use AI/LLM to extract information
            // For now, we'll use pattern matching and heuristics

            var insights = new TaskAIInsights();

            // Extract title (clean up the input)
            insights.ExtractedTitle = ExtractTitle(input, language);

            // Detect due date/time
            insights.DetectedDueDate = ExtractDueDate(input, language, timezone);

            // Detect priority
            insights.DetectedPriority = ExtractPriority(input, language);

            // Suggest category
            insights.SuggestedCategory = SuggestCategory(input, language);

            // Estimate duration
            insights.EstimatedDurationMinutes = EstimateDuration(input, insights.SuggestedCategory);

            // Extract entities (people, places, things)
            insights.DetectedEntities = ExtractEntities(input, language);

            // Suggest tags
            insights.SuggestedTags = SuggestTags(input, insights.SuggestedCategory, insights.DetectedEntities);

            // Calculate confidence
            insights.Confidence = CalculateConfidence(insights);

            return await Task.FromResult(insights);
        }

        private string ExtractTitle(string input, string language)
        {
            // Remove common prefixes and clean up
            var cleanInput = input.Trim();

            // Remove reminder prefixes
            var reminderPrefixes = language switch
            {
                "de" => new[] { "erinnere mich", "ich muss", "ich sollte", "vergiss nicht" },
                "es" => new[] { "recuérdame", "tengo que", "debo", "no olvides" },
                _ => new[] { "remind me to", "remember to", "don't forget to", "i need to", "i should", "i have to" }
            };

            foreach (var prefix in reminderPrefixes)
            {
                if (cleanInput.ToLowerInvariant().StartsWith(prefix))
                {
                    cleanInput = cleanInput.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Remove time expressions from title
            var timePattern = @"\b(tomorrow|today|tonight|next week|next month|at \d+|in \d+|on \w+)\b";
            cleanInput = Regex.Replace(cleanInput, timePattern, "", RegexOptions.IgnoreCase).Trim();

            // Capitalize first letter
            if (cleanInput.Length > 0)
            {
                cleanInput = char.ToUpper(cleanInput[0]) + cleanInput.Substring(1);
            }

            return cleanInput.Length > 0 ? cleanInput : "New Task";
        }

        private DateTime? ExtractDueDate(string input, string language, string timezone)
        {
            var inputLower = input.ToLowerInvariant();
            var now = DateTime.UtcNow;

            // Convert to user's timezone for date calculations
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(now, userTimeZone);

            // Common time expressions
            if (ContainsAny(inputLower, language == "de" ? new[] { "heute" } : 
                                        language == "es" ? new[] { "hoy" } : 
                                        new[] { "today" }))
            {
                return TimeZoneInfo.ConvertTimeToUtc(localNow.Date.AddHours(18), userTimeZone); // 6 PM today
            }

            if (ContainsAny(inputLower, language == "de" ? new[] { "morgen" } :
                                        language == "es" ? new[] { "mañana" } :
                                        new[] { "tomorrow" }))
            {
                return TimeZoneInfo.ConvertTimeToUtc(localNow.Date.AddDays(1).AddHours(9), userTimeZone); // 9 AM tomorrow
            }

            if (ContainsAny(inputLower, language == "de" ? new[] { "nächste woche" } :
                                        language == "es" ? new[] { "próxima semana" } :
                                        new[] { "next week" }))
            {
                var nextWeek = localNow.AddDays(7 - (int)localNow.DayOfWeek + 1); // Next Monday
                return TimeZoneInfo.ConvertTimeToUtc(nextWeek.Date.AddHours(9), userTimeZone);
            }

            // Extract specific times (e.g., "at 3pm", "at 15:00")
            var timeMatch = Regex.Match(inputLower, @"at (\d{1,2})(?::(\d{2}))?\s*(am|pm)?");
            if (timeMatch.Success)
            {
                if (int.TryParse(timeMatch.Groups[1].Value, out var hour))
                {
                    var minutes = timeMatch.Groups[2].Success ? int.Parse(timeMatch.Groups[2].Value) : 0;
                    var isPm = timeMatch.Groups[3].Value == "pm";
                    
                    if (isPm && hour != 12) hour += 12;
                    if (!isPm && hour == 12) hour = 0;

                    var timeToday = localNow.Date.AddHours(hour).AddMinutes(minutes);
                    if (timeToday < localNow) timeToday = timeToday.AddDays(1); // If time has passed, schedule for tomorrow
                    
                    return TimeZoneInfo.ConvertTimeToUtc(timeToday, userTimeZone);
                }
            }

            return null; // No due date detected
        }

        private TaskPriority ExtractPriority(string input, string language)
        {
            var inputLower = input.ToLowerInvariant();

            // Urgent indicators
            var urgentKeywords = language switch
            {
                "de" => new[] { "dringend", "sofort", "asap", "wichtig", "eilig" },
                "es" => new[] { "urgente", "inmediatamente", "importante", "prisa" },
                _ => new[] { "urgent", "asap", "immediately", "important", "critical", "emergency" }
            };

            if (ContainsAny(inputLower, urgentKeywords))
            {
                return TaskPriority.Urgent;
            }

            // High priority indicators
            var highKeywords = language switch
            {
                "de" => new[] { "muss", "sollte unbedingt", "priorität" },
                "es" => new[] { "debo", "necesito", "prioridad" },
                _ => new[] { "must", "need to", "should", "priority", "high" }
            };

            if (ContainsAny(inputLower, highKeywords))
            {
                return TaskPriority.High;
            }

            // Low priority indicators
            var lowKeywords = language switch
            {
                "de" => new[] { "irgendwann", "wenn zeit", "optional" },
                "es" => new[] { "algún día", "cuando tenga tiempo", "opcional" },
                _ => new[] { "sometime", "when i have time", "optional", "maybe", "low priority" }
            };

            if (ContainsAny(inputLower, lowKeywords))
            {
                return TaskPriority.Low;
            }

            return TaskPriority.Medium; // Default
        }

        private string SuggestCategory(string input, string language)
        {
            var inputLower = input.ToLowerInvariant();

            // Category mapping by keywords
            var categories = new Dictionary<string, string[]>
            {
                ["work"] = language == "de" ? new[] { "arbeit", "job", "büro", "meeting", "projekt", "kollege" } :
                          language == "es" ? new[] { "trabajo", "oficina", "reunión", "proyecto", "colega" } :
                          new[] { "work", "job", "office", "meeting", "project", "colleague", "deadline" },

                ["health"] = language == "de" ? new[] { "arzt", "medizin", "sport", "fitness", "gesundheit" } :
                            language == "es" ? new[] { "médico", "medicina", "deporte", "fitness", "salud" } :
                            new[] { "doctor", "medicine", "exercise", "fitness", "health", "workout" },

                ["family"] = language == "de" ? new[] { "familie", "mutter", "vater", "kind", "eltern" } :
                            language == "es" ? new[] { "familia", "madre", "padre", "hijo", "padres" } :
                            new[] { "family", "mom", "dad", "parent", "child", "kids", "mother", "father" },

                ["shopping"] = language == "de" ? new[] { "einkaufen", "kaufen", "geschäft", "markt" } :
                              language == "es" ? new[] { "comprar", "tienda", "mercado", "supermercado" } :
                              new[] { "buy", "shop", "store", "market", "grocery", "purchase" },

                ["home"] = language == "de" ? new[] { "haus", "putzen", "reparieren", "garten" } :
                          language == "es" ? new[] { "casa", "limpiar", "reparar", "jardín" } :
                          new[] { "home", "house", "clean", "repair", "garden", "maintenance" },

                ["finance"] = language == "de" ? new[] { "geld", "bank", "rechnung", "steuer", "versicherung" } :
                             language == "es" ? new[] { "dinero", "banco", "factura", "impuesto", "seguro" } :
                             new[] { "money", "bank", "bill", "tax", "insurance", "payment", "budget" }
            };

            foreach (var category in categories)
            {
                if (ContainsAny(inputLower, category.Value))
                {
                    return category.Key;
                }
            }

            return "general";
        }

        private int? EstimateDuration(string input, string category)
        {
            var inputLower = input.ToLowerInvariant();

            // Look for explicit time mentions
            var durationMatch = Regex.Match(inputLower, @"(\d+)\s*(minute|hour|min|hr)s?");
            if (durationMatch.Success)
            {
                var amount = int.Parse(durationMatch.Groups[1].Value);
                var unit = durationMatch.Groups[2].Value;
                
                return unit.StartsWith("hour") || unit.StartsWith("hr") ? amount * 60 : amount;
            }

            // Category-based estimates
            return category switch
            {
                "work" => 30,
                "health" => 60,
                "shopping" => 45,
                "home" => 60,
                "finance" => 20,
                "family" => 30,
                _ => 15
            };
        }

        private List<string> ExtractEntities(string input, string language)
        {
            var entities = new List<string>();
            var inputLower = input.ToLowerInvariant();

            // Simple entity extraction using common patterns
            // People (look for common names and family terms)
            var peoplePatterns = language switch
            {
                "de" => new[] { "mutter", "vater", "mama", "papa", "freund", "kollege" },
                "es" => new[] { "madre", "padre", "mamá", "papá", "amigo", "colega" },
                _ => new[] { "mom", "dad", "mother", "father", "friend", "colleague", "boss", "doctor" }
            };

            foreach (var pattern in peoplePatterns)
            {
                if (inputLower.Contains(pattern))
                {
                    entities.Add($"person:{pattern}");
                }
            }

            // Places
            var placePatterns = language switch
            {
                "de" => new[] { "büro", "geschäft", "apotheke", "bank", "post" },
                "es" => new[] { "oficina", "tienda", "farmacia", "banco", "correos" },
                _ => new[] { "office", "store", "pharmacy", "bank", "post office", "gym", "hospital" }
            };

            foreach (var pattern in placePatterns)
            {
                if (inputLower.Contains(pattern))
                {
                    entities.Add($"place:{pattern}");
                }
            }

            return entities;
        }

        private List<string> SuggestTags(string input, string category, List<string> entities)
        {
            var tags = new List<string> { category };

            // Add entity-based tags
            foreach (var entity in entities)
            {
                var entityType = entity.Split(':')[0];
                tags.Add(entityType);
            }

            // Add priority-based tags
            var inputLower = input.ToLowerInvariant();
            if (ContainsAny(inputLower, new[] { "urgent", "important", "dringend", "urgente" }))
            {
                tags.Add("urgent");
            }

            // Add time-based tags
            if (ContainsAny(inputLower, new[] { "today", "heute", "hoy" }))
            {
                tags.Add("today");
            }
            else if (ContainsAny(inputLower, new[] { "tomorrow", "morgen", "mañana" }))
            {
                tags.Add("tomorrow");
            }

            return tags.Distinct().ToList();
        }

        private double CalculateConfidence(TaskAIInsights insights)
        {
            var confidence = 0.5; // Base confidence

            // Increase confidence based on extracted information
            if (!string.IsNullOrEmpty(insights.ExtractedTitle) && insights.ExtractedTitle != "New Task")
                confidence += 0.2;

            if (insights.DetectedDueDate.HasValue)
                confidence += 0.2;

            if (insights.DetectedEntities.Any())
                confidence += 0.1;

            if (insights.SuggestedCategory != "general")
                confidence += 0.1;

            return Math.Min(1.0, confidence);
        }

        private TaskDocument CreateTaskFromInsights(string taskId, Command request, TaskAIInsights insights, UserDocument user)
        {
            return new TaskDocument
            {
                Id = $"task_{taskId}",
                UserId = request.UserId,
                TaskId = taskId,
                PersonaId = request.PersonaId,
                Title = insights.ExtractedTitle,
                Description = request.Input,
                OriginalInput = request.Input,
                Status = SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Pending,
                Priority = insights.DetectedPriority,
                Category = insights.SuggestedCategory,
                Tags = insights.SuggestedTags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = insights.DetectedDueDate,
                EstimatedMinutes = insights.EstimatedDurationMinutes,
                AIContext = new TaskAIContext
                {
                    ExtractedEntities = insights.DetectedEntities,
                    SuggestedTags = insights.SuggestedTags,
                    DetectedUrgency = (double)insights.DetectedPriority / 3.0,
                    EstimatedComplexity = insights.EstimatedDurationMinutes switch
                    {
                        <= 15 => "simple",
                        <= 60 => "medium",
                        _ => "complex"
                    },
                    ProcessingDate = DateTime.UtcNow,
                    Confidence = insights.Confidence
                }
            };
        }

        private List<SuggestedAction> GenerateSuggestedActions(TaskDocument task, TaskAIInsights insights, string language)
        {
            var actions = new List<SuggestedAction>();

            // Suggest setting a reminder if due date exists
            if (task.DueDate.HasValue)
            {
                actions.Add(new SuggestedAction
                {
                    Type = "set_reminder",
                    Title = GetLocalizedText("Set Reminder", language),
                    Description = GetLocalizedText("Get notified before this task is due", language),
                    ActionData = new Dictionary<string, object>
                    {
                        {"taskId", task.TaskId},
                        {"suggestedTime", task.DueDate.Value.AddHours(-1)}
                    }
                });
            }

            // Suggest breaking down complex tasks
            if (task.EstimatedMinutes > 60)
            {
                actions.Add(new SuggestedAction
                {
                    Type = "break_down_task",
                    Title = GetLocalizedText("Break Down Task", language),
                    Description = GetLocalizedText("Split this into smaller, manageable steps", language),
                    ActionData = new Dictionary<string, object>
                    {
                        {"taskId", task.TaskId},
                        {"estimatedSubtasks", Math.Ceiling((double)task.EstimatedMinutes.Value / 30)}
                    }
                });
            }

            // Suggest linking to goals if entities indicate it might be goal-related
            if (task.AIContext.ExtractedEntities.Any(e => e.Contains("work") || e.Contains("health") || e.Contains("family")))
            {
                actions.Add(new SuggestedAction
                {
                    Type = "link_to_goal",
                    Title = GetLocalizedText("Link to Goal", language),
                    Description = GetLocalizedText("Connect this task to one of your life goals", language),
                    ActionData = new Dictionary<string, object>
                    {
                        {"taskId", task.TaskId},
                        {"suggestedCategories", new[] { task.Category }}
                    }
                });
            }

            return actions;
        }

        private string GetLocalizedText(string text, string language)
        {
            return language switch
            {
                "de" => text switch
                {
                    "Set Reminder" => "Erinnerung setzen",
                    "Get notified before this task is due" => "Benachrichtigung vor Fälligkeit erhalten",
                    "Break Down Task" => "Aufgabe aufteilen",
                    "Split this into smaller, manageable steps" => "In kleinere, überschaubare Schritte unterteilen",
                    "Link to Goal" => "Mit Ziel verknüpfen",
                    "Connect this task to one of your life goals" => "Diese Aufgabe mit einem deiner Lebensziele verbinden",
                    _ => text
                },
                "es" => text switch
                {
                    "Set Reminder" => "Establecer recordatorio",
                    "Get notified before this task is due" => "Recibir notificación antes del vencimiento",
                    "Break Down Task" => "Dividir tarea",
                    "Split this into smaller, manageable steps" => "Dividir en pasos más pequeños y manejables",
                    "Link to Goal" => "Vincular a objetivo",
                    "Connect this task to one of your life goals" => "Conectar esta tarea con uno de tus objetivos de vida",
                    _ => text
                },
                _ => text
            };
        }

        private static bool ContainsAny(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }
    }

    // 6. Endpoint (HTTP API)
    [ApiController]
    [Route("api/tasks")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a task from natural language input
        /// </summary>
        /// <param name="command">Task creation data</param>
        /// <returns>Created task with AI insights</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(Result), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> CreateTask([FromBody] Command command)
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

            return CreatedAtAction(
                nameof(GetTask.Endpoint.GetById), // Reference to future GetTask slice
                new { id = result.TaskId },
                result);
        }
    }
}

// Future reference for GetTask slice (placeholder)
public static class GetTask
{
    public static class Endpoint
    {
        public static string GetById => "GetTaskById";
    }
}