# Email Integration Architecture - Spinner.Net

## Overview

Spinner.Net's email integration transforms inbox chaos into structured time management through the Zeitsparkasse™ system. The architecture supports multiple email providers, room-based organization, AI-powered task extraction, and respects user data sovereignty preferences.

## Core Principles

### 1. **Data Sovereignty First**
- Users control where email data is stored and processed
- Local-first processing with optional cloud synchronization
- Granular privacy controls per email account

### 2. **Room-Based Organization**
- Emails organized into contextual "rooms" (Work, Personal, Projects)
- AI automatically categorizes emails into appropriate rooms
- Users can customize room rules and behaviors

### 3. **AI-Powered Automation**
- Natural language processing for task extraction
- Smart categorization and priority detection
- Context-aware suggestions and actions

### 4. **Multi-Provider Support**
- IMAP/POP3 for traditional providers (GMX, web.de, T-Online)
- OAuth2 for modern providers (Gmail, Outlook)
- Universal API abstraction layer

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Email Integration Layer                   │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   IMAP/POP3     │  │    OAuth2       │  │   Exchange      │ │
│  │   Connector     │  │   Connector     │  │   Connector     │ │
│  │                 │  │                 │  │                 │ │
│  │ • GMX          │  │ • Gmail         │  │ • Office 365    │ │
│  │ • web.de       │  │ • Outlook.com   │  │ • On-Premise    │ │
│  │ • T-Online     │  │ • Yahoo         │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                    Email Processing Pipeline                     │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  Fetch → Parse → Analyze → Categorize → Extract → Store     │ │
│  └─────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                      Room Management                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   Work Room     │  │  Personal Room  │  │  Project Room   │ │
│  │                 │  │                 │  │                 │ │
│  │ • Colleagues    │  │ • Family        │  │ • Clients       │ │
│  │ • Meetings      │  │ • Friends       │  │ • Deadlines     │ │
│  │ • Projects      │  │ • Services      │  │ • Milestones    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                    AI Analysis Engine                           │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  Task Extraction → Priority Detection → Deadline Analysis   │ │
│  │  Contact Recognition → Context Linking → Action Suggestions │ │
│  └─────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                    Zeitsparkasse Integration                    │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │     Email → Task Creation → Buddy Notifications → Time      │ │
│  │     Archive → Goal Linking → Progress Tracking → Savings    │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Email Provider Support

### 1. **IMAP/POP3 Providers** (Priority 1 - German Market)

#### Configuration Examples
```json
{
  "providers": {
    "gmx": {
      "type": "imap",
      "servers": {
        "imap": "imap.gmx.net:993",
        "smtp": "mail.gmx.net:587"
      },
      "security": "ssl",
      "authentication": "login"
    },
    "web_de": {
      "type": "imap", 
      "servers": {
        "imap": "imap.web.de:993",
        "smtp": "smtp.web.de:587"
      },
      "security": "ssl",
      "authentication": "login"
    },
    "t_online": {
      "type": "imap",
      "servers": {
        "imap": "secureimap.t-online.de:993",
        "smtp": "securesmtp.t-online.de:465"
      },
      "security": "ssl",
      "authentication": "login"
    }
  }
}
```

### 2. **OAuth2 Providers**

#### Gmail Integration
```csharp
public class GmailConnector : IEmailConnector
{
    public async Task<AuthenticationResult> AuthenticateAsync(string clientId, string redirectUri)
    {
        var request = new AuthorizationCodeRequestUrl(
            "https://accounts.google.com/o/oauth2/auth",
            clientId,
            redirectUri,
            "https://www.googleapis.com/auth/gmail.readonly"
        );
        
        return await ProcessOAuthFlowAsync(request);
    }
}
```

#### Microsoft Graph Integration
```csharp
public class OutlookConnector : IEmailConnector
{
    public async Task<AuthenticationResult> AuthenticateAsync(string clientId, string tenantId)
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();
            
        return await app.AcquireTokenForClient(scopes).ExecuteAsync();
    }
}
```

## Data Models

