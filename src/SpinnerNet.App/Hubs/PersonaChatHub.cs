using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using SpinnerNet.Core.Services.AI;
using System.Security.Claims;

namespace SpinnerNet.App.Hubs
{
    [Authorize]
    public class PersonaChatHub : Hub
    {
        private readonly PersonaChatService _chatService;
        private readonly ILogger<PersonaChatHub> _logger;

        public PersonaChatHub(PersonaChatService chatService, ILogger<PersonaChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                var userId = GetUserId();
                
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "User authentication failed. Please log in again.");
                    return;
                }

                _logger.LogInformation("Processing message from user {UserId}", userId);
                
                var response = await _chatService.ProcessMessageAsync(userId, message);
                
                await Clients.Caller.SendAsync("ReceiveMessage", response.Message, response.PersonaProgress);
                
                if (response.PersonaProgress >= 100)
                {
                    await Clients.Caller.SendAsync("PersonaComplete", "Your persona has been successfully created!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                await Clients.Caller.SendAsync("ReceiveMessage", 
                    "I'm having trouble understanding. Could you try rephrasing?", 0);
            }
        }

        public async Task StartPersonaDiscovery()
        {
            try
            {
                var userId = GetUserId();
                
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "User authentication failed. Please log in again.");
                    return;
                }

                _logger.LogInformation("Starting persona discovery for user {UserId}", userId);
                
                var greeting = "Hi! I'm your Spinner.Net buddy 🕷️ I'm here to help create your unique digital persona. " +
                              "Let's have a casual chat about what brings you here and what you're hoping to achieve. " +
                              "What inspired you to join Spinner.Net today?";
                
                await Clients.Caller.SendAsync("ReceiveMessage", greeting, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting persona discovery");
                await Clients.Caller.SendAsync("ReceiveError", "Failed to start persona discovery. Please try again.");
            }
        }

        public async Task PauseConversation()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Pausing conversation for user {UserId}", userId);
                
                await Clients.Caller.SendAsync("ConversationPaused", 
                    "Your conversation has been paused. You can continue anytime!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing conversation");
            }
        }

        public async Task ResumeConversation()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Resuming conversation for user {UserId}", userId);
                
                await Clients.Caller.SendAsync("ConversationResumed", 
                    "Welcome back! Let's continue where we left off.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming conversation");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} connected to PersonaChatHub", userId);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} disconnected from PersonaChatHub", userId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier) ?? 
                              Context.User?.FindFirst("sub") ??
                              Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            return userIdClaim?.Value ?? Context.UserIdentifier ?? string.Empty;
        }
    }
}