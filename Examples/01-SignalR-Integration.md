# SignalR Integration Example

This example demonstrates how to integrate SignalR for real-time communication between JavaScript clients and C# hubs in a .NET 9 Blazor Server application.

## Overview

SignalR enables real-time bidirectional communication, essential for the hybrid AI architecture where:
- JavaScript WebLLM sends AI inference results to the server
- C# Semantic Kernel orchestrates workflows and sends responses back
- Real-time updates flow seamlessly between client and server

## Key Components

### 1. Client-Side JavaScript Integration

**File:** `wwwroot/js/webllm-integration.js`

```javascript
class HybridWebLLMIntegration {
    constructor() {
        this.signalRConnection = null;
        this.isHybridMode = false;
    }

    async initWithSemanticKernel(hubUrl) {
        try {
            console.log("🔗 Connecting to Semantic Kernel hub:", hubUrl);
            
            // Check if SignalR is available
            if (typeof window.signalR === 'undefined') {
                throw new Error("SignalR client library not found");
            }
            
            // Create connection
            this.signalRConnection = new window.signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
                .withAutomaticReconnect()
                .build();
                
            // Set up event handlers
            this.signalRConnection.on("PersonaTraitsExtracted", (traits) => {
                console.log("✅ Persona traits received from SK:", traits);
                if (window.blazorPersonaCallback) {
                    window.blazorPersonaCallback(traits);
                }
            });
            
            this.signalRConnection.on("Error", (error) => {
                console.error("❌ Hub error:", error);
            });
            
            // Start connection
            await this.signalRConnection.start();
            console.log("✅ Connected to Semantic Kernel hub");
            
            this.isHybridMode = true;
        } catch (error) {
            console.error("❌ Failed to connect to SK hub:", error);
            this.isHybridMode = false;
        }
    }

    async executeInterviewStep(sessionId, userInput) {
        if (!this.isHybridMode || !this.signalRConnection) {
            return await this.generateInterviewResponse(userInput, 0, 4);
        }
        
        try {
            // Get optimized prompt from Semantic Kernel
            const prompt = await this.signalRConnection.invoke(
                "GetNextPrompt", 
                sessionId, 
                userInput
            );
            
            // Execute with WebLLM for ultra-low latency
            const response = await this.engine.chat.completions.create({
                messages: [
                    { role: "system", content: prompt },
                    { role: "user", content: userInput }
                ],
                temperature: 0.7,
                max_tokens: 200
            });
            
            // Send insights back to SK for processing
            await this.signalRConnection.invoke(
                "SaveInsights", 
                sessionId, 
                response.choices[0].message.content
            );
            
            return response.choices[0].message.content;
        } catch (error) {
            console.error("❌ Error in hybrid interview step:", error);
            return await this.generateInterviewResponse(userInput, 0, 4);
        }
    }
}
```

### 2. Server-Side C# Hub

**File:** `Hubs/AIHub.cs`

```csharp
[Authorize]
public class AIHub : Hub
{
    private readonly AIOrchestrationService _orchestrationService;
    private readonly ILogger<AIHub> _logger;

    public AIHub(
        AIOrchestrationService orchestrationService,
        ILogger<AIHub> logger)
    {
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the next interview prompt based on conversation history
    /// </summary>
    public async Task<string> GetNextPrompt(string sessionId, string? previousResponse)
    {
        try
        {
            // Store previous response if provided
            if (!string.IsNullOrEmpty(previousResponse))
            {
                await _orchestrationService.StoreUserResponseAsync(sessionId, previousResponse);
            }

            // Generate next prompt using Semantic Kernel
            var nextPrompt = await _orchestrationService.GenerateNextPromptAsync(sessionId, previousResponse ?? string.Empty);
            
            _logger.LogInformation("Generated prompt for session {SessionId}", sessionId);
            return nextPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next prompt for session {SessionId}", sessionId);
            return "I'm having trouble generating the next question. Could you tell me more about what you're looking for?";
        }
    }

    /// <summary>
    /// Processes insights from the client-side WebLLM and extracts persona traits
    /// </summary>
    public async Task SaveInsights(string sessionId, string insights)
    {
        try
        {
            _logger.LogInformation("Processing insights for session {SessionId}", sessionId);
            
            // Extract persona traits using Semantic Kernel
            var traits = await _orchestrationService.ExtractPersonaTraitsAsync(sessionId, insights);
            
            // Send extracted traits back to the client
            await Clients.Caller.SendAsync("PersonaTraitsExtracted", traits);
            
            _logger.LogInformation("Persona traits extracted for session {SessionId}: {PersonalityType}", 
                sessionId, traits.PersonalityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing insights for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("Error", "Failed to process insights");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
```

### 3. SignalR Configuration in Program.cs

```csharp
// Add SignalR services
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Map SignalR hubs
app.MapHub<SpinnerNet.App.Hubs.AIHub>("/aihub");
```

### 4. Including SignalR JavaScript Client