### EmailDocument
```csharp
public class EmailDocument
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "email";
    public string UserId { get; set; } = string.Empty;
    public string EmailId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    
    // Email Content
    public string Subject { get; set; } = string.Empty;
    public EmailAddress From { get; set; } = new();
    public List<EmailAddress> To { get; set; } = new();
    public List<EmailAddress> Cc { get; set; } = new();
    public List<EmailAddress> Bcc { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string BodyType { get; set; } = "text"; // text, html
    public List<EmailAttachment> Attachments { get; set; } = new();
    
    // Metadata
    public DateTime SentAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public bool IsRead { get; set; } = false;
    public bool IsFlagged { get; set; } = false;
    public List<string> Labels { get; set; } = new();
    
    // AI Analysis
    public EmailAIAnalysis AIAnalysis { get; set; } = new();
    
    // Data Sovereignty
    public EmailDataSovereignty DataSovereignty { get; set; } = new();
    
    public long Timestamp { get; set; }
}

public class EmailAIAnalysis
{
    public string Category { get; set; } = "general";
    public EmailPriority DetectedPriority { get; set; } = EmailPriority.Normal;
    public List<string> ActionItems { get; set; } = new();
    public List<ExtractedTask> SuggestedTasks { get; set; } = new();
    public string Sentiment { get; set; } = "neutral";
    public List<string> ExtractedEntities { get; set; } = new();
    public double Confidence { get; set; } = 1.0;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string ProcessingProvider { get; set; } = "local";
}

public class EmailDataSovereignty
{
    public string StorageLocation { get; set; } = "local"; // local, hybrid, cloud
    public bool ProcessedLocally { get; set; } = true;
    public bool CloudSyncEnabled { get; set; } = false;
    public string EncryptionKey { get; set; } = string.Empty;
    public List<string> ProcessingHistory { get; set; } = new();
}
```

### RoomDocument
```csharp
public class RoomDocument
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "room";
    public string UserId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    
    // Room Configuration
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078d4";
    public string Icon { get; set; } = "inbox";
    public RoomType Type { get; set; } = RoomType.Personal;
    
    // Auto-categorization Rules
    public List<EmailRule> Rules { get; set; } = new();
    
    // Connected Email Accounts
    public List<ConnectedEmailAccount> ConnectedAccounts { get; set; } = new();
    
    // AI Buddy Assignment
    public string? AssignedBuddyId { get; set; }
    public RoomBuddySettings BuddySettings { get; set; } = new();
    
    // Statistics
    public RoomStatistics Statistics { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public long Timestamp { get; set; }
}
```

## Email Processing Pipeline

### 1. **Email Fetching Service**
```csharp
public class EmailFetchingService
{
    public async Task<List<EmailDocument>> FetchNewEmailsAsync(
        string userId, 
        string accountId, 
        CancellationToken cancellationToken)
    {
        var account = await GetEmailAccountAsync(userId, accountId);
        var connector = _connectorFactory.CreateConnector(account.Provider);
        
        var newEmails = await connector.FetchNewEmailsAsync(account, cancellationToken);
        
        var emailDocuments = new List<EmailDocument>();
        foreach (var email in newEmails)
        {
            var document = await ProcessEmailAsync(email, userId, cancellationToken);
            emailDocuments.Add(document);
        }
        
        return emailDocuments;
    }
    
    private async Task<EmailDocument> ProcessEmailAsync(
        RawEmail email, 
        string userId, 
        CancellationToken cancellationToken)
    {
        // 1. Parse email content
        var parsedEmail = await _emailParser.ParseAsync(email);
        
        // 2. Determine room assignment
        var roomId = await _roomAssignmentService.AssignToRoomAsync(parsedEmail, userId);
        
        // 3. Run AI analysis
        var aiAnalysis = await _aiAnalysisService.AnalyzeEmailAsync(parsedEmail, userId);
        
        // 4. Check data sovereignty preferences
        var dataSovereignty = await _dataSovereigntyService.GetEmailPreferencesAsync(userId);
        
        // 5. Create document
        var document = new EmailDocument
        {
            Id = $"email_{userId}_{Guid.NewGuid():N}",
            UserId = userId,
            RoomId = roomId,
            // ... populate other fields
            AIAnalysis = aiAnalysis,
            DataSovereignty = dataSovereignty
        };
        
        // 6. Store based on sovereignty preferences
        await _emailRepository.CreateOrUpdateAsync(document, userId, cancellationToken);
        
        // 7. Generate tasks if needed
        await _taskGenerationService.GenerateTasksFromEmailAsync(document, cancellationToken);
        
        return document;
    }
}
```

