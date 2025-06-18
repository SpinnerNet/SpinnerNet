using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data;
using SpinnerNet.Shared.Models;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Users;

/// <summary>
/// Vertical slice for user registration with Entity Framework (Sprint 1 SQLite version)
/// Implements: Command → Validation → Handler → Endpoint pattern
/// </summary>
public static class RegisterUserEF
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
        public string DataResidencyPreference { get; init; } = "Local";

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

        [JsonPropertyName("userId")]
        public string? UserId { get; init; }

        [JsonPropertyName("user")]
        public User? User { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(User user) =>
            new() { Success = true, UserId = user.Id, User = user };

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
                .NotEmpty().WithMessage("Data residency preference is required")
                .Must(BeValidDataResidencyPreference).WithMessage("Data residency preference must be a valid option");

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

        private static bool BeValidDataResidencyPreference(string preference)
        {
            var validPreferences = new[] { "Local", "EU", "US", "UserRegion", "Hybrid" };
            return validPreferences.Contains(preference);
        }
    }

    // 4. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly SpinnerNetDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(
            SpinnerNetDbContext dbContext,
            ILogger<Handler> logger)
        {
            _dbContext = dbContext;
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
                var existingUser = await _dbContext.SpinnerNetUsers
                    .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
                    return Result.Failure("A user with this email address already exists");
                }

                // 4. Generate user ID
                var userId = $"user_{Guid.NewGuid():N}";

                // 5. Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 6. Create age-appropriate data sovereignty settings
                var dataSovereigntySettings = CreateAgeAppropriateDataSettings(age, request.DataResidencyPreference);

                // 7. Create age-appropriate safety settings
                var safetySettings = CreateAgeAppropriateSafetySettings(age);

                // 8. Create parental controls for minors
                var parentalControls = isMinor ? CreateParentalControls(age, request.ParentalEmail) : null;

                // 9. Create user entity
                var user = new User
                {
                    Id = userId,
                    Email = request.Email.ToLowerInvariant(),
                    DisplayName = request.DisplayName,
                    PasswordHash = passwordHash,
                    BirthDate = request.BirthDate,
                    Age = age,
                    IsMinor = isMinor,
                    PreferredLanguage = request.Language,
                    TimeZone = request.Timezone,
                    DataResidencyPreference = request.DataResidencyPreference,
                    DataSovereigntySettings = dataSovereigntySettings,
                    SafetySettings = safetySettings,
                    ParentalControls = parentalControls,
                    ParentalEmail = isMinor ? request.ParentalEmail : null,
                    ParentalConsentVerifiedAt = isMinor ? DateTime.UtcNow : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 10. Save to database
                _dbContext.SpinnerNetUsers.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User registered successfully: {UserId}, Age: {Age}, IsMinor: {IsMinor}", 
                    userId, age, isMinor);

                return Result.SuccessResult(user);
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

        private static DataSovereigntySettings CreateAgeAppropriateDataSettings(int age, string preference)
        {
            return new DataSovereigntySettings
            {
                PreferredRegion = preference.ToLowerInvariant(),
                DataResidency = age < 18 ? "local_first" : preference.ToLowerInvariant(),
                AiProcessingLocation = age < 16 ? "local_only" : age < 18 ? "local_preferred" : "hybrid",
                EncryptionRequired = age < 18,
                ThirdPartyDataSharing = false, // Always false for privacy
                DataRetentionDays = age < 18 ? 30 : 365 // Shorter retention for minors
            };
        }

        private static SafetySettings CreateAgeAppropriateSafetySettings(int age)
        {
            return new SafetySettings
            {
                MaxContentLevel = age switch
                {
                    < 13 => "safe",
                    < 16 => "mild", 
                    < 18 => "moderate",
                    _ => "adult"
                },
                ContentFilteringEnabled = true,
                SafeModeEnabled = age < 18,
                ParentalNotificationsEnabled = age < 18,
                RestrictedTopics = age < 18 ? new List<string> 
                { 
                    "adult_content", 
                    "explicit_relationships",
                    age < 16 ? "dating" : null!,
                    age < 13 ? "personal_health" : null! 
                }.Where(t => t != null).ToList() : new List<string>(),
                AllowedInteractionTypes = age < 13 ? new List<string> 
                { 
                    "educational", 
                    "family_safe", 
                    "creative" 
                } : new List<string>()
            };
        }

        private static ParentalControls CreateParentalControls(int age, string? parentEmail)
        {
            return new ParentalControls
            {
                ParentEmail = parentEmail,
                ConsentVerifiedAt = DateTime.UtcNow,
                NotificationLevel = age < 16 ? "all" : "important",
                RequiresOversight = age < 16,
                AllowedContentCategories = new List<string> 
                { 
                    "educational", 
                    "entertainment", 
                    age >= 13 ? "social" : null! 
                }.Where(c => c != null).ToList()
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
        /// Register a new user (Sprint 1 SQLite version)
        /// </summary>
        /// <param name="command">User registration data</param>
        /// <returns>Registration result</returns>
        [HttpPost("register-ef")]
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
                nameof(Register),
                new { id = result.UserId },
                result);
        }
    }
}