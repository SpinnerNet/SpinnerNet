using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Personas;

/// <summary>
/// Vertical slice for checking if a user has any personas
/// Simplified implementation for Sprint 1 - basic existence check
/// </summary>
public static class CheckUserHasPersona
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("hasPersona")]
        public bool HasPersona { get; init; }

        [JsonPropertyName("personaCount")]
        public int PersonaCount { get; init; }

        [JsonPropertyName("defaultPersona")]
        public PersonaDocument? DefaultPersona { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        public static Result SuccessResult(bool hasPersona, int count = 0, PersonaDocument? defaultPersona = null) =>
            new() { Success = true, HasPersona = hasPersona, PersonaCount = count, DefaultPersona = defaultPersona };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };
    }

    // 3. Validator
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");
        }
    }

    // 4. Handler (Simplified Implementation)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<PersonaDocument> _personaRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(ICosmosRepository<PersonaDocument> personaRepository, ILogger<Handler> logger)
        {
            _personaRepository = personaRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🔍 Checking if user has personas: {UserId}", request.UserId);

                // Simple implementation - query for personas by UserId
                var personas = await _personaRepository.QueryAsync(
                    p => p.UserId == request.UserId,
                    cancellationToken: cancellationToken);

                var personaList = personas.ToList();
                var hasPersona = personaList.Any();
                var defaultPersona = personaList.FirstOrDefault(p => p.isDefault) ?? personaList.FirstOrDefault();

                _logger.LogInformation("✅ User {UserId} has {Count} personas", request.UserId, personaList.Count);

                return Result.SuccessResult(hasPersona, personaList.Count, defaultPersona);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error checking personas for user {UserId}", request.UserId);
                return Result.Failure($"Error checking personas: {ex.Message}");
            }
        }
    }

    // 5. Endpoint
    [ApiController]
    [Route("api/personas")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Check if user has any personas
        /// </summary>
        [HttpGet("user/{userId}/exists")]
        public async Task<ActionResult<Result>> CheckUserHasPersona([FromRoute] string userId)
        {
            var command = new Command { UserId = userId };
            var result = await _mediator.Send(command);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}