### 2. **AI Analysis Service**
```csharp
public class EmailAIAnalysisService
{
    public async Task<EmailAIAnalysis> AnalyzeEmailAsync(
        ParsedEmail email, 
        string userId,
        CancellationToken cancellationToken)
    {
        var userPreferences = await _userPreferencesService.GetAsync(userId);
        var aiProvider = await _aiRoutingService.SelectProviderAsync(
            userId, 
            "email_analysis", 
            userPreferences.DataSovereignty);
        
        var analysis = new EmailAIAnalysis();
        
        // 1. Category detection
        analysis.Category = await DetectCategoryAsync(email, aiProvider);
        
        // 2. Priority assessment
        analysis.DetectedPriority = await AssessPriorityAsync(email, aiProvider);
        
        // 3. Action item extraction
        analysis.ActionItems = await ExtractActionItemsAsync(email, aiProvider);
        
        // 4. Task suggestions
        analysis.SuggestedTasks = await GenerateTaskSuggestionsAsync(email, aiProvider);
        
        // 5. Sentiment analysis
        analysis.Sentiment = await AnalyzeSentimentAsync(email, aiProvider);
        
        // 6. Entity recognition
        analysis.ExtractedEntities = await ExtractEntitiesAsync(email, aiProvider);
        
        analysis.ProcessingProvider = aiProvider.Name;
        analysis.ProcessedAt = DateTime.UtcNow;
        
        return analysis;
    }
    
    private async Task<List<ExtractedTask>> GenerateTaskSuggestionsAsync(
        ParsedEmail email, 
        IAIProvider aiProvider)
    {
        var prompt = $"""
        Analyze this email and suggest actionable tasks:
        
        Subject: {email.Subject}
        From: {email.From.Email}
        Body: {email.Body}
        
        Extract specific, actionable tasks that the recipient should take.
        Consider deadlines, priorities, and required actions.
        Format as JSON array with title, priority, estimatedMinutes, and dueDate.
        """;
        
        var response = await aiProvider.GenerateResponseAsync(prompt);
        return JsonSerializer.Deserialize<List<ExtractedTask>>(response);
    }
}
```

## Room Assignment Algorithm

### 1. **Rule-Based Assignment**
```csharp
public class RoomAssignmentService
{
    public async Task<string> AssignToRoomAsync(ParsedEmail email, string userId)
    {
        var rooms = await _roomRepository.GetUserRoomsAsync(userId);
        
        // Check explicit rules first
        foreach (var room in rooms.OrderBy(r => r.Rules.Count))
        {
            if (await EvaluateRulesAsync(email, room.Rules))
            {
                await UpdateRoomStatisticsAsync(room.RoomId, email);
                return room.RoomId;
            }
        }
        
        // Fall back to AI-based classification
        var aiClassification = await _aiClassificationService.ClassifyEmailAsync(email, rooms);
        return aiClassification.RecommendedRoomId;
    }
    
    private async Task<bool> EvaluateRulesAsync(ParsedEmail email, List<EmailRule> rules)
    {
        foreach (var rule in rules)
        {
            var matches = rule.Condition switch
            {
                "from_contains" => email.From.Email.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
                "subject_contains" => email.Subject.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
                "body_contains" => email.Body.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
                "domain_equals" => email.From.Email.EndsWith($"@{rule.Value}", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
            
            if (rule.Operator == "AND" && !matches) return false;
            if (rule.Operator == "OR" && matches) return true;
        }
        
        return true; // All AND conditions met
    }
}
```

