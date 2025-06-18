using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Users;

/// <summary>
/// Vertical slice for retrieving user by ID
/// </summary>
public static class GetUser
{
    // 1. Query (Input)
    public record Query : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;
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

        public static Result SuccessResult(UserDocument user) =>
            new() { Success = true, User = user };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };
    }

    // 3. Validator (Input Validation)
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }

    // 4. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Query, Result>
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

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetAsync(
                    $"user_{request.UserId}",
                    request.UserId,
                    cancellationToken);

                if (user == null)
                {
                    return Result.Failure("User not found");
                }

                return Result.SuccessResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {UserId}", request.UserId);
                return Result.Failure("An error occurred while retrieving the user");
            }
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
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> GetById(string id)
        {
            var result = await _mediator.Send(new Query { UserId = id });

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}