**File:** `Components/App.razor`

```html
<script src="_framework/blazor.web.js"></script>
<script src="@Assets["_content/MudBlazor/MudBlazor.min.js"]"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js"></script>
<script src="@Assets["js/webllm-integration.js"]"></script>
```

## Implementation Steps

### Step 1: Add SignalR JavaScript Client

Choose the appropriate method:

**Option A: CDN (Recommended for .NET 9)**
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js"></script>
```

**Option B: npm Package**
```bash
npm install @microsoft/signalr
```

**Option C: LibMan**
```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    {
      "library": "@microsoft/signalr@8.0.7",
      "destination": "wwwroot/lib/signalr/",
      "files": [
        "dist/browser/signalr.js",
        "dist/browser/signalr.min.js"
      ]
    }
  ]
}
```

### Step 2: Create SignalR Hub

```csharp
[Authorize]  // Ensure only authenticated users can connect
public class AIHub : Hub
{
    // Hub methods for client-server communication
    public async Task<string> GetNextPrompt(string sessionId, string previousResponse)
    {
        // Implementation here
    }
    
    public async Task SaveInsights(string sessionId, string insights)
    {
        // Implementation here
    }
}
```

### Step 3: Configure Services and Routing

```csharp
// In Program.cs
builder.Services.AddSignalR();
app.MapHub<AIHub>("/aihub");
```

### Step 4: Implement JavaScript Integration

```javascript
// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/aihub")
    .withAutomaticReconnect()
    .build();

// Set up event handlers
connection.on("PersonaTraitsExtracted", (traits) => {
    console.log("Received traits:", traits);
});

// Start connection
await connection.start();

// Invoke server methods
const result = await connection.invoke("GetNextPrompt", sessionId, userInput);
```

## Testing and Verification

### 1. Console Verification

```javascript
// Check SignalR availability
console.log("SignalR available:", typeof window.signalR !== 'undefined');
console.log("SignalR version:", window.signalR.VERSION);

// Test connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/aihub")
    .build();

connection.start().then(() => {
    console.log("✅ SignalR connected successfully");
}).catch(err => {
    console.error("❌ SignalR connection failed:", err);
});
```

### 2. Server-Side Logging

```csharp
public override async Task OnConnectedAsync()
{
    _logger.LogInformation("✅ Client connected: {ConnectionId}", Context.ConnectionId);
    await base.OnConnectedAsync();
}

public override async Task OnDisconnectedAsync(Exception? exception)
{
    _logger.LogInformation("❌ Client disconnected: {ConnectionId}", Context.ConnectionId);
    await base.OnDisconnectedAsync(exception);
}
```

### 3. Network Tab Monitoring

Monitor the browser's Network tab for:
- WebSocket connection establishment
- SignalR handshake completion
- Real-time message exchanges

## Common Issues and Solutions

### Issue: SignalR Client Not Found

**Problem:** `❌ SignalR not found. Please ensure SignalR client library is included.`

**Solution:** Verify the script tag order and CDN availability:
```html
<!-- Correct order -->
<script src="_framework/blazor.web.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js"></script>
<script src="js/your-integration.js"></script>
```

### Issue: Connection Failed

**Problem:** SignalR connection fails to establish

**Solutions:**
1. Check authentication requirements
2. Verify hub endpoint mapping
3. Check CORS configuration for development
4. Monitor server logs for connection errors

### Issue: Version Compatibility

**Problem:** SignalR client version mismatch

**Solution:** Use compatible versions:
- .NET 9: SignalR 8.0.7+ (backwards compatible)
- .NET 8: SignalR 8.0.x
- .NET 7: SignalR 7.0.x

## Performance Considerations

### Connection Management

```javascript
// Implement connection retry logic
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/aihub")
    .withAutomaticReconnect([0, 2000, 10000, 30000])  // Retry intervals
    .build();

// Handle reconnection events
connection.onreconnecting(() => {
    console.log("🔄 SignalR reconnecting...");
});

connection.onreconnected(() => {
    console.log("✅ SignalR reconnected");
});
```

### Message Size Optimization

```csharp
// Configure message size limits
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
    options.StreamBufferCapacity = 10;
});
```

## Production Deployment

### Azure App Service Configuration

```json
{
  "webSocketsEnabled": true,
  "alwaysOn": true
}
```

### HTTPS Requirements

SignalR requires HTTPS in production:
```csharp
app.UseHttpsRedirection();  // Ensure HTTPS
app.MapHub<AIHub>("/aihub");
```

## Result

✅ **Real-time bidirectional communication established**  
✅ **JavaScript ↔ C# integration working**  
✅ **Ultra-low latency message exchange**  
✅ **Production-ready SignalR implementation**  

**Live Demo:** https://spinnernet-app-3lauxg.azurewebsites.net/ai-interview-hybrid

The SignalR integration enables seamless communication between client-side WebLLM and server-side Semantic Kernel, forming the backbone of the hybrid AI architecture.