### 2. **Machine Learning Classification**
```csharp
public class AIEmailClassificationService
{
    public async Task<ClassificationResult> ClassifyEmailAsync(
        ParsedEmail email, 
        List<RoomDocument> availableRooms)
    {
        var features = ExtractEmailFeatures(email);
        var roomDescriptions = availableRooms.Select(r => new 
        { 
            Id = r.RoomId, 
            Name = r.Name, 
            Description = r.Description 
        });
        
        var prompt = $"""
        Classify this email into the most appropriate room:
        
        Email Features:
        - Subject: {email.Subject}
        - From: {email.From.Email} ({email.From.Name})
        - Body Preview: {email.Body.Substring(0, Math.Min(500, email.Body.Length))}
        - Time: {email.SentAt}
        
        Available Rooms:
        {JsonSerializer.Serialize(roomDescriptions, new JsonSerializerOptions { WriteIndented = true })}
        
        Consider:
        1. Sender domain and relationship
        2. Subject line keywords
        3. Content context and urgency
        4. Time and frequency patterns
        
        Respond with JSON: {{ "roomId": "room_id", "confidence": 0.95, "reasoning": "explanation" }}
        """;
        
        var response = await _aiProvider.GenerateResponseAsync(prompt);
        return JsonSerializer.Deserialize<ClassificationResult>(response);
    }
}
```

## Task Generation from Emails

### 1. **Automatic Task Creation**
```csharp
public class EmailToTaskService
{
    public async Task GenerateTasksFromEmailAsync(
        EmailDocument email, 
        CancellationToken cancellationToken)
    {
        if (!email.AIAnalysis.SuggestedTasks.Any()) return;
        
        foreach (var suggestedTask in email.AIAnalysis.SuggestedTasks)
        {
            // Create task using the CreateTask vertical slice
            var createTaskCommand = new CreateTask.Command
            {
                UserId = email.UserId,
                Input = suggestedTask.Title,
                Language = await GetUserLanguageAsync(email.UserId),
                Context = new TaskContext
                {
                    RelatedGoals = new[] { email.AIAnalysis.Category },
                    CurrentActivity = "email_processing"
                }
            };
            
            var taskResult = await _mediator.Send(createTaskCommand, cancellationToken);
            
            if (taskResult.Success)
            {
                // Link task back to email
                await LinkTaskToEmailAsync(email.EmailId, taskResult.TaskId, cancellationToken);
                
                // Notify user's buddy if they have one
                await NotifyBuddyOfNewTaskAsync(email.UserId, taskResult.Task, cancellationToken);
            }
        }
    }
}
```

### 2. **Interactive Task Confirmation**
```csharp
public class InteractiveTaskConfirmationService
{
    public async Task<TaskConfirmationResult> RequestTaskConfirmationAsync(
        string userId,
        EmailDocument email,
        List<ExtractedTask> suggestedTasks)
    {
        // Send notification to user's buddy for interactive confirmation
        var buddy = await _buddyService.GetActiveBuddyAsync(userId);
        if (buddy == null) return TaskConfirmationResult.AutoApprove();
        
        var confirmationMessage = GenerateConfirmationMessage(email, suggestedTasks, buddy.Language);
        
        // This would trigger a buddy chat message
        var chatCommand = new ChatWithBuddy.Command
        {
            UserId = userId,
            BuddyId = buddy.BuddyId,
            Message = confirmationMessage,
            MessageType = MessageType.TaskRequest,
            Context = new ChatContext
            {
                CurrentActivity = "email_task_confirmation",
                RecentTasks = suggestedTasks.Select(t => t.Title).ToList()
            }
        };
        
        // The buddy will respond with task creation suggestions
        var chatResult = await _mediator.Send(chatCommand);
        
        return new TaskConfirmationResult
        {
            UserNotified = true,
            BuddyResponseId = chatResult.MessageId,
            RequiresUserInput = true
        };
    }
}
```

## Data Sovereignty Implementation

### 1. **Local-First Storage**
```csharp
public class EmailDataSovereigntyService
{
    public async Task<EmailDocument> StoreEmailAsync(
        EmailDocument email, 
        DataSovereigntyPreferences preferences)
    {
        switch (preferences.EmailStorage)
        {
            case "local_only":
                await _localEmailRepository.CreateAsync(email);
                break;
                
            case "local_first":
                await _localEmailRepository.CreateAsync(email);
                if (preferences.CloudSyncEnabled)
                {
                    await _syncQueue.EnqueueForSyncAsync(email);
                }
                break;
                
            case "hybrid":
                // Store locally and in cloud simultaneously
                await Task.WhenAll(
                    _localEmailRepository.CreateAsync(email),
                    _cloudEmailRepository.CreateAsync(email)
                );
                break;
                
            case "cloud_only":
                await _cloudEmailRepository.CreateAsync(email);
                break;
        }
        
        return email;
    }
}
```

### 2. **Encryption and Privacy**
```csharp
public class EmailEncryptionService
{
    public async Task<EmailDocument> EncryptSensitiveDataAsync(
        EmailDocument email, 
        EncryptionPreferences preferences)
    {
        if (preferences.EncryptBody)
        {
            email.Body = await _encryptionService.EncryptAsync(
                email.Body, 
                preferences.UserKey);
        }
        
        if (preferences.EncryptAttachments)
        {
            foreach (var attachment in email.Attachments)
            {
                attachment.Content = await _encryptionService.EncryptAsync(
                    attachment.Content, 
                    preferences.UserKey);
            }
        }
        
        // Mark as encrypted
        email.DataSovereignty.EncryptionKey = preferences.KeyId;
        
        return email;
    }
}
```

## Performance Optimizations

### 1. **Incremental Sync**
```csharp
public class IncrementalEmailSyncService
{
    public async Task SyncEmailsAsync(string userId, string accountId)
    {
        var lastSync = await _syncStateRepository.GetLastSyncAsync(userId, accountId);
        var connector = _connectorFactory.CreateConnector(accountId);
        
        // Only fetch emails since last sync
        var newEmails = await connector.FetchEmailsSinceAsync(lastSync.Timestamp);
        
        // Process in batches to avoid memory issues
        const int batchSize = 50;
        for (int i = 0; i < newEmails.Count; i += batchSize)
        {
            var batch = newEmails.Skip(i).Take(batchSize);
            await ProcessEmailBatchAsync(batch, userId);
        }
        
        // Update sync state
        await _syncStateRepository.UpdateLastSyncAsync(userId, accountId, DateTime.UtcNow);
    }
}
```

### 2. **Background Processing**
```csharp
public class EmailBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Get all users with email accounts configured
                var usersWithEmail = await _userRepository.GetUsersWithEmailAccountsAsync();
                
                foreach (var user in usersWithEmail)
                {
                    await _emailSyncService.SyncUserEmailsAsync(user.UserId, stoppingToken);
                }
                
                // Wait before next sync cycle
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in email background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
```

## Security Considerations

### 1. **Credential Management**
- OAuth tokens stored encrypted in Azure Key Vault
- IMAP/POP3 passwords encrypted at rest
- Automatic token refresh for OAuth providers
- Secure credential rotation policies

### 2. **Data Protection**
- Email content encrypted in transit and at rest
- User-controlled encryption keys
- Secure deletion capabilities
- GDPR compliance for data export/deletion

### 3. **Access Control**
- Room-based permission system
- Buddy access limited by user preferences
- Audit logging for all email operations
- Rate limiting for API endpoints

## Monitoring and Analytics

### 1. **Performance Metrics**
- Email processing latency
- AI analysis accuracy
- Sync success rates
- User engagement with generated tasks

### 2. **Usage Analytics**
- Room assignment accuracy
- Task generation effectiveness
- User interaction patterns
- Feature adoption rates

This email integration architecture provides a robust, privacy-respecting foundation for transforming email management into an intelligent time-saving system within the Zeitsparkasse™ platform.