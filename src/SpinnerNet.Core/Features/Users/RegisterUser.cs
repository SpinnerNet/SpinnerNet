using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Users;

/// <summary>
/// Vertical slice for user registration with data sovereignty preferences
/// Implements: Command → Validation → Handler → Endpoint pattern
/// </summary>
public static class RegisterUser
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; init; }

        [JsonPropertyName("language")]
        public string Language { get; init; } = "en";

        [JsonPropertyName("timezone")]
        public string Timezone { get; init; } = "UTC";

        [JsonPropertyName("dataResidencyPreference")]
        public DataResidencyPreference DataResidencyPreference { get; init; } = DataResidencyPreference.Local;

        [JsonPropertyName("acceptTerms")]
        public bool AcceptTerms { get; init; }

        [JsonPropertyName("acceptPrivacyPolicy")]
        public bool AcceptPrivacyPolicy { get; init; }

        [JsonPropertyName("acceptDataProcessing")]
        public bool AcceptDataProcessing { get; init; }

        // Parental consent for minors
        [JsonPropertyName("parentalEmail")]
        public string? ParentalEmail { get; init; }

        [JsonPropertyName("parentalConsentToken")]
        public string? ParentalConsentToken { get; init; }

        [JsonPropertyName("isParentalConsentVerified")]
        public bool IsParentalConsentVerified { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("Guid.NewGuid().ToString()")]
        public string? UserId { get; init; }

        [JsonPropertyName("user")]
        public UserDocument? User { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(UserDocument user) =>
            new() { Success = true, UserId = user.UserId, User = user };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThan(DateTime.UtcNow).WithMessage("Birth date must be in the past")
                .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Birth date must be within reasonable range")
                .Must(BeAtLeastMinimumAge).WithMessage("You must be at least 13 years old to register");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required")
                .Length(2, 5).WithMessage("Language must be a valid language code (2-5 characters)");

            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("Timezone is required")
                .MaximumLength(50).WithMessage("Timezone must not exceed 50 characters");

            RuleFor(x => x.DataResidencyPreference)
                .IsInEnum().WithMessage("Data residency preference must be a valid option");

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("You must accept the terms of service");

            RuleFor(x => x.AcceptPrivacyPolicy)
                .Equal(true).WithMessage("You must accept the privacy policy");

            RuleFor(x => x.AcceptDataProcessing)
                .Equal(true).WithMessage("You must accept data processing terms");

            // Parental consent validation for minors
            RuleFor(x => x.ParentalEmail)
                .NotEmpty().WithMessage("Parental email is required for users under 18")
                .EmailAddress().WithMessage("Parental email must be a valid email address")
                .When(x => CalculateAge(x.BirthDate) < 18);

            RuleFor(x => x.ParentalConsentToken)
                .NotEmpty().WithMessage("Parental consent verification is required for users under 18")
                .When(x => CalculateAge(x.BirthDate) < 18);

            RuleFor(x => x.IsParentalConsentVerified)
                .Equal(true).WithMessage("Parental consent must be verified for users under 18")
                .When(x => CalculateAge(x.BirthDate) < 18);
        }

        private static bool BeAtLeastMinimumAge(DateTime birthDate)
        {
            return CalculateAge(birthDate) >= 13;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (DateTime.UtcNow.DayOfYear < birthDate.DayOfYear) age--;
            return age;
        }

        private static bool BeValidLanguageCode(string languageCode)
        {
            // Simple validation for common language codes
            var validCodes = new[] { "en", "de", "fr", "es", "it", "pt", "nl", "sv", "da", "no", "fi" };
            return validCodes.Contains(languageCode.ToLowerInvariant());
        }
    }

    // 4. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<UserDocument> _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<UserDocument> userRepository,
            ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

                // 1. Calculate user age
                var age = CalculateAge(request.BirthDate);
                var isMinor = age < 18;

                _logger.LogInformation("User registration: Age {Age}, IsMinor: {IsMinor}", age, isMinor);

                // 2. Verify parental consent for minors
                if (isMinor)
                {
                    if (!request.IsParentalConsentVerified || 
                        string.IsNullOrEmpty(request.ParentalEmail) || 
                        string.IsNullOrEmpty(request.ParentalConsentToken))
                    {
                        _logger.LogWarning("Registration attempt for minor without proper parental consent");
                        return Result.Failure("Parental consent verification is required for users under 18");
                    }

                    // TODO: In production, verify the parental consent token
                    // var isConsentValid = await _parentalConsentService.VerifyConsentAsync(
                    //     request.ParentalConsentToken, request.ParentalEmail);
                    // if (!isConsentValid) return Result.Failure("Invalid parental consent");
                }

                // 3. Check if user already exists
                var existingUsers = await _userRepository.QueryAsync(
                    u => u.email == request.Email,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (existingUsers.Any())
                {
                    _logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
                    return Result.Failure("A user with this email address already exists");
                }

                // 4. Generate user ID
                var userid = $"user_{Guid.NewGuid():N}";

                // 5. Hash password (in production, use proper password hashing)
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 6. Create age-appropriate data sovereignty settings
                var dataSovereigntySettings = CreateAgeAppropriateDataSettings(age, request.DataResidencyPreference);

                // 7. Create age-appropriate safety settings
                var safetySettings = CreateAgeAppropriateSafetySettings(age);

                // 8. Create user document with age-aware configuration
                var userDocument = new UserDocument
                {
                    id = $"user_{Guid.NewGuid().ToString()}",
                    UserId = Guid.NewGuid().ToString(),
                    email = request.Email.ToLowerInvariant(),
                    displayName = request.DisplayName,
                    age = age,
                    birthDate = request.BirthDate,
                    isMinor = isMinor,
                    createdAt = DateTime.UtcNow,
                    isActive = true,
                    authProviders = new List<AuthProvider>
                    {
                        new()
                        {
                            provider = "local",
                            providerId = Guid.NewGuid().ToString(),
                            email = request.Email.ToLowerInvariant(),
                            passwordHash = passwordHash
                        }
                    },
                    preferences = new UserPreferences
                    {
                        language = request.Language,
                        timezone = request.Timezone,
                        notifications = new NotificationPreferences
                        {
                            email = true,
                            push = false
                        }
                    },
                    dataSovereignty = dataSovereigntySettings,
                    safetySettings = safetySettings,
                    parentalControls = isMinor ? new ParentalControlSettings
                    {
                        parentEmail = request.ParentalEmail,
                        consentVerifiedAt = DateTime.UtcNow,
                        notificationLevel = age < 16 ? "all" : "important",
                        requiresOversight = age < 16
                    } : null
                };

                // 9. Store user based on data residency preference
                var createdUser = await _userRepository.CreateOrUpdateAsync(
                    userDocument,
                    Guid.NewGuid().ToString(),
                    cancellationToken);

                _logger.LogInformation("User registered successfully: {UserId}, Age: {Age}, IsMinor: {IsMinor}", 
                    Guid.NewGuid().ToString(), age, isMinor);

                return Result.SuccessResult(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
                return Result.Failure("An error occurred during registration. Please try again.");
            }
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (DateTime.UtcNow.DayOfYear < birthDate.DayOfYear) age--;
            return age;
        }

        private static DataSovereigntySettings CreateAgeAppropriateDataSettings(int age, DataResidencyPreference preference)
        {
            return new DataSovereigntySettings
            {
                preferredRegion = preference.ToString().ToLowerInvariant(),
                dataResidency = age < 18 ? "local_first" : preference.ToString().ToLowerInvariant(),
                aiProcessingLocation = age < 16 ? "local_only" : age < 18 ? "local_preferred" : "hybrid",
                encryptionRequired = age < 18,
                thirdPartyDataSharing = false, // Always false for privacy
                dataRetentionDays = age < 18 ? 30 : 365 // Shorter retention for minors
            };
        }

        private static UserSafetySettings CreateAgeAppropriateSafetySettings(int age)
        {
            return new UserSafetySettings
            {
                maxContentLevel = age switch
                {
                    < 13 => "safe",
                    < 16 => "mild", 
                    < 18 => "moderate",
                    _ => "adult"
                },
                contentFilteringEnabled = true,
                safeModeEnabled = age < 18,
                parentalNotificationsEnabled = age < 18,
                restrictedTopics = age < 18 ? new List<string> 
                { 
                    "adult_content", 
                    "explicit_relationships",
                    age < 16 ? "dating" : null!,
                    age < 13 ? "personal_health" : null! 
                }.Where(t => t != null).ToList() : new List<string>(),
                allowedInteractionTypes = age < 13 ? new List<string> 
                { 
                    "educational", 
                    "family_safe", 
                    "creative" 
                } : new List<string>()
            };
        }
    }

    // 5. Endpoint (HTTP API)
    [ApiController]
    [Route("api/users")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="command">User registration data</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Result), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 409)]
        public async Task<ActionResult<Result>> Register([FromBody] Command command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("already exists") == true)
                {
                    return Conflict(result);
                }

                if (result.ValidationErrors != null)
                {
                    return BadRequest(result);
                }

                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetUser.Endpoint.GetById),
                new { id = result.UserId },
                result);
        }
    }
}

/// <summary>
/// Data residency preference enumeration
/// </summary>
public enum DataResidencyPreference
{
    /// <summary>
    /// Store data locally on user's device/private cloud
    /// </summary>
    Local = 0,

    /// <summary>
    /// Store data in European Union
    /// </summary>
    EU = 1,

    /// <summary>
    /// Store data in United States
    /// </summary>
    US = 2,

    /// <summary>
    /// Store data in user's region (auto-detect)
    /// </summary>
    UserRegion = 3,

    /// <summary>
    /// Hybrid approach: sensitive data local, other data cloud
    /// </summary>
    Hybrid = 4
}