# API Specification - Spinner.Net

## Overview

Spinner.Net provides a comprehensive RESTful API for the Zeitsparkasse™ time management platform with AI buddy companions. The API follows vertical slice architecture principles with consistent patterns across all endpoints.

## Base Configuration

### Base URL
```
Production: https://api.spinner.net/v1
Development: https://localhost:5001/api
```

### Authentication
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### Common Response Format
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2024-01-01T12:00:00Z",
  "requestId": "req_123456789"
}
```

### Error Response Format
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Input validation failed",
    "details": {
      "userId": ["User ID is required"],
      "email": ["Invalid email format"]
    }
  },
  "timestamp": "2024-01-01T12:00:00Z",
  "requestId": "req_123456789"
}
```

## User Management

### Register User
```http
POST /api/users/register
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "displayName": "John Doe",
  "birthDate": "1995-06-15T00:00:00Z",
  "language": "en",
  "timezone": "Europe/Berlin",
  "dataResidencyPreference": "local",
  "acceptTerms": true,
  "acceptPrivacyPolicy": true,
  "acceptDataProcessing": true,
  "parentalEmail": "parent@example.com",
  "parentalConsentToken": "consent_token_123",
  "isParentalConsentVerified": true
}
```

**Note:** For users under 18, `parentalEmail`, `parentalConsentToken`, and `isParentalConsentVerified` are required.

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "email": "user@example.com",
    "displayName": "John Doe",
    "age": 28,
    "isMinor": false,
    "isEmailVerified": false,
    "verificationToken": "verify_token_123",
    "preferences": {
      "defaultLanguage": "en",
      "timezone": "Europe/Berlin"
    },
    "dataSovereignty": {
      "preferredRegion": "local",
      "dataResidency": "local_first",
      "aiProcessingLocation": "hybrid",
      "encryptionRequired": false
    },
    "safetySettings": {
      "maxContentLevel": "adult",
      "contentFilteringEnabled": true,
      "safeModeEnabled": false,
      "parentalNotificationsEnabled": false
    },
    "parentalControls": null
  }
}
```

**For minor users (under 18), the response includes:**
```json
{
  "parentalControls": {
    "parentEmail": "parent@example.com",
    "consentVerifiedAt": "2024-01-01T12:00:00Z",
    "notificationLevel": "important",
    "requiresOversight": true
  },
  "safetySettings": {
    "maxContentLevel": "moderate",
    "safeModeEnabled": true,
    "parentalNotificationsEnabled": true
  }
}
```

### Get User Profile
```http
GET /api/users/{userId}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "email": "user@example.com",
    "displayName": "John Doe",
    "isEmailVerified": true,
    "preferences": {
      "defaultLanguage": "en",
      "timezone": "Europe/Berlin",
      "theme": "light"
    },
    "subscription": {
      "plan": "free",
      "features": ["basic_ai", "local_storage"]
    },
    "dataSovereignty": {
      "preferredRegion": "europe",
      "dataResidency": "local_first",
      "aiProcessing": "hybrid"
    },
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

## Persona Management

### Start Persona Interview
```http
POST /api/personas/interview/start
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "language": "en"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "sessionId": "interview_session_123",
    "welcomeQuestion": "Hello! I'm excited to get to know you. Let's start with something simple - what's your name and what do you do for work?",
    "expectedQuestions": 8,
    "estimatedMinutes": 5,
    "supportedLanguages": ["en", "de", "es"],
    "privacyNotice": "Your responses help create a personalized AI companion. All data remains under your control."
  }
}
```

### Process Interview Response
```http
POST /api/personas/interview/respond
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "sessionId": "interview_session_123",
  "response": "Hi! I'm John, I work as a software developer at a tech startup. I'm really into photography and traveling when I have time.",
  "language": "en"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "sessionId": "interview_session_123",
    "nextQuestion": "That's great, John! Photography and travel - what exciting combination. How do you typically organize your time between work projects and your hobbies?",
    "extractedInfo": {
      "name": "John",
      "occupation": "Software Developer",
      "interests": ["photography", "travel", "technology"],
      "workEnvironment": "startup"
    },
    "progressPercentage": 25,
    "questionsRemaining": 6,
    "isComplete": false
  }
}
```

### Complete Persona Interview
```http
POST /api/personas/interview/complete
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "sessionId": "interview_session_123",
  "finalResponse": "I usually try to batch my coding work and then take photography trips on weekends. Time management is definitely something I want to improve.",
  "customizations": {
    "uiComplexity": "Advanced",
    "enableAnimations": true,
    "preferredBuddyType": "DailyCompanion"
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "personaId": "persona_123456789",
    "persona": {
      "basicInfo": {
        "age": 28,
        "occupation": "Software Developer",
        "interests": ["photography", "travel", "technology"],
        "timeZone": "Europe/Berlin",
        "languages": {
          "preferred": "en",
          "supported": ["en", "de"]
        }
      },
      "typeLeapConfig": {
        "uiComplexityLevel": "Advanced",
        "fontSizePreferences": "medium",
        "enableAnimations": true
      },
      "privacySettings": {
        "emailAccess": "None",
        "calendarAccess": "None",
        "aiProcessingLocation": "local_preferred"
      }
    },
    "recommendedBuddies": [
      {
        "type": "DailyCompanion",
        "name": "Alex",
        "description": "A friendly daily companion for work-life balance",
        "capabilities": ["task_management", "goal_tracking"]
      },
      {
        "type": "CreativePartner", 
        "name": "Luna",
        "description": "Perfect for organizing photography projects",
        "capabilities": ["project_planning", "creative_workflows"]
      }
    ]
  }
}
```

## Task Management (Zeitsparkasse™)

### Create Task from Natural Language
```http
POST /api/tasks/create
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "personaId": "persona_123456789",
  "input": "Remind me to call mom tomorrow at 7pm about weekend plans",
  "language": "en",
  "timezone": "Europe/Berlin",
  "context": {
    "currentLocation": "home",
    "currentActivity": "planning",
    "deviceType": "mobile"
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "taskId": "task_123456789",
    "task": {
      "title": "Call mom about weekend plans",
      "description": "Remind me to call mom tomorrow at 7pm about weekend plans",
      "originalInput": "Remind me to call mom tomorrow at 7pm about weekend plans",
      "status": "pending",
      "priority": "medium",
      "category": "family",
      "tags": ["family", "call", "tomorrow", "person"],
      "dueDate": "2024-01-02T19:00:00Z",
      "estimatedMinutes": 30,
      "createdAt": "2024-01-01T12:00:00Z"
    },
    "aiInsights": {
      "extractedTitle": "Call mom about weekend plans",
      "detectedDueDate": "2024-01-02T19:00:00Z",
      "detectedPriority": "medium",
      "suggestedCategory": "family",
      "estimatedDurationMinutes": 30,
      "detectedEntities": ["person:mom"],
      "suggestedTags": ["family", "call", "person"],
      "confidence": 0.92,
      "processingTime": "00:00:00.150"
    },
    "suggestedActions": [
      {
        "type": "set_reminder",
        "title": "Set Reminder",
        "description": "Get notified before this task is due",
        "actionData": {
          "taskId": "task_123456789",
          "suggestedTime": "2024-01-02T18:00:00Z"
        }
      }
    ]
  }
}
```

### Get User Tasks
```http
GET /api/tasks?status=pending&category=family&limit=20&offset=0
```

**Query Parameters:**
- `status`: pending, in_progress, completed, cancelled (optional)
- `category`: family, work, health, etc. (optional)
- `priority`: low, medium, high, urgent (optional)
- `dueAfter`: ISO date string (optional)
- `dueBefore`: ISO date string (optional)
- `limit`: number of results (default: 20, max: 100)
- `offset`: pagination offset (default: 0)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "tasks": [
      {
        "taskId": "task_123456789",
        "title": "Call mom about weekend plans",
        "status": "pending",
        "priority": "medium",
        "category": "family",
        "dueDate": "2024-01-02T19:00:00Z",
        "estimatedMinutes": 30,
        "createdAt": "2024-01-01T12:00:00Z",
        "tags": ["family", "call"]
      }
    ],
    "pagination": {
      "total": 1,
      "limit": 20,
      "offset": 0,
      "hasMore": false
    },
    "summary": {
      "pendingCount": 1,
      "inProgressCount": 0,
      "completedToday": 3,
      "overdueCount": 0
    }
  }
}
```

### Update Task
```http
PUT /api/tasks/{taskId}
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "status": "completed",
  "actualMinutes": 25,
  "completionNotes": "Had a great conversation about the family reunion plans"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "taskId": "task_123456789",
    "task": {
      "title": "Call mom about weekend plans",
      "status": "completed",
      "completedAt": "2024-01-02T19:25:00Z",
      "actualMinutes": 25,
      "estimatedMinutes": 30,
      "timeSavings": 5,
      "completionNotes": "Had a great conversation about the family reunion plans"
    },
    "timeAnalytics": {
      "estimationAccuracy": 0.83,
      "completionStreak": 4,
      "categoryProductivity": {
        "family": {
          "averageCompletion": 27,
          "estimationAccuracy": 0.85
        }
      }
    }
  }
}
```

## AI Buddy System

### Create Buddy
```http
POST /api/buddies/create
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "personaId": "persona_123456789",
  "buddyType": "DailyCompanion",
  "language": "en",
  "customization": {
    "name": "Alex",
    "personalityArchetype": "Helpful",
    "communicationTone": "friendly",
    "formality": 0.3,
    "proactiveness": 0.7,
    "useEmojis": true,
    "useHumor": false
  },
  "requestedCapabilities": ["task_management", "email_management"]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "buddyId": "buddy_123456789",
    "buddy": {
      "buddyType": "DailyCompanion",
      "basicInfo": {
        "name": "Alex",
        "avatar": "friendly-companion",
        "description": "Your friendly daily companion for all aspects of life"
      },
      "personality": {
        "archetype": "Helpful",
        "traits": {
          "friendliness": 0.9,
          "professionalism": 0.5,
          "proactiveness": 0.7,
          "formality": 0.3
        },
        "communicationStyle": {
          "tone": "friendly",
          "verbosity": "normal",
          "useEmojis": true,
          "useHumor": false
        }
      },
      "capabilities": {
        "taskManagement": {
          "enabled": true,
          "permissions": ["view_tasks", "create_tasks", "update_tasks"]
        },
        "emailManagement": {
          "enabled": false,
          "permissions": []
        }
      }
    },
    "onboardingTips": [
      {
        "title": "Say Hello",
        "description": "Start by introducing yourself to Alex. They're excited to meet you!",
        "action": "start_chat",
        "priority": 1
      },
      {
        "title": "Create Your First Task",
        "description": "Try saying 'Remind me to call my mom tomorrow' to see how AI understands natural language",
        "action": "create_task",
        "priority": 2
      }
    ],
    "suggestedFirstActions": [
      {
        "type": "chat",
        "title": "Start a Conversation",
        "description": "Begin chatting with your buddy to see their personality in action",
        "example": "Hello! How can you help me today?"
      }
    ]
  }
}
```

### Chat with Buddy
```http
POST /api/buddies/chat
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "buddyId": "buddy_123456789",
  "message": "Hey Alex! I'm feeling overwhelmed with my tasks today. Can you help me prioritize?",
  "conversationId": "conv_123456789",
  "messageType": "question",
  "context": {
    "currentLocation": "office",
    "timeOfDay": "morning",
    "currentMood": "stressed",
    "deviceType": "desktop"
  },
  "streamResponse": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "conversationId": "conv_123456789",
    "messageId": "msg_123456789",
    "buddyResponse": {
      "message": "I understand you're feeling overwhelmed! Let me help you organize your day. I can see you have 8 pending tasks. Should we start by looking at what's due today and what's most important? 😊",
      "emotion": "supportive",
      "responseType": "actionRequired",
      "confidence": 0.95,
      "processingTime": "00:00:01.250",
      "usedCapabilities": ["task_management"],
      "followUpQuestions": [
        "Would you like me to show your tasks by priority?",
        "Should we break down any large tasks into smaller steps?",
        "How much time do you have available today?"
      ]
    },
    "conversationState": {
      "messageCount": 12,
      "currentTopic": "task_prioritization",
      "buddyMood": "helpful",
      "lastActiveAt": "2024-01-01T10:15:00Z"
    },
    "suggestedActions": [
      {
        "type": "view_tasks",
        "title": "View My Tasks",
        "action": "show_task_list",
        "priority": 1
      },
      {
        "type": "prioritize_tasks",
        "title": "Help Me Prioritize",
        "action": "run_prioritization",
        "priority": 2
      }
    ]
  }
}
```

## Data Sovereignty

### Set Data Residency Preference
```http
POST /api/data-sovereignty/residency
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "dataType": "tasks",
  "preferredLocation": "local_first",
  "region": "europe",
  "encryptionRequired": true,
  "aiProcessingLocation": "local_preferred"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "dataType": "tasks",
    "currentLocation": "local_first",
    "region": "europe",
    "encryptionEnabled": true,
    "aiProcessingLocation": "local_preferred",
    "appliedAt": "2024-01-01T12:00:00Z",
    "affectedDocuments": 127,
    "migrationStatus": "completed"
  }
}
```

### Get Data Locations
```http
GET /api/data-sovereignty/locations
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "preferences": {
      "tasks": {
        "location": "local_first",
        "region": "europe",
        "encrypted": true
      },
      "emails": {
        "location": "local_only",
        "region": "europe",
        "encrypted": true
      },
      "conversations": {
        "location": "hybrid",
        "region": "europe", 
        "encrypted": false
      }
    },
    "availableRegions": ["europe", "us", "asia"],
    "storageOptions": ["local_only", "local_first", "hybrid", "cloud_preferred", "cloud_only"],
    "encryptionSupported": true,
    "localStorageUsed": "2.4 GB",
    "cloudStorageUsed": "1.1 GB"
  }
}
```

## Content Safety and Age Protection

### Analyze Content Safety
```http
POST /api/content-safety/analyze
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "content": "Let's discuss relationship advice for teenagers",
  "contentType": "text",
  "context": {
    "source": "buddy_chat",
    "messageType": "user_input"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "contentId": "content_123456789",
    "classification": {
      "riskLevel": "moderate",
      "categories": ["relationships", "advice"],
      "confidence": 0.92,
      "flags": ["age_sensitive"]
    },
    "safetyAction": "allow_with_warning",
    "warningMessage": "This content discusses relationship topics appropriate for users 16+",
    "isAgeAppropriate": true,
    "alternatives": [
      {
        "title": "General life advice",
        "description": "Broader life guidance topics",
        "contentLevel": "safe"
      }
    ]
  }
}
```

### Get User Safety Settings
```http
GET /api/content-safety/settings/{userId}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "user_123456789",
    "age": 16,
    "isMinor": true,
    "safetySettings": {
      "maxContentLevel": "moderate",
      "contentFilteringEnabled": true,
      "safeModeEnabled": true,
      "restrictedTopics": ["adult_content", "explicit_relationships"],
      "allowedInteractionTypes": ["educational", "family_safe", "age_appropriate"]
    },
    "parentalControls": {
      "notificationLevel": "important",
      "requiresOversight": false,
      "timeRestrictions": [
        {
          "dayOfWeek": 1,
          "startTime": "07:00",
          "endTime": "21:00",
          "isActive": true
        }
      ]
    }
  }
}
```

### Update Safety Settings (18+ only)
```http
PUT /api/content-safety/settings/{userId}
```

**Request Body:**
```json
{
  "maxContentLevel": "adult",
  "contentFilteringEnabled": false,
  "safeModeEnabled": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "updated": true,
    "appliedAt": "2024-01-01T12:00:00Z",
    "requiresVerification": true,
    "verificationMethod": "enhanced_age_verification"
  }
}
```

### Report Content Concern
```http
POST /api/content-safety/report
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "contentId": "content_123456789",
  "reportType": "inappropriate_for_age",
  "description": "Content seems too mature for my age group",
  "severity": "medium"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "reportId": "report_123456789",
    "status": "submitted",
    "reviewExpectedBy": "2024-01-02T12:00:00Z",
    "parentNotified": true,
    "immediateAction": "content_quarantined"
  }
}
```

### Parental Controls (Parent/Guardian Access)

#### Get Child Safety Report
```http
GET /api/content-safety/parental/report/{childUserId}
```

**Headers:**
```http
Authorization: Bearer {parent_jwt_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "childUserId": "user_123456789",
    "reportPeriod": "week",
    "contentInteractions": {
      "totalMessages": 156,
      "filteredContent": 8,
      "warningsShown": 3,
      "inappropriateContent": 0
    },
    "safetyIncidents": [],
    "topicBreakdown": [
      {
        "category": "educational",
        "percentage": 65,
        "safe": true
      },
      {
        "category": "social",
        "percentage": 30,
        "safe": true
      },
      {
        "category": "relationships",
        "percentage": 5,
        "safe": true,
        "note": "Age-appropriate guidance only"
      }
    ],
    "recommendations": [
      {
        "type": "setting_adjustment",
        "title": "Consider enabling advanced content filtering",
        "reason": "Based on recent interaction patterns"
      }
    ]
  }
}
```

#### Update Parental Controls
```http
PUT /api/content-safety/parental/controls/{childUserId}
```

**Request Body:**
```json
{
  "notificationLevel": "all",
  "contentFilterLevel": "strict",
  "allowedCategories": ["educational", "family_safe", "creative"],
  "timeRestrictions": [
    {
      "dayOfWeek": 1,
      "startTime": "16:00",
      "endTime": "20:00",
      "isActive": true
    }
  ],
  "buddyInteractionLimits": {
    "maxSessionMinutes": 60,
    "cooldownMinutes": 30,
    "requireApprovalForNewTopics": true
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "updated": true,
    "appliedAt": "2024-01-01T12:00:00Z",
    "childNotified": true,
    "effectiveImmediately": true
  }
}
```

## Email Integration

### Connect Email Account
```http
POST /api/email/accounts/connect
```

**Request Body:**
```json
{
  "userId": "user_123456789",
  "provider": "gmail",
  "connectionType": "oauth2",
  "roomId": "room_work",
  "permissions": ["read", "categorize"],
  "syncSettings": {
    "frequency": "real_time",
    "historicalDays": 30,
    "autoCreateTasks": true
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "accountId": "email_account_123",
    "provider": "gmail",
    "email": "john@company.com",
    "status": "connected",
    "roomId": "room_work",
    "permissions": ["read", "categorize"],
    "syncSettings": {
      "frequency": "real_time",
      "lastSync": "2024-01-01T12:00:00Z",
      "emailsProcessed": 0
    },
    "oauthRedirectUrl": "https://accounts.google.com/oauth/authorize?client_id=..."
  }
}
```

### Get Processed Emails
```http
GET /api/email/processed?roomId=room_work&category=important&limit=20
```

**Query Parameters:**
- `roomId`: specific room filter (optional)
- `category`: work, personal, urgent, etc. (optional)
- `hasActionItems`: true/false (optional)
- `processed`: true/false (optional)
- `limit`: number of results (default: 20)
- `offset`: pagination offset (default: 0)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "emails": [
      {
        "emailId": "email_123456789",
        "subject": "Project Update - Q1 Review",
        "from": {
          "email": "manager@company.com",
          "name": "Sarah Manager"
        },
        "receivedAt": "2024-01-01T09:00:00Z",
        "roomId": "room_work",
        "aiAnalysis": {
          "category": "work",
          "priority": "high",
          "actionItems": ["Review Q1 metrics", "Prepare presentation", "Schedule team meeting"],
          "sentiment": "neutral",
          "suggestedTasks": [
            {
              "title": "Review Q1 metrics for project update",
              "priority": "high",
              "estimatedMinutes": 60,
              "dueDate": "2024-01-03T17:00:00Z"
            }
          ],
          "confidence": 0.89
        },
        "isRead": false,
        "tasksCreated": 1
      }
    ],
    "pagination": {
      "total": 15,
      "limit": 20,
      "offset": 0,
      "hasMore": false
    },
    "summary": {
      "unreadCount": 3,
      "actionItemsCount": 8,
      "tasksCreated": 5
    }
  }
}
```

## Analytics and Insights

### Get Time Analytics
```http
GET /api/analytics/time?period=week&category=all
```

**Query Parameters:**
- `period`: day, week, month, year
- `category`: work, family, health, etc. (optional)
- `startDate`: ISO date string (optional)
- `endDate`: ISO date string (optional)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "period": "week",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-07T23:59:59Z",
    "timeSavings": {
      "totalSaved": 240,
      "averagePerTask": 15,
      "bestCategory": "work",
      "improvementTrend": 0.23
    },
    "productivity": {
      "tasksCompleted": 28,
      "completionRate": 0.87,
      "averageCompletionTime": 32,
      "estimationAccuracy": 0.81
    },
    "categoryBreakdown": [
      {
        "category": "work",
        "tasksCompleted": 15,
        "timeSpent": 420,
        "timeSaved": 95,
        "efficiency": 0.82
      },
      {
        "category": "family",
        "tasksCompleted": 8,
        "timeSpent": 180,
        "timeSaved": 85,
        "efficiency": 0.89
      }
    ],
    "weeklyTrend": [
      {"day": "Monday", "tasksCompleted": 6, "timeSaved": 45},
      {"day": "Tuesday", "tasksCompleted": 4, "timeSaved": 35}
    ]
  }
}
```

### Get AI Insights
```http
GET /api/analytics/insights
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "personalInsights": [
      {
        "type": "productivity_pattern",
        "title": "Peak Productivity Hours",
        "description": "You're most productive between 9-11 AM, completing tasks 40% faster",
        "confidence": 0.92,
        "actionSuggestion": "Schedule important tasks during morning hours"
      },
      {
        "type": "time_estimation",
        "title": "Estimation Improvement",
        "description": "Your time estimates have improved 23% over the last month",
        "confidence": 0.87,
        "actionSuggestion": "Continue tracking actual vs. estimated time"
      }
    ],
    "recommendations": [
      {
        "type": "task_batching",
        "title": "Batch Similar Tasks",
        "description": "Grouping email tasks together could save 30 minutes daily",
        "impact": "high",
        "effort": "low"
      }
    ],
    "zeitCoinProjection": {
      "currentRate": 15.2,
      "projectedDaily": 45.6,
      "projectedWeekly": 319.2,
      "readyForLaunch": "2026-Q3"
    }
  }
}
```

## Error Codes

### Client Errors (4xx)
- `400 BAD_REQUEST`: Invalid request format
- `401 UNAUTHORIZED`: Authentication required
- `403 FORBIDDEN`: Insufficient permissions
- `404 NOT_FOUND`: Resource not found
- `409 CONFLICT`: Resource conflict
- `422 VALIDATION_ERROR`: Input validation failed
- `429 RATE_LIMITED`: Too many requests

### Server Errors (5xx)
- `500 INTERNAL_ERROR`: Unexpected server error
- `503 SERVICE_UNAVAILABLE`: Service temporarily unavailable
- `507 STORAGE_FULL`: User storage quota exceeded

### Spinner.Net Specific Errors
- `PERSONA_INCOMPLETE`: User persona setup required
- `BUDDY_NOT_FOUND`: AI buddy not configured
- `AI_PROVIDER_UNAVAILABLE`: AI service unavailable
- `DATA_SOVEREIGNTY_VIOLATION`: Request violates user privacy settings
- `SYNC_CONFLICT`: Data synchronization conflict
- `CONTENT_BLOCKED`: Content blocked by safety filters
- `AGE_VERIFICATION_REQUIRED`: Enhanced age verification needed
- `PARENTAL_CONSENT_REQUIRED`: Parental consent missing or invalid
- `CONTENT_TOO_MATURE`: Content exceeds user's age-appropriate level
- `SAFETY_VIOLATION`: Content violates platform safety policies
- `PARENTAL_OVERRIDE_REQUIRED`: Parent/guardian approval needed
- `TIME_RESTRICTION_ACTIVE`: User access restricted by parental controls

## Rate Limiting

### Limits by Endpoint Category
- **Authentication**: 5 requests/minute
- **User Operations**: 60 requests/minute
- **Task Management**: 100 requests/minute
- **Buddy Chat**: 30 requests/minute
- **Email Processing**: 20 requests/minute
- **Content Safety**: 50 requests/minute
- **Parental Controls**: 20 requests/minute
- **Analytics**: 10 requests/minute

### Headers
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1640995200
X-RateLimit-RetryAfter: 60
```

## Versioning

### API Versions
- **v1.0**: Current stable version
- **v1.1**: ZeitCoin integration (planned Q3 2026)
- **v2.0**: Multi-tenant support (planned 2027)

### Backward Compatibility
- Breaking changes require new major version
- Deprecated endpoints supported for 12 months
- Migration guides provided for version updates

This API specification provides comprehensive access to all Spinner.Net platform capabilities while maintaining consistency, security, and user data sovereignty.