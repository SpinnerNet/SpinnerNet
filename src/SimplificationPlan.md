# Cosmos DB Simplification Plan

## What We Keep (Sprint 1 Essentials)
✅ UserDocument - Authentication
✅ PersonaDocument - Core feature
✅ InterviewSessionDocument - Interview flow
✅ Basic auth endpoints
✅ Persona creation/check

## What We Comment Out (Future Sprints)
❌ TaskDocument + all Task features
❌ BuddyDocument + all Buddy features  
❌ GoalDocument + all Goal features
❌ ConversationDocument
❌ EmailDocument
❌ BuddyMemoryDocument
❌ Analytics features
❌ DataSovereignty features

## The Pattern (Microsoft Style)
```csharp
// Simple, direct PascalCase properties
public class PersonaDocument : CosmosDocument
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string PersonaId { get; set; }
    public string DisplayName { get; set; }
    public int Age { get; set; }
    public string Personality { get; set; }
    public List<string> Goals { get; set; }
    public Dictionary<string, object> Preferences { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Why This Works
1. Matches Microsoft's examples
2. Cosmos DB SDK handles serialization
3. Clean, maintainable code
4. Fast path to zero errors
5. Solid foundation for future

## Next Steps
1. Comment out non-essential code
2. Apply pattern to 3 documents
3. Fix related handlers
4. Clean build
5. Document for future sprints