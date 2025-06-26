/*
 * COMMENTED OUT FOR SPRINT 1 - USING COSMOS DB INSTEAD OF ENTITY FRAMEWORK
 * This Entity Framework version is for future use when SQL database is needed
 * Currently using RegisterUser.cs with Cosmos DB
 */

/*
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

        [JsonPropertyName("acceptTerms")]
        public bool AcceptTerms { get; init; }

        [JsonPropertyName("acceptPrivacyPolicy")]
        public bool AcceptPrivacyPolicy { get; init; }

        // Parental consent for minors
        [JsonPropertyName("parentalEmail")]
        public string? ParentalEmail { get; init; }

        [JsonPropertyName("parentalConsentToken")]
        public string? ParentalConsentToken { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("userId")]
        public string? UserId { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; init; }

        public static Result SuccessResult(string userId) =>
            new() { Success = true, UserId = userId };

        public static Result Failure(string message) =>
            new() { Success = false, Message = message };

        public static Result ValidationFailure(Dictionary<string, string[]> errors) =>
            new() { Success = false, Errors = errors };
    }

    // 3. Validator
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .Length(2, 100).WithMessage("Display name must be between 2 and 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.BirthDate)
                .Must(BeAValidAge).WithMessage("You must be at least 13 years old to register");

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("You must accept the terms of service");

            RuleFor(x => x.AcceptPrivacyPolicy)
                .Equal(true).WithMessage("You must accept the privacy policy");

            // Parental consent required for minors
            RuleFor(x => x.ParentalEmail)
                .NotEmpty()
                .EmailAddress()
                .When(x => IsMinor(x.BirthDate))
                .WithMessage("Parental consent email is required for users under 18");
        }

        private bool BeAValidAge(DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
            return age >= 13;
        }

        private bool IsMinor(DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
            return age < 18;
        }
    }

    // 4. Handler (Business Logic)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly SpinnerNetDbContext _dbContext;
        private readonly ILogger<Handler> _logger;

        public Handler(SpinnerNetDbContext dbContext, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if (existingUser != null)
                {
                    return Result.Failure("An account with this email already exists");
                }

                // Calculate age
                var age = DateTime.Today.Year - request.BirthDate.Year;
                if (request.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    BirthDate = request.BirthDate,
                    Age = age,
                    IsMinor = age < 18,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true,
                    HasCompletedOnboarding = false,
                    AcceptedTermsAt = DateTime.UtcNow,
                    AcceptedPrivacyPolicyAt = DateTime.UtcNow
                };

                // Handle parental consent for minors
                if (user.IsMinor)
                {
                    user.RequiresParentalConsent = true;
                    user.ParentalConsentEmail = request.ParentalEmail;
                    user.ParentalConsentToken = Guid.NewGuid().ToString();
                    user.ParentalConsentRequestedAt = DateTime.UtcNow;
                    
                    // TODO: Send parental consent email
                    _logger.LogInformation("Parental consent required for user {UserId}. Token: {Token}", 
                        user.Id, user.ParentalConsentToken);
                }

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} registered successfully", user.Id);
                return Result.SuccessResult(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user");
                return Result.Failure("An error occurred during registration");
            }
        }
    }

    // 5. Endpoint (Controller)
    public static class Endpoint
    {
        public static async Task<IActionResult> Handle(
            [FromBody] Command command,
            [FromServices] IMediator mediator,
            [FromServices] IValidator<Command> validator,
            CancellationToken cancellationToken)
        {
            // Validate
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                return new BadRequestObjectResult(Result.ValidationFailure(errors));
            }

            // Handle
            var result = await mediator.Send(command, cancellationToken);
            
            return result.Success 
                ? new OkObjectResult(result)
                : new BadRequestObjectResult(result);
        }
    }
}
*/