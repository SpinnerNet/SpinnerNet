using SpinnerNet.Shared.DTOs;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.Personas.Services;

/// <summary>
/// Service for managing user personas
/// </summary>
public interface IPersonaService
{
    /// <summary>
    /// Creates a new persona from onboarding data
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Onboarding request data</param>
    /// <returns>The created persona</returns>
    Task<UserPersona> CreatePersonaFromOnboardingAsync(string userId, OnboardingRequest request);
    
    /// <summary>
    /// Gets a persona by ID
    /// </summary>
    /// <param name="personaId">The persona ID</param>
    /// <returns>The persona if found, null otherwise</returns>
    Task<UserPersona?> GetPersonaAsync(int personaId);
    
    /// <summary>
    /// Gets the active persona for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The active persona if found, null otherwise</returns>
    Task<UserPersona?> GetActivePersonaAsync(string userId);
    
    /// <summary>
    /// Updates a persona
    /// </summary>
    /// <param name="persona">The persona to update</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdatePersonaAsync(UserPersona persona);
    
    /// <summary>
    /// Sets a persona as active for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="personaId">The persona ID to set as active</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetActivePersonaAsync(string userId, int personaId);
    
    /// <summary>
    /// Adapts UI configuration based on persona attributes
    /// </summary>
    /// <param name="personaId">The persona ID</param>
    /// <returns>UI configuration adapted to the persona</returns>
    Task<PersonaUIConfiguration> GetPersonaUIConfigurationAsync(int personaId);
}