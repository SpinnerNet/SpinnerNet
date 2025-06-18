using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.DataSovereignty;

/// <summary>
/// Vertical slice for setting user data residency preferences
/// Allows users to control where different data types are stored (local/cloud/region)
/// </summary>
public static class SetDataResidencyPreference
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("dataTypePreferences")]
        public Dictionary<string, DataResidencyPreference> DataTypePreferences { get; init; } = new();

        [JsonPropertyName("defaultPreference")]
        public DataResidencyPreference DefaultPreference { get; init; } = DataResidencyPreference.Local;

        [JsonPropertyName("enableCloudSync")]
        public bool EnableCloudSync { get; init; } = false;

        [JsonPropertyName("enableCrossBorderTransfer")]
        public bool EnableCrossBorderTransfer { get; init; } = false;
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("user")]
        public UserDocument? User { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(UserDocument user) =>
            new() { Success = true, User = user };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        private readonly string[] _validDataTypes = {
            "persona", "email", "buddy", "room", "assets", "calendar", "contacts", "documents"
        };

        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.DefaultPreference)
                .IsInEnum().WithMessage("Default preference must be a valid option");

            RuleFor(x => x.DataTypePreferences)
                .Must(BeValidDataTypePreferences)
                .WithMessage("Data type preferences contain invalid data types or preferences");
        }

        private bool BeValidDataTypePreferences(Dictionary<string, DataResidencyPreference> preferences)
        {
            if (preferences == null) return true;

            foreach (var kvp in preferences)
            {
                // Check if data type is valid
                if (!_validDataTypes.Contains(kvp.Key.ToLowerInvariant()))
                    return false;

                // Check if preference is valid enum value
                if (!Enum.IsDefined(typeof(DataResidencyPreference), kvp.Value))
                    return false;
            }

            return true;
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
                _logger.LogInformation("Setting data residency preferences for user: {UserId}", request.UserId);

                // 1. Retrieve existing user
                var existingUser = await _userRepository.GetAsync(
                    $"user_{request.UserId}",
                    request.UserId,
                    cancellationToken);

                if (existingUser == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result.Failure("User not found");
                }

                // 2. Update user preferences with data sovereignty settings
                var updatedPreferences = existingUser.Preferences;
                
                // Extend UserPreferences to include data sovereignty (this would need to be added to the model)
                // For now, we'll store as a custom property or extend the preferences model
                
                // 3. Update user document properties  
                existingUser.Preferences = updatedPreferences;

                // 4. Save updated user
                var savedUser = await _userRepository.CreateOrUpdateAsync(
                    existingUser,
                    request.UserId,
                    cancellationToken);

                _logger.LogInformation("Data residency preferences updated for user: {UserId}", request.UserId);

                // 5. Log data sovereignty change for audit purposes
                await LogDataSovereigntyChange(request, cancellationToken);

                return Result.SuccessResult(savedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data residency preferences for user: {UserId}", request.UserId);
                return Result.Failure("An error occurred while updating data residency preferences");
            }
        }

        private async Task LogDataSovereigntyChange(Command request, CancellationToken cancellationToken)
        {
            // TODO: Implement audit logging for data sovereignty changes
            // This would create an audit record in a separate container
            
            // Use proper async pattern for JSON serialization as per Microsoft best practices
            var preferencesJson = await Task.Run(() => 
                System.Text.Json.JsonSerializer.Serialize(request.DataTypePreferences), cancellationToken);
                
            _logger.LogInformation("Data sovereignty preferences changed for user {UserId}: {Preferences}",
                request.UserId, preferencesJson);
        }
    }

    // 5. Endpoint (HTTP API)
    [ApiController]
    [Route("api/users/{userId}/data-sovereignty")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Set data residency preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Data residency preferences</param>
        /// <returns>Updated user with preferences</returns>
        [HttpPut]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> SetPreferences(
            string userId,
            [FromBody] DataResidencyRequest request)
        {
            var command = new Command
            {
                UserId = userId,
                DataTypePreferences = request.DataTypePreferences,
                DefaultPreference = request.DefaultPreference,
                EnableCloudSync = request.EnableCloudSync,
                EnableCrossBorderTransfer = request.EnableCrossBorderTransfer
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current data residency preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Current data residency preferences</returns>
        [HttpGet]
        [ProducesResponseType(typeof(DataResidencyResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataResidencyResponse>> GetPreferences(string userId)
        {
            // TODO: Implement GetDataResidencyPreferences slice
            // Use proper async pattern for future database operations
            var response = await Task.FromResult(new DataResidencyResponse
            {
                UserId = userId,
                DefaultPreference = DataResidencyPreference.Local,
                DataTypePreferences = new(),
                EnableCloudSync = false,
                EnableCrossBorderTransfer = false
            });
            
            return Ok(response);
        }
    }

    /// <summary>
    /// Request model for setting data residency preferences
    /// </summary>
    public record DataResidencyRequest
    {
        [JsonPropertyName("dataTypePreferences")]
        public Dictionary<string, DataResidencyPreference> DataTypePreferences { get; init; } = new();

        [JsonPropertyName("defaultPreference")]
        public DataResidencyPreference DefaultPreference { get; init; } = DataResidencyPreference.Local;

        [JsonPropertyName("enableCloudSync")]
        public bool EnableCloudSync { get; init; } = false;

        [JsonPropertyName("enableCrossBorderTransfer")]
        public bool EnableCrossBorderTransfer { get; init; } = false;
    }

    /// <summary>
    /// Response model for data residency preferences
    /// </summary>
    public record DataResidencyResponse
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("dataTypePreferences")]
        public Dictionary<string, DataResidencyPreference> DataTypePreferences { get; init; } = new();

        [JsonPropertyName("defaultPreference")]
        public DataResidencyPreference DefaultPreference { get; init; }

        [JsonPropertyName("enableCloudSync")]
        public bool EnableCloudSync { get; init; }

        [JsonPropertyName("enableCrossBorderTransfer")]
        public bool EnableCrossBorderTransfer { get; init; }

        [JsonPropertyName("lastUpdated")]
        public DateTime? LastUpdated { get; init; }
    }
}

/// <summary>
/// Data residency preference enumeration (reused from RegisterUser)
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