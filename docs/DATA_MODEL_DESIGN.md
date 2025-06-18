# Data Model Design for Spinner.Net

## Overview

Spinner.Net uses Azure Cosmos DB with a document-based NoSQL approach optimized for the Zeitsparkasse™ time management system and AI buddy companions. The design prioritizes user data sovereignty, multi-language support, and efficient querying patterns.

## Core Design Principles

### 1. **User-Centric Partitioning**
- All documents are partitioned by `/userId`
- Ensures data locality and sovereignty
- Enables efficient user-scoped queries
- Supports data residency preferences

### 2. **Document Type Discrimination**
- Each document has a `type` field for polymorphic queries
- Enables multiple document types in same container
- Simplifies queries and indexing strategies

### 3. **Embedded vs Referenced Data**
- **Embedded**: Frequently accessed together (persona preferences, buddy personality)
- **Referenced**: Independent lifecycle (tasks, conversations)
- **Hybrid**: Memory references for AI context

### 4. **Multi-Language First**
- All user-facing text supports localization
- Language preferences stored in persona
- AI responses adapt to user language

## Container Structure

### Primary Containers

#### 1. **Users Container**
- Partition Key: `/userId`
- Documents: `UserDocument`
- Purpose: User authentication and basic profile

#### 2. **Personas Container** 
- Partition Key: `/userId`
- Documents: `PersonaDocument`, `BuddyDocument`
- Purpose: User personalities and AI companions

#### 3. **Tasks Container**
- Partition Key: `/userId` 
- Documents: `TaskDocument`, `GoalDocument`
- Purpose: Zeitsparkasse time management

#### 4. **Conversations Container**
- Partition Key: `/userId`
- Documents: `ConversationDocument`, `BuddyMemoryDocument`
- Purpose: AI chat history and learning

#### 5. **Communications Container**
- Partition Key: `/userId`
- Documents: `EmailDocument`, `RoomDocument`
- Purpose: Email integration and room organization

## Document Models

### UserDocument
```json
{
  "id": "user_${userId}",
  "type": "user",
  "userId": "user123",
  "email": "user@example.com",
  "displayName": "John Doe",
  "authProvider": "email",
  "isEmailVerified": true,
  "preferences": {
    "defaultLanguage": "en",
    "timezone": "UTC",
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
  "createdAt": "2024-01-01T00:00:00Z",
  "_ts": 1641024000
}
```

### PersonaDocument
```json
{
  "id": "persona_${userId}_${personaId}",
  "type": "persona", 
  "userId": "user123",
  "personaId": "persona456",
  "basicInfo": {
    "age": 28,
    "occupation": "Software Developer",
    "interests": ["technology", "photography", "travel"],
    "timeZone": "Europe/Berlin",
    "languages": {
      "preferred": "en",
      "supported": ["en", "de", "es"]
    }
  },
  "typeLeapConfig": {
    "uiComplexityLevel": "Advanced",
    "fontSizePreferences": "medium",
    "enableAnimations": true,
    "culturalAdaptations": ["german_formality", "tech_terminology"]
  },
  "privacySettings": {
    "emailAccess": "ReadWrite",
    "calendarAccess": "ReadOnly", 
    "locationAccess": "None",
    "aiProcessingLocation": "local_preferred"
  },
  "buddyRelationships": [
    {
      "buddyId": "buddy789",
      "relationship": "DailyCompanion",
      "trustLevel": 0.8,
      "permissions": ["view_tasks", "create_tasks"],
      "isActive": true
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "_ts": 1641024000
}
```

### TaskDocument
```json
{
  "id": "task_${userId}_${taskId}",
  "type": "task",
  "userId": "user123",
  "taskId": "task456",
  "personaId": "persona456",
  "title": "Call mom",
  "description": "Check in with mom about weekend plans",
  "originalInput": "Remind me to call mom tomorrow at 7pm",
  "status": "pending",
  "priority": "medium",
  "category": "family",
  "tags": ["family", "tomorrow", "person"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "dueDate": "2024-01-02T19:00:00Z",
  "estimatedMinutes": 30,
  "goalId": "goal123",
  "aiContext": {
    "extractedEntities": ["person:mom"],
    "suggestedTags": ["family", "call"],
    "detectedUrgency": 0.6,
    "estimatedComplexity": "simple",
    "confidence": 0.9,
    "processingDate": "2024-01-01T00:00:00Z"
  },
  "reminders": [
    {
      "id": "reminder123",
      "triggerTime": "2024-01-02T18:00:00Z",
      "message": "Don't forget to call mom!",
      "type": "notification",
      "isTriggered": false
    }
  ],
  "_ts": 1641024000
}
```

### BuddyDocument
```json
{
  "id": "buddy_${userId}_${buddyId}",
  "type": "buddy",
  "userId": "user123", 
  "buddyId": "buddy789",
  "personaId": "persona456",
  "buddyType": "DailyCompanion",
  "basicInfo": {
    "name": "Alex",
    "avatar": "friendly-companion",
    "description": "Your friendly daily companion"
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
      "permissions": ["view_tasks", "create_tasks"]
    },
    "emailManagement": {
      "enabled": false,
      "permissions": [],
      "accounts": []
    }
  },
  "learningData": {
    "userPreferences": {
      "preferredResponseTime": "anytime",
      "communicationFrequency": "moderate",
      "topicInterests": ["technology", "family"]
    },
    "adaptationHistory": [
      {
        "timestamp": "2024-01-01T00:00:00Z",
        "adaptation": "Increased formality based on user feedback",
        "reason": "User preference for professional tone"
      }
    ]
  },
  "memoryReferences": ["memory_buddy789_001", "memory_buddy789_002"],
  "createdAt": "2024-01-01T00:00:00Z",
  "lastActiveAt": "2024-01-01T12:00:00Z",
  "_ts": 1641024000
}
```

### ConversationDocument
```json
{
  "id": "conversation_${conversationId}",
  "type": "conversation",
  "userId": "user123",
  "conversationId": "conv123",
  "buddyId": "buddy789",
  "startedAt": "2024-01-01T10:00:00Z",
  "lastMessageAt": "2024-01-01T12:00:00Z",
  "messageCount": 12,
  "isActive": true,
  "messages": [
    {
      "id": "msg_001_user",
      "messageId": "001_user",
      "senderId": "user123",
      "senderType": "user",
      "content": "Hello Alex!",
      "messageType": "greeting",
      "timestamp": "2024-01-01T10:00:00Z"
    },
    {
      "id": "msg_001_buddy", 
      "messageId": "001_buddy",
      "senderId": "buddy789",
      "senderType": "buddy",
      "content": "Hello! How can I help you today? 😊",
      "messageType": "conversational",
      "timestamp": "2024-01-01T10:00:01Z",
      "metadata": {
        "emotion": "friendly",
        "confidence": 0.95,
        "usedCapabilities": [],
        "followUpQuestions": ["How has your day been?"]
      }
    }
  ],
  "_ts": 1641024000
}
```

### EmailDocument
```json
{
  "id": "email_${userId}_${emailId}",
  "type": "email",
  "userId": "user123",
  "emailId": "email456",
  "roomId": "room_work",
  "provider": "gmail",
  "messageId": "original_message_id",
  "threadId": "thread123",
  "subject": "Project Update",
  "from": {
    "email": "colleague@company.com",
    "name": "Jane Smith"
  },
  "to": [
    {
      "email": "user@example.com", 
      "name": "John Doe"
    }
  ],
  "receivedAt": "2024-01-01T09:00:00Z",
  "aiAnalysis": {
    "category": "work",
    "priority": "medium",
    "actionItems": ["Review proposal", "Schedule meeting"],
    "sentiment": "neutral",
    "suggestedTasks": [
      {
        "title": "Review proposal from Jane",
        "priority": "medium",
        "estimatedMinutes": 30
      }
    ],
    "confidence": 0.8,
    "processedAt": "2024-01-01T09:01:00Z"
  },
  "dataSovereignty": {
    "storageLocation": "local",
    "processedLocally": true,
    "cloudSyncEnabled": false
  },
  "_ts": 1641024000
}
```

## Indexing Strategy

### Cosmos DB Indexes

#### Primary Indexes
```json
{
  "indexingPolicy": {
    "indexingMode": "consistent",
    "includedPaths": [
      {
        "path": "/userId/*"
      },
      {
        "path": "/type/*"  
      },
      {
        "path": "/status/*"
      },
      {
        "path": "/createdAt/*"
      }
    ],
    "excludedPaths": [
      {
        "path": "/aiContext/extractedEntities/*"
      },
      {
        "path": "/messages/*/metadata/*"
      }
    ]
  }
}
```

#### Composite Indexes
```json
{
  "compositeIndexes": [
    [
      { "path": "/userId", "order": "ascending" },
      { "path": "/type", "order": "ascending" },
      { "path": "/status", "order": "ascending" }
    ],
    [
      { "path": "/userId", "order": "ascending" },
      { "path": "/dueDate", "order": "ascending" }
    ],
    [
      { "path": "/userId", "order": "ascending" },
      { "path": "/priority", "order": "descending" },
      { "path": "/createdAt", "order": "descending" }
    ]
  ]
}
```

## Query Patterns

### Common Queries

#### 1. Get User's Active Tasks
```sql
SELECT * FROM c 
WHERE c.userId = "user123" 
  AND c.type = "task" 
  AND c.status IN ("pending", "in_progress")
ORDER BY c.priority DESC, c.dueDate ASC
```

#### 2. Get Buddy Conversations
```sql
SELECT * FROM c 
WHERE c.userId = "user123" 
  AND c.type = "conversation" 
  AND c.buddyId = "buddy789"
  AND c.isActive = true
ORDER BY c.lastMessageAt DESC
```

#### 3. Get Tasks by Category and Date Range
```sql
SELECT * FROM c 
WHERE c.userId = "user123" 
  AND c.type = "task"
  AND c.category = "work"
  AND c.dueDate >= "2024-01-01T00:00:00Z"
  AND c.dueDate <= "2024-01-31T23:59:59Z"
```

#### 4. Search Tasks by Natural Language
```sql
SELECT * FROM c 
WHERE c.userId = "user123" 
  AND c.type = "task"
  AND (CONTAINS(c.title, "mom") 
    OR CONTAINS(c.description, "mom")
    OR ARRAY_CONTAINS(c.aiContext.extractedEntities, "person:mom"))
```

## Data Sovereignty Implementation

### Location-Based Storage
```json
{
  "dataSovereignty": {
    "storageLocation": "local|hybrid|cloud",
    "preferredRegion": "europe|us|asia",
    "aiProcessingLocation": "local_only|local_preferred|cloud_preferred|cloud_only",
    "syncSettings": {
      "enabled": true,
      "frequency": "real_time|hourly|daily",
      "conflictResolution": "local_wins|cloud_wins|timestamp"
    }
  }
}
```

### Privacy-First Design
- **Local-First**: Critical data (personal conversations, sensitive tasks)
- **Hybrid**: Tasks and goals (local + optional cloud sync)
- **Cloud-Preferred**: Non-sensitive data (public templates, shared resources)

## Migrations and Versioning

### Document Versioning
```json
{
  "schemaVersion": "1.0",
  "migrationHistory": [
    {
      "version": "1.0",
      "appliedAt": "2024-01-01T00:00:00Z",
      "changes": ["Initial schema"]
    }
  ]
}
```

### Migration Strategy
1. **Additive Changes**: New optional fields
2. **Breaking Changes**: Versioned documents with migration utilities
3. **Data Transformation**: Background jobs for schema updates

## Performance Optimizations

### 1. **Request Unit (RU) Optimization**
- Partition-aware queries (always include userId)
- Efficient indexing for common query patterns
- Pagination for large result sets

### 2. **Document Size Management**
- Large conversations split into multiple documents
- Historical data archived to separate containers
- Embedded data limited to frequently accessed items

### 3. **Connection Pooling**
- Singleton Cosmos client
- Connection reuse across requests
- Bulk operations for batch processing

### 4. **Caching Strategy**
- In-memory cache for user preferences
- Redis cache for frequently accessed data
- Invalidation based on document updates

## Backup and Recovery

### 1. **Point-in-Time Recovery**
- Continuous backup enabled
- 30-day retention for production
- Regional replication for disaster recovery

### 2. **User Data Export**
- JSON export for all user documents
- GDPR compliance for data portability
- Automated backup scheduling

## Monitoring and Analytics

### 1. **Performance Metrics**
- Query latency and RU consumption
- Document size and growth patterns
- Hot partition detection

### 2. **Usage Analytics**
- Feature usage tracking (anonymized)
- AI accuracy metrics
- User engagement patterns

### 3. **Data Quality**
- Schema validation
- Orphaned document detection
- Consistency checks across references

## Future Enhancements

### 1. **Graph Relationships**
- Task-Goal relationships
- Buddy-Memory connections
- User collaboration networks

### 2. **Time-Series Data**
- Dedicated containers for time tracking
- Efficient aggregation queries
- Historical trend analysis

### 3. **Vector Search**
- AI embedding storage
- Semantic similarity queries
- Content-based recommendations

### 4. **Multi-Tenant Extensions**
- Organization-level partitioning
- Team collaboration features
- Enterprise data governance

This data model design provides the foundation for Spinner.Net's scalable, privacy-first, and AI-enhanced time management platform while maintaining flexibility for future growth and feature additions.