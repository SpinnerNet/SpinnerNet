# Phase 2: Core Features - Product Requirements Document

## 🎯 **PHASE OVERVIEW**

**Phase 2** implements the core user-facing features that transform the foundational infrastructure into a complete persona creation system. This phase builds upon the WebLLM and age-adaptive UI foundation to deliver enhanced persona creation, dynamic environments, and voice integration.

### **Key Objectives**
- Implement sophisticated multi-phase persona creation
- Create dynamic environment system with age-appropriate themes
- Integrate voice recognition and synthesis capabilities
- Deliver complete core user experience
- Establish extensible architecture for advanced features

### **Phase Duration**: 8-10 weeks
### **Team Size**: 3-4 developers (2 Frontend, 1 Backend, 1 UI/UX)

---

## 📦 **PACKAGE 3: ENHANCED PERSONA CREATION**

### **Overview**
Implement sophisticated multi-phase interview flow that creates comprehensive user personas through AI-driven conversation analysis and preference detection.

### **Core Features**
- **Multi-Phase Interview Flow**: 4-phase structured persona discovery
- **AI-Driven Analysis**: Real-time personality and preference detection
- **Adaptive Questioning**: Questions adapt based on age and previous responses
- **Comprehensive Persona Models**: Extended PersonaDocument with rich metadata
- **Progress Tracking**: Visual progress indicators with estimated completion time

### **Technical Specifications**

#### **Enhanced Interview Flow Controller**
```csharp
// Services/PersonaInterviewService.cs
using SpinnerNet.App.Services;
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.App.Services;

public class PersonaInterviewService
{
    private readonly WebLLMService _webLLMService;
    private readonly AgeDetectionService _ageDetectionService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<PersonaInterviewService> _logger;

    public event Action<InterviewProgress>? ProgressUpdated;
    public event Action<PersonaInsight>? InsightGenerated;

    public PersonaInterviewService(
        WebLLMService webLLMService,
        AgeDetectionService ageDetectionService,
        ILocalizationService localizationService,
        ILogger<PersonaInterviewService> logger)
    {
        _webLLMService = webLLMService;
        _ageDetectionService = ageDetectionService;
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task<InterviewSession> StartInterviewAsync(string userId, AgeProfile ageProfile)
    {
        var session = new InterviewSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = userId,
            AgeProfile = ageProfile,
            CurrentPhase = InterviewPhase.LanguageAndIdentity,
            StartedAt = DateTime.UtcNow,
            Responses = new List<InterviewResponse>(),
            GeneratedInsights = new List<PersonaInsight>()
        };

        await InitializePhaseAsync(session, InterviewPhase.LanguageAndIdentity);
        return session;
    }

    public async Task<InterviewResponse> ProcessResponseAsync(
        InterviewSession session, 
        string userResponse, 
        InterviewResponseType responseType = InterviewResponseType.Text)
    {
        var response = new InterviewResponse
        {
            ResponseId = Guid.NewGuid().ToString(),
            Phase = session.CurrentPhase,
            UserInput = userResponse,
            ResponseType = responseType,
            Timestamp = DateTime.UtcNow,
            ProcessingTimeMs = 0
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Generate AI analysis of the response
            var analysisPrompt = await BuildAnalysisPromptAsync(session, response);
            var aiAnalysis = await _webLLMService.GenerateResponseAsync(analysisPrompt, new WebLLMOptions
            {
                SystemPrompt = GetSystemPromptForPhase(session.CurrentPhase, session.AgeProfile),
                Temperature = 0.3,
                MaxTokens = 400
            });

            response.AIAnalysis = aiAnalysis;

            // Extract insights from analysis
            var insights = await ExtractInsightsAsync(aiAnalysis, session.CurrentPhase);
            response.ExtractedInsights = insights;

            // Add to session
            session.Responses.Add(response);
            session.GeneratedInsights.AddRange(insights);

            // Generate follow-up question
            var followUpPrompt = await BuildFollowUpPromptAsync(session);
            var followUpQuestion = await _webLLMService.GenerateResponseAsync(followUpPrompt, new WebLLMOptions
            {
                SystemPrompt = GetQuestionGenerationPrompt(session.AgeProfile),
                Temperature = 0.7,
                MaxTokens = 200
            });

            response.FollowUpQuestion = followUpQuestion;

            // Check for phase completion
            if (await IsPhaseCompleteAsync(session))
            {
                await CompleteCurrentPhaseAsync(session);
                
                if (session.CurrentPhase < InterviewPhase.BuddyNamingAndFinalization)
                {
                    await AdvanceToNextPhaseAsync(session);
                }
                else
                {
                    await CompleteInterviewAsync(session);
                }
            }

            // Update progress
            var progress = CalculateProgress(session);
            ProgressUpdated?.Invoke(progress);

            // Emit insights
            foreach (var insight in insights)
            {
                InsightGenerated?.Invoke(insight);
            }

            response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing interview response");
            response.Error = ex.Message;
            response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            return response;
        }
    }

    private async Task<string> BuildAnalysisPromptAsync(InterviewSession session, InterviewResponse response)
    {
        var ageContext = GetAgeContextDescription(session.AgeProfile);
        var phaseContext = GetPhaseContextDescription(session.CurrentPhase);
        var conversationHistory = GetConversationHistory(session);

        return $@"Analyze this user response for persona creation insights:

AGE CONTEXT: {ageContext}
INTERVIEW PHASE: {phaseContext}
CONVERSATION HISTORY: {conversationHistory}
USER RESPONSE: {response.UserInput}

Provide detailed analysis including:
1. Personality traits indicated (Big Five framework)
2. Communication style preferences
3. Interest and value indicators
4. Technology comfort level
5. Social interaction preferences
6. Learning style indicators
7. Confidence level in analysis (0-100)

Format as structured analysis with evidence from the response.";
    }

    private string GetSystemPromptForPhase(InterviewPhase phase, AgeProfile ageProfile)
    {
        var basePrompt = "You are an expert psychologist and UX researcher conducting persona analysis.";
        var ageAdjustment = GetAgeAppropriateAnalysisGuidance(ageProfile);

        return phase switch
        {
            InterviewPhase.LanguageAndIdentity => $@"{basePrompt} {ageAdjustment}
                Focus on cultural background, language preferences, and identity markers.
                Look for communication style, cultural values, and accessibility needs.",

            InterviewPhase.PassionAndInterestDiscovery => $@"{basePrompt} {ageAdjustment}
                Focus on interests, hobbies, passions, and intrinsic motivations.
                Analyze creativity levels, social vs solitary preferences, and growth mindset.",

            InterviewPhase.PersonaRelationshipDefinition => $@"{basePrompt} {ageAdjustment}
                Focus on relationship preferences, communication styles, and support needs.
                Analyze trust patterns, authority preferences, and interaction dynamics.",

            InterviewPhase.BuddyNamingAndFinalization => $@"{basePrompt} {ageAdjustment}
                Focus on finalizing persona characteristics and preferences.
                Synthesize all previous insights into comprehensive personality profile.",

            _ => basePrompt
        };
    }

    private async Task<List<PersonaInsight>> ExtractInsightsAsync(string analysis, InterviewPhase phase)
    {
        var insights = new List<PersonaInsight>();

        // Use AI to structure the analysis into specific insights
        var extractionPrompt = $@"Extract specific persona insights from this analysis:

{analysis}

Return structured insights in this format:
TRAIT: [trait name]
VALUE: [numerical value 0-100]
CONFIDENCE: [confidence 0-100]
EVIDENCE: [evidence from user response]
CATEGORY: [personality|communication|interests|technology|social|learning]

Extract 3-5 most significant insights.";

        var structuredInsights = await _webLLMService.GenerateResponseAsync(extractionPrompt, new WebLLMOptions
        {
            Temperature = 0.1,
            MaxTokens = 500
        });

        // Parse structured insights (simplified parsing - could use more sophisticated NLP)
        var lines = structuredInsights.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var currentInsight = new PersonaInsight { Phase = phase, Timestamp = DateTime.UtcNow };

        foreach (var line in lines)
        {
            if (line.StartsWith("TRAIT:"))
            {
                if (!string.IsNullOrEmpty(currentInsight.TraitName))
                {
                    insights.Add(currentInsight);
                    currentInsight = new PersonaInsight { Phase = phase, Timestamp = DateTime.UtcNow };
                }
                currentInsight.TraitName = line.Substring(6).Trim();
            }
            else if (line.StartsWith("VALUE:"))
            {
                if (int.TryParse(line.Substring(6).Trim(), out var value))
                    currentInsight.Value = value;
            }
            else if (line.StartsWith("CONFIDENCE:"))
            {
                if (int.TryParse(line.Substring(11).Trim(), out var confidence))
                    currentInsight.Confidence = confidence;
            }
            else if (line.StartsWith("EVIDENCE:"))
            {
                currentInsight.Evidence = line.Substring(9).Trim();
            }
            else if (line.StartsWith("CATEGORY:"))
            {
                currentInsight.Category = line.Substring(9).Trim();
            }
        }

        if (!string.IsNullOrEmpty(currentInsight.TraitName))
        {
            insights.Add(currentInsight);
        }

        return insights;
    }

    private async Task<PersonaDocument> FinalizePersonaAsync(InterviewSession session)
    {
        var persona = new PersonaDocument
        {
            id = $"persona_{session.UserId}_{Guid.NewGuid()}",
            type = "persona",
            UserId = session.UserId,
            personaId = Guid.NewGuid().ToString(),
            isDefault = true, // First persona is default
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        // Synthesize basic info from insights
        persona.basicInfo = await SynthesizeBasicInfoAsync(session);
        
        // Create comprehensive personality profile
        persona.personalityProfile = await CreatePersonalityProfileAsync(session);
        
        // Generate optimal TypeLeap configuration
        persona.typeLeapConfig = await GenerateTypeLeapConfigAsync(session);
        
        // Set learning preferences
        persona.learningPreferences = await DetermineLearningPreferencesAsync(session);
        
        // Configure privacy settings
        persona.privacySettings = await ConfigurePrivacySettingsAsync(session);

        return persona;
    }
}

// Models for enhanced interview system
public class InterviewSession
{
    public string SessionId { get; set; } = "";
    public string UserId { get; set; } = "";
    public AgeProfile AgeProfile { get; set; } = new();
    public InterviewPhase CurrentPhase { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<InterviewResponse> Responses { get; set; } = new();
    public List<PersonaInsight> GeneratedInsights { get; set; } = new();
    public PersonaDocument? FinalizedPersona { get; set; }
}

public class InterviewResponse
{
    public string ResponseId { get; set; } = "";
    public InterviewPhase Phase { get; set; }
    public string UserInput { get; set; } = "";
    public InterviewResponseType ResponseType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? AIAnalysis { get; set; }
    public List<PersonaInsight> ExtractedInsights { get; set; } = new();
    public string? FollowUpQuestion { get; set; }
    public string? Error { get; set; }
    public long ProcessingTimeMs { get; set; }
}

public class PersonaInsight
{
    public string TraitName { get; set; } = "";
    public int Value { get; set; } // 0-100
    public int Confidence { get; set; } // 0-100
    public string Evidence { get; set; } = "";
    public string Category { get; set; } = "";
    public InterviewPhase Phase { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum InterviewPhase
{
    LanguageAndIdentity = 1,
    PassionAndInterestDiscovery = 2,
    PersonaRelationshipDefinition = 3,
    BuddyNamingAndFinalization = 4
}

public enum InterviewResponseType
{
    Text,
    Voice,
    Selection,
    Rating
}
```

#### **Enhanced Persona Document Model**
```csharp
// Models/Enhanced/EnhancedPersonaDocument.cs
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.Shared.Models.Enhanced;

public class EnhancedPersonaDocument : PersonaDocument
{
    /// <summary>Comprehensive personality analysis from interview</summary>
    public PersonalityProfile personalityProfile { get; set; } = new();
    
    /// <summary>Interview session metadata and insights</summary>
    public InterviewMetadata interviewMetadata { get; set; } = new();
    
    /// <summary>AI-generated persona summary and recommendations</summary>
    public PersonaSummary personaSummary { get; set; } = new();
    
    /// <summary>Confidence scores for different personality aspects</summary>
    public Dictionary<string, double> confidenceScores { get; set; } = new();
    
    /// <summary>Evidence backing personality trait assignments</summary>
    public List<TraitEvidence> traitEvidence { get; set; } = new();
}

public class PersonalityProfile
{
    public BigFiveTraits bigFiveTraits { get; set; } = new();
    public CommunicationStyle communicationStyle { get; set; } = new();
    public MotivationalDrivers motivationalDrivers { get; set; } = new();
    public SocialPreferences socialPreferences { get; set; } = new();
    public LearningStyle learningStyle { get; set; } = new();
    public TechnologyComfort technologyComfort { get; set; } = new();
}

public class BigFiveTraits
{
    public int Openness { get; set; } // 0-100
    public int Conscientiousness { get; set; } // 0-100
    public int Extraversion { get; set; } // 0-100
    public int Agreeableness { get; set; } // 0-100
    public int Neuroticism { get; set; } // 0-100
    
    public Dictionary<string, string> TraitDescriptions { get; set; } = new();
    public Dictionary<string, double> ConfidenceScores { get; set; } = new();
}

public class CommunicationStyle
{
    public string PreferredTone { get; set; } = ""; // formal, casual, friendly, professional
    public string InformationDensity { get; set; } = ""; // concise, detailed, comprehensive
    public string FeedbackStyle { get; set; } = ""; // direct, gentle, encouraging, challenging
    public int EmotionalExpressiveness { get; set; } // 0-100
    public int TechnicalLanguage { get; set; } // 0-100
    public List<string> CommunicationTriggers { get; set; } = new(); // What motivates them
    public List<string> CommunicationAvoidance { get; set; } = new(); // What to avoid
}

public class InterviewMetadata
{
    public TimeSpan TotalDuration { get; set; }
    public int TotalResponses { get; set; }
    public Dictionary<InterviewPhase, TimeSpan> PhasesDuration { get; set; } = new();
    public Dictionary<InterviewPhase, int> PhasesResponseCount { get; set; } = new();
    public double OverallEngagement { get; set; } // 0-100
    public List<string> InterviewNotes { get; set; } = new(); // AI observations
}

public class PersonaSummary
{
    public string CorePersonality { get; set; } = "";
    public string IdealAIRelationship { get; set; } = "";
    public string OptimalCommunicationApproach { get; set; } = "";
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> GrowthOpportunities { get; set; } = new();
    public List<string> PersonalizedRecommendations { get; set; } = new();
}

public class TraitEvidence
{
    public string TraitName { get; set; } = "";
    public string Evidence { get; set; } = "";
    public InterviewPhase SourcePhase { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}
```

#### **Interview Flow Component**
```razor
@* Components/EnhancedPersonaInterview.razor *@
@using SpinnerNet.App.Services
@inject PersonaInterviewService InterviewService
@inject ILocalizationService LocalizationService
@inject AgeDetectionService AgeDetectionService
@implements IAsyncDisposable

<MudContainer MaxWidth="MaxWidth.Large" Class="enhanced-interview-container">
    
    @if (_currentSession == null)
    {
        <InterviewWelcomeCard AgeProfile="@_ageProfile" OnStartInterview="StartInterviewAsync" />
    }
    else if (_currentSession.CompletedAt == null)
    {
        <MudStack Spacing="6">
            
            <!-- Progress Indicator -->
            <InterviewProgressBar Session="@_currentSession" />
            
            <!-- Current Phase Content -->
            <MudCard Class="interview-main-card">
                <MudCardHeader>
                    <MudText Typo="Typo.h5">@GetPhaseTitle(_currentSession.CurrentPhase)</MudText>
                    <MudSpacer />
                    <MudChip Icon="@GetPhaseIcon(_currentSession.CurrentPhase)" 
                             Color="Color.Primary">
                        @GetPhaseDescription(_currentSession.CurrentPhase)
                    </MudChip>
                </MudCardHeader>
                
                <MudCardContent>
                    @if (_isProcessing)
                    {
                        <ProcessingIndicator Message="@_processingMessage" />
                    }
                    else
                    {
                        <!-- Conversation History -->
                        <div class="conversation-history" style="max-height: 400px; overflow-y: auto;">
                            @foreach (var response in _currentSession.Responses.Where(r => r.Phase == _currentSession.CurrentPhase))
                            {
                                <ConversationBubble Response="@response" AgeProfile="@_ageProfile" />
                            }
                        </div>
                        
                        <!-- Current Question -->
                        @if (!string.IsNullOrEmpty(_currentQuestion))
                        {
                            <div class="current-question mt-4">
                                <MudPaper Class="question-bubble pa-4" Elevation="2">
                                    <MudStack Row="true" Spacing="3" AlignItems="Center">
                                        <MudAvatar Color="Color.Primary" Size="Size.Medium">
                                            <MudIcon Icon="@Icons.Material.Filled.Psychology" />
                                        </MudAvatar>
                                        <MudText Typo="Typo.body1">@_currentQuestion</MudText>
                                    </MudStack>
                                </MudPaper>
                            </div>
                        }
                        
                        <!-- Response Input -->
                        <div class="response-input mt-4">
                            <AdaptiveResponseInput UserAge="@_ageProfile.EstimatedAgeRange"
                                                   @bind-Value="_userInput"
                                                   OnSubmit="SubmitResponseAsync"
                                                   Placeholder="@GetInputPlaceholder(_currentSession.CurrentPhase)" 
                                                   VoiceEnabled="@ShouldEnableVoice()" />
                        </div>
                    }
                </MudCardContent>
            </MudCard>
            
            <!-- Real-time Insights Panel -->
            @if (_realtimeInsights.Any())
            {
                <InsightsPanel Insights="@_realtimeInsights" AgeProfile="@_ageProfile" />
            }
            
        </MudStack>
    }
    else
    {
        <PersonaCompletionCard Session="@_currentSession" OnCreateAnother="ResetInterviewAsync" />
    }
    
</MudContainer>

@code {
    [Parameter] public string UserId { get; set; } = "";
    
    private InterviewSession? _currentSession;
    private AgeProfile _ageProfile = new();
    private bool _isProcessing = false;
    private string _processingMessage = "";
    private string _currentQuestion = "";
    private string _userInput = "";
    private List<PersonaInsight> _realtimeInsights = new();

    protected override async Task OnInitializedAsync()
    {
        InterviewService.ProgressUpdated += OnProgressUpdated;
        InterviewService.InsightGenerated += OnInsightGenerated;
        
        _ageProfile = await AgeDetectionService.DetectAgeProfileAsync();
    }

    private async Task StartInterviewAsync()
    {
        try
        {
            _isProcessing = true;
            _processingMessage = LocalizationService.GetString("Interview_Starting");
            StateHasChanged();
            
            _currentSession = await InterviewService.StartInterviewAsync(UserId, _ageProfile);
            _currentQuestion = GetInitialQuestionForPhase(_currentSession.CurrentPhase);
            
            _isProcessing = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _isProcessing = false;
            // Handle error
            StateHasChanged();
        }
    }

    private async Task SubmitResponseAsync()
    {
        if (string.IsNullOrWhiteSpace(_userInput) || _currentSession == null || _isProcessing)
            return;

        try
        {
            _isProcessing = true;
            _processingMessage = LocalizationService.GetString("Interview_Processing");
            StateHasChanged();

            var response = await InterviewService.ProcessResponseAsync(
                _currentSession, 
                _userInput.Trim(), 
                InterviewResponseType.Text);

            _currentQuestion = response.FollowUpQuestion ?? "";
            _userInput = "";
            
            _isProcessing = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _isProcessing = false;
            // Handle error
            StateHasChanged();
        }
    }

    private void OnProgressUpdated(InterviewProgress progress)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnInsightGenerated(PersonaInsight insight)
    {
        _realtimeInsights.Add(insight);
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        InterviewService.ProgressUpdated -= OnProgressUpdated;
        InterviewService.InsightGenerated -= OnInsightGenerated;
    }
}
```

---

## 📦 **PACKAGE 4: DYNAMIC ENVIRONMENT SYSTEM**

### **Overview**
Create immersive, age-appropriate environment system that adapts based on user demographics, interests, and personality insights gathered during the interview process.

### **Core Features**
- **Age-Specific Environment Themes**: 8 distinct age group environments
- **Interest-Based Customization**: Environments adapt to user interests and hobbies
- **Dynamic Particle Systems**: Interactive animations and visual effects
- **Mood-Responsive Backgrounds**: Environments change based on conversation tone
- **Cultural Adaptations**: Themes reflect user's cultural background

### **Technical Specifications**

#### **Environment Management Service**
```csharp
// Services/EnvironmentService.cs
using SpinnerNet.App.Models;
using Microsoft.JSInterop;

namespace SpinnerNet.App.Services;

public class EnvironmentService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<EnvironmentService> _logger;
    
    public event Action<EnvironmentConfig>? EnvironmentChanged;
    
    private EnvironmentConfig _currentEnvironment = new();

    public EnvironmentService(IJSRuntime jsRuntime, ILocalStorageService localStorage, ILogger<EnvironmentService> logger)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<EnvironmentConfig> SelectOptimalEnvironmentAsync(
        AgeProfile ageProfile, 
        List<string> interests, 
        string mood = "neutral",
        string culturalContext = "western")
    {
        var environment = SelectBaseEnvironment(ageProfile, interests);
        
        // Apply mood adaptations
        environment = ApplyMoodAdaptations(environment, mood);
        
        // Apply cultural adaptations
        environment = ApplyCulturalAdaptations(environment, culturalContext);
        
        // Apply accessibility enhancements
        environment = ApplyAccessibilityEnhancements(environment, ageProfile);
        
        await ActivateEnvironmentAsync(environment);
        
        return environment;
    }

    private EnvironmentConfig SelectBaseEnvironment(AgeProfile ageProfile, List<string> interests)
    {
        var age = ageProfile.Age ?? GetAgeFromRange(ageProfile.EstimatedAgeRange);
        
        return age switch
        {
            // Children (6-12): Fantasy & Educational
            <= 12 when interests.Any(i => i.Contains("animals")) => EnvironmentTemplates.MagicalForestCreatures,
            <= 12 when interests.Any(i => i.Contains("space")) => EnvironmentTemplates.FriendlySpaceStation,
            <= 12 when interests.Any(i => i.Contains("art")) => EnvironmentTemplates.RainbowArtStudio,
            <= 12 when interests.Any(i => i.Contains("stories")) => EnvironmentTemplates.FairyTaleLibrary,
            <= 12 => EnvironmentTemplates.ColorfulPlayground,
            
            // Teens (13-17): Social & Identity
            <= 17 when interests.Any(i => i.Contains("social")) => EnvironmentTemplates.TrendyHangoutSpace,
            <= 17 when interests.Any(i => i.Contains("music")) => EnvironmentTemplates.BackstageConcertVenue,
            <= 17 when interests.Any(i => i.Contains("gaming")) => EnvironmentTemplates.EsportsArena,
            <= 17 when interests.Any(i => i.Contains("fashion")) => EnvironmentTemplates.InfluencerStudio,
            <= 17 => EnvironmentTemplates.ModernTeenLoft,
            
            // Young Adults (18-25): Dynamic & Aspirational
            <= 25 when interests.Any(i => i.Contains("gaming")) => EnvironmentTemplates.CyberpunkNeonCity,
            <= 25 when interests.Any(i => i.Contains("travel")) => EnvironmentTemplates.GlobalNomadHub,
            <= 25 when interests.Any(i => i.Contains("creativity")) => EnvironmentTemplates.MakerSpaceLab,
            <= 25 when interests.Any(i => i.Contains("fitness")) => EnvironmentTemplates.FuturisticGym,
            <= 25 => EnvironmentTemplates.UrbanCreativeLoft,
            
            // Early Adults (26-39): Professional & Balanced
            <= 39 when interests.Any(i => i.Contains("tech")) => EnvironmentTemplates.StartupInnovationLab,
            <= 39 when interests.Any(i => i.Contains("business")) => EnvironmentTemplates.ModernCornerOffice,
            <= 39 when interests.Any(i => i.Contains("health")) => EnvironmentTemplates.WellnessSanctuary,
            <= 39 when interests.Any(i => i.Contains("family")) => EnvironmentTemplates.ModernFamilyHome,
            <= 39 => EnvironmentTemplates.ContemporaryWorkspace,
            
            // Middle Adults (40-54): Established & Strategic
            <= 54 when interests.Any(i => i.Contains("leadership")) => EnvironmentTemplates.ExecutiveBoardroom,
            <= 54 when interests.Any(i => i.Contains("mentoring")) => EnvironmentTemplates.WisdomSharingSpace,
            <= 54 when interests.Any(i => i.Contains("luxury")) => EnvironmentTemplates.SophisticatedLounge,
            <= 54 when interests.Any(i => i.Contains("outdoors")) => EnvironmentTemplates.ExecutiveRetreat,
            <= 54 => EnvironmentTemplates.ElegantProfessionalOffice,
            
            // Young Seniors (55-69): Active & Engaged
            <= 69 when interests.Any(i => i.Contains("gardening")) => EnvironmentTemplates.MasterGardenerGreenhouse,
            <= 69 when interests.Any(i => i.Contains("travel")) => EnvironmentTemplates.SeasonedExplorerDen,
            <= 69 when interests.Any(i => i.Contains("learning")) => EnvironmentTemplates.DistinguishedStudy,
            <= 69 when interests.Any(i => i.Contains("grandchildren")) => EnvironmentTemplates.WarmFamilyGathering,
            <= 69 => EnvironmentTemplates.ComfortableActivityRoom,
            
            // Seniors (70-84): Comfortable & Nostalgic
            <= 84 when interests.Any(i => i.Contains("reading")) => EnvironmentTemplates.ClassicHomeLibrary,
            <= 84 when interests.Any(i => i.Contains("crafts")) => EnvironmentTemplates.CozyHobbyRoom,
            <= 84 when interests.Any(i => i.Contains("nature")) => EnvironmentTemplates.PeacefulGardenView,
            <= 84 when interests.Any(i => i.Contains("family")) => EnvironmentTemplates.HeritageFamilyRoom,
            <= 84 => EnvironmentTemplates.SereneLivingSpace,
            
            // Elder Seniors (85+): Peaceful & Assistive
            _ when interests.Any(i => i.Contains("memories")) => EnvironmentTemplates.MemoryLaneSanctuary,
            _ when interests.Any(i => i.Contains("music")) => EnvironmentTemplates.GentleMusicConservatory,
            _ when interests.Any(i => i.Contains("spiritual")) => EnvironmentTemplates.TranquilMeditationSpace,
            _ when interests.Any(i => i.Contains("care")) => EnvironmentTemplates.AssistedLivingSuite,
            _ => EnvironmentTemplates.HealingComfortEnvironment
        };
    }

    public async Task ActivateEnvironmentAsync(EnvironmentConfig environment)
    {
        try
        {
            _currentEnvironment = environment;
            
            // Update CSS custom properties
            await _jsRuntime.InvokeVoidAsync("updateEnvironmentStyles", environment.ToStyleObject());
            
            // Initialize particle system
            if (!string.IsNullOrEmpty(environment.ParticleSystem))
            {
                await _jsRuntime.InvokeVoidAsync("initializeParticleSystem", environment.ParticleSystem, environment.ParticleIntensity);
            }
            
            // Set background music
            if (!string.IsNullOrEmpty(environment.BackgroundMusic))
            {
                await _jsRuntime.InvokeVoidAsync("setBackgroundMusic", environment.BackgroundMusic, environment.AudioVolume);
            }
            
            // Apply animations
            await _jsRuntime.InvokeVoidAsync("setAnimationIntensity", environment.AnimationIntensity);
            
            // Store preference
            await _localStorage.SetItemAsync("preferred_environment", environment);
            
            EnvironmentChanged?.Invoke(environment);
            
            _logger.LogInformation($"Environment activated: {environment.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to activate environment: {environment.Name}");
        }
    }

    public async Task UpdateMoodAsync(string mood)
    {
        var updatedEnvironment = ApplyMoodAdaptations(_currentEnvironment, mood);
        await ActivateEnvironmentAsync(updatedEnvironment);
    }

    private EnvironmentConfig ApplyMoodAdaptations(EnvironmentConfig environment, string mood)
    {
        var adapted = environment.Clone();
        
        switch (mood.ToLower())
        {
            case "energetic":
                adapted.ColorVibrancy *= 1.2f;
                adapted.AnimationIntensity = "high";
                adapted.ParticleIntensity *= 1.3f;
                break;
                
            case "calm":
                adapted.ColorVibrancy *= 0.8f;
                adapted.AnimationIntensity = "gentle";
                adapted.ParticleIntensity *= 0.7f;
                break;
                
            case "focused":
                adapted.ColorVibrancy *= 0.9f;
                adapted.AnimationIntensity = "minimal";
                adapted.ParticleIntensity *= 0.5f;
                adapted.BackgroundMusic = "focus_ambient";
                break;
                
            case "celebratory":
                adapted.ColorVibrancy *= 1.4f;
                adapted.AnimationIntensity = "celebration";
                adapted.ParticleIntensity *= 1.5f;
                adapted.ParticleSystem = "celebration_confetti";
                break;
        }
        
        return adapted;
    }
}

public class EnvironmentConfig
{
    public string Name { get; set; } = "";
    public string ThemeId { get; set; } = "";
    public string BackgroundImage { get; set; } = "";
    public string BackgroundVideo { get; set; } = "";
    public ColorPalette PrimaryColors { get; set; } = new();
    public ColorPalette AccentColors { get; set; } = new();
    public float ColorVibrancy { get; set; } = 1.0f;
    public string AnimationIntensity { get; set; } = "medium"; // minimal, gentle, medium, high, celebration
    public string ParticleSystem { get; set; } = "";
    public float ParticleIntensity { get; set; } = 1.0f;
    public string BackgroundMusic { get; set; } = "";
    public float AudioVolume { get; set; } = 0.3f;
    public bool AccessibilityMode { get; set; } = false;
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    
    public EnvironmentConfig Clone()
    {
        return new EnvironmentConfig
        {
            Name = Name,
            ThemeId = ThemeId,
            BackgroundImage = BackgroundImage,
            BackgroundVideo = BackgroundVideo,
            PrimaryColors = PrimaryColors.Clone(),
            AccentColors = AccentColors.Clone(),
            ColorVibrancy = ColorVibrancy,
            AnimationIntensity = AnimationIntensity,
            ParticleSystem = ParticleSystem,
            ParticleIntensity = ParticleIntensity,
            BackgroundMusic = BackgroundMusic,
            AudioVolume = AudioVolume,
            AccessibilityMode = AccessibilityMode,
            CustomProperties = new Dictionary<string, string>(CustomProperties)
        };
    }
}
```

#### **Environment Templates**
```csharp
// Models/EnvironmentTemplates.cs
namespace SpinnerNet.App.Models;

public static class EnvironmentTemplates
{
    public static EnvironmentConfig MagicalForestCreatures => new()
    {
        Name = "Magical Forest Creatures",
        ThemeId = "magical_forest",
        BackgroundImage = "/environments/magical-forest-bg.jpg",
        BackgroundVideo = "/environments/magical-forest.mp4",
        PrimaryColors = new ColorPalette 
        { 
            Primary = "#2d5a27", // Forest green
            Secondary = "#8bc34a", // Light green
            Accent = "#ffeb3b" // Magical yellow
        },
        AccentColors = new ColorPalette
        {
            Primary = "#9c27b0", // Magic purple
            Secondary = "#e91e63", // Fairy pink
            Accent = "#00bcd4" // Sparkle cyan
        },
        ColorVibrancy = 1.3f,
        AnimationIntensity = "magical",
        ParticleSystem = "forest_sparkles",
        ParticleIntensity = 1.2f,
        BackgroundMusic = "whimsical_forest",
        AudioVolume = 0.4f,
        CustomProperties = new Dictionary<string, string>
        {
            ["--forest-canopy-opacity"] = "0.8",
            ["--creature-animation-speed"] = "2s",
            ["--sparkle-frequency"] = "high"
        }
    };

    public static EnvironmentConfig CyberpunkNeonCity => new()
    {
        Name = "Cyberpunk Neon City",
        ThemeId = "cyberpunk_city",
        BackgroundImage = "/environments/cyberpunk-city-bg.jpg",
        BackgroundVideo = "/environments/cyberpunk-neon.mp4",
        PrimaryColors = new ColorPalette
        {
            Primary = "#0a0a0a", // Deep black
            Secondary = "#1a1a1a", // Dark gray
            Accent = "#00ffff" // Neon cyan
        },
        AccentColors = new ColorPalette
        {
            Primary = "#ff0080", // Neon pink
            Secondary = "#8000ff", // Neon purple
            Accent = "#00ff40" // Neon green
        },
        ColorVibrancy = 1.5f,
        AnimationIntensity = "high",
        ParticleSystem = "neon_trails",
        ParticleIntensity = 1.4f,
        BackgroundMusic = "cyberpunk_ambient",
        AudioVolume = 0.35f,
        CustomProperties = new Dictionary<string, string>
        {
            ["--neon-glow-intensity"] = "high",
            ["--hologram-opacity"] = "0.7",
            ["--scan-lines"] = "enabled"
        }
    };

    public static EnvironmentConfig PeacefulGardenView => new()
    {
        Name = "Peaceful Garden View",
        ThemeId = "peaceful_garden",
        BackgroundImage = "/environments/peaceful-garden-bg.jpg",
        BackgroundVideo = "/environments/gentle-garden.mp4",
        PrimaryColors = new ColorPalette
        {
            Primary = "#4a5d23", // Sage green
            Secondary = "#8fbc8f", // Light sage
            Accent = "#deb887" // Warm beige
        },
        AccentColors = new ColorPalette
        {
            Primary = "#cd853f", // Sandy brown
            Secondary = "#f0e68c", // Soft yellow
            Accent = "#dda0dd" // Gentle lavender
        },
        ColorVibrancy = 0.8f,
        AnimationIntensity = "gentle",
        ParticleSystem = "floating_petals",
        ParticleIntensity = 0.6f,
        BackgroundMusic = "peaceful_nature",
        AudioVolume = 0.25f,
        CustomProperties = new Dictionary<string, string>
        {
            ["--wind-animation"] = "gentle",
            ["--flower-sway"] = "slow",
            ["--water-ripple"] = "subtle"
        }
    };

    // Additional templates for all age groups...
    public static EnvironmentConfig ModernCornerOffice => new()
    {
        Name = "Modern Corner Office",
        ThemeId = "modern_office",
        BackgroundImage = "/environments/modern-office-bg.jpg",
        PrimaryColors = new ColorPalette
        {
            Primary = "#263238", // Blue gray
            Secondary = "#37474f", // Light blue gray
            Accent = "#00acc1" // Cyan accent
        },
        AccentColors = new ColorPalette
        {
            Primary = "#ff5722", // Professional orange
            Secondary = "#ffc107", // Amber
            Accent = "#4caf50" // Success green
        },
        ColorVibrancy = 1.0f,
        AnimationIntensity = "professional",
        ParticleSystem = "floating_documents",
        ParticleIntensity = 0.8f,
        BackgroundMusic = "productive_ambient",
        AudioVolume = 0.2f,
        CustomProperties = new Dictionary<string, string>
        {
            ["--window-light"] = "natural",
            ["--document-animation"] = "subtle",
            ["--productivity-indicators"] = "enabled"
        }
    };
}
```

#### **Environment Component**
```razor
@* Components/DynamicEnvironment.razor *@
@using SpinnerNet.App.Services
@inject EnvironmentService EnvironmentService
@inject IJSRuntime JSRuntime
@implements IAsyncDisposable

<div class="dynamic-environment-container @GetEnvironmentClasses()" 
     style="@GetEnvironmentStyles()">
     
    <!-- Background Layer -->
    <div class="environment-background">
        @if (!string.IsNullOrEmpty(_currentEnvironment?.BackgroundVideo))
        {
            <video autoplay muted loop class="background-video">
                <source src="@_currentEnvironment.BackgroundVideo" type="video/mp4">
            </video>
        }
        else if (!string.IsNullOrEmpty(_currentEnvironment?.BackgroundImage))
        {
            <div class="background-image" style="background-image: url('@_currentEnvironment.BackgroundImage')"></div>
        }
    </div>
    
    <!-- Particle System Layer -->
    <div id="particle-system-container" class="particle-system-layer"></div>
    
    <!-- Overlay Effects Layer -->
    <div class="environment-overlay">
        @if (_currentEnvironment?.AnimationIntensity == "celebration")
        {
            <div class="celebration-effects"></div>
        }
        
        @if (_currentEnvironment?.AccessibilityMode == true)
        {
            <div class="accessibility-overlay"></div>
        }
    </div>
    
    <!-- Content Layer -->
    <div class="environment-content">
        @ChildContent
    </div>
    
    <!-- Environment Controls (Debug/Admin) -->
    @if (_showControls)
    {
        <EnvironmentControls CurrentEnvironment="_currentEnvironment" 
                           OnEnvironmentChange="ChangeEnvironmentAsync" />
    }
    
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool ShowControls { get; set; } = false;
    [Parameter] public AgeProfile? UserAge { get; set; }
    [Parameter] public List<string> UserInterests { get; set; } = new();
    
    private EnvironmentConfig? _currentEnvironment;
    private bool _showControls = false;
    private IJSObjectReference? _particleModule;

    protected override async Task OnInitializedAsync()
    {
        EnvironmentService.EnvironmentChanged += OnEnvironmentChanged;
        
        // Load particle system module
        _particleModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/particle-system.js");
        
        // Initialize with default or detected environment
        if (UserAge != null)
        {
            await InitializeEnvironmentAsync();
        }
    }

    private async Task InitializeEnvironmentAsync()
    {
        if (UserAge == null) return;
        
        _currentEnvironment = await EnvironmentService.SelectOptimalEnvironmentAsync(
            UserAge, UserInterests, "neutral");
        
        StateHasChanged();
    }

    private async Task ChangeEnvironmentAsync(EnvironmentConfig newEnvironment)
    {
        await EnvironmentService.ActivateEnvironmentAsync(newEnvironment);
    }

    private void OnEnvironmentChanged(EnvironmentConfig environment)
    {
        _currentEnvironment = environment;
        InvokeAsync(StateHasChanged);
    }

    private string GetEnvironmentClasses()
    {
        if (_currentEnvironment == null) return "environment-loading";
        
        var classes = new List<string>
        {
            "environment-active",
            $"theme-{_currentEnvironment.ThemeId}",
            $"animation-{_currentEnvironment.AnimationIntensity}"
        };
        
        if (_currentEnvironment.AccessibilityMode)
            classes.Add("accessibility-enhanced");
        
        return string.Join(" ", classes);
    }

    private string GetEnvironmentStyles()
    {
        if (_currentEnvironment == null) return "";
        
        var styles = new List<string>
        {
            $"--primary-color: {_currentEnvironment.PrimaryColors.Primary}",
            $"--secondary-color: {_currentEnvironment.PrimaryColors.Secondary}",
            $"--accent-color: {_currentEnvironment.PrimaryColors.Accent}",
            $"--color-vibrancy: {_currentEnvironment.ColorVibrancy}",
            $"--animation-intensity: {GetAnimationIntensityValue()}",
            $"--particle-intensity: {_currentEnvironment.ParticleIntensity}"
        };
        
        // Add custom properties
        foreach (var prop in _currentEnvironment.CustomProperties)
        {
            styles.Add($"{prop.Key}: {prop.Value}");
        }
        
        return string.Join("; ", styles);
    }

    private string GetAnimationIntensityValue()
    {
        return _currentEnvironment?.AnimationIntensity switch
        {
            "minimal" => "0.3",
            "gentle" => "0.6",
            "medium" => "1.0",
            "high" => "1.5",
            "celebration" => "2.0",
            "magical" => "1.8",
            "professional" => "0.8",
            _ => "1.0"
        };
    }

    public async ValueTask DisposeAsync()
    {
        EnvironmentService.EnvironmentChanged -= OnEnvironmentChanged;
        
        if (_particleModule != null)
        {
            await _particleModule.DisposeAsync();
        }
    }
}
```

---

## 📦 **PACKAGE 5: VOICE INTEGRATION**

### **Overview**
Implement comprehensive voice recognition and synthesis system with age-adaptive configurations, accessibility support, and seamless integration with the persona creation flow.

### **Core Features**
- **Web Speech API Integration**: Browser-native speech recognition and synthesis
- **Age-Adaptive Voice Settings**: Optimized configurations for different age groups
- **Multi-language Support**: Voice recognition in multiple languages
- **Accessibility Features**: Voice-first navigation for users with visual impairments
- **Real-time Transcription**: Live speech-to-text with confidence scoring

### **Technical Specifications**

#### **Voice Recognition Service**
```csharp
// Services/VoiceRecognitionService.cs
using Microsoft.JSInterop;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.App.Services;

public class VoiceRecognitionService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _voiceModule;
    private DotNetObjectReference<VoiceRecognitionService>? _dotNetRef;
    
    public event Action<string>? OnTranscriptionUpdate;
    public event Action<string>? OnFinalTranscription;
    public event Action<string>? OnError;
    public event Action? OnStartListening;
    public event Action? OnStopListening;

    public bool IsListening { get; private set; }
    public bool IsSupported { get; private set; }
    public VoiceSettings CurrentSettings { get; private set; } = new();

    public VoiceRecognitionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _voiceModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/voice-recognition.js");
            
            _dotNetRef = DotNetObjectReference.Create(this);
            
            IsSupported = await _voiceModule.InvokeAsync<bool>("checkSupport");
            
            if (IsSupported)
            {
                await _voiceModule.InvokeVoidAsync("initialize", _dotNetRef);
            }
            
            return IsSupported;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to initialize voice recognition: {ex.Message}");
            return false;
        }
    }

    public async Task ConfigureForAgeAsync(AgeProfile ageProfile, string language = "en-US")
    {
        CurrentSettings = CreateAgeAdaptiveSettings(ageProfile, language);
        
        if (_voiceModule != null)
        {
            await _voiceModule.InvokeVoidAsync("configureRecognition", CurrentSettings);
        }
    }

    public async Task StartListeningAsync()
    {
        if (!IsSupported || IsListening || _voiceModule == null)
            return;

        try
        {
            await _voiceModule.InvokeVoidAsync("startListening");
            IsListening = true;
            OnStartListening?.Invoke();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to start listening: {ex.Message}");
        }
    }

    public async Task StopListeningAsync()
    {
        if (!IsListening || _voiceModule == null)
            return;

        try
        {
            await _voiceModule.InvokeVoidAsync("stopListening");
            IsListening = false;
            OnStopListening?.Invoke();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to stop listening: {ex.Message}");
        }
    }

    private VoiceSettings CreateAgeAdaptiveSettings(AgeProfile ageProfile, string language)
    {
        return ageProfile.EstimatedAgeRange switch
        {
            AgeRange.Child => new VoiceSettings
            {
                Language = language,
                Continuous = false,
                InterimResults = false,
                MaxAlternatives = 3,
                Timeout = 10000,
                Confidence = 0.6f,
                Grammar = "simple",
                Encouragement = true,
                SpecialKeywords = new[] { "help", "stop", "parent" }
            },
            
            AgeRange.Teen => new VoiceSettings
            {
                Language = language,
                Continuous = true,
                InterimResults = true,
                MaxAlternatives = 2,
                Timeout = 4000,
                Confidence = 0.7f,
                SlangRecognition = true,
                SocialContext = true
            },
            
            AgeRange.YoungAdult => new VoiceSettings
            {
                Language = language,
                Continuous = true,
                InterimResults = true,
                MaxAlternatives = 2,
                Timeout = 3000,
                Confidence = 0.7f,
                CasualLanguage = true,
                BackgroundNoiseTolerance = "high"
            },
            
            AgeRange.EarlyAdult => new VoiceSettings
            {
                Language = language,
                Continuous = true,
                InterimResults = true,
                MaxAlternatives = 1,
                Timeout = 5000,
                Confidence = 0.8f,
                BusinessTerms = true,
                ProfessionalMode = true
            },
            
            AgeRange.MiddleAdult => new VoiceSettings
            {
                Language = language,
                Continuous = false,
                InterimResults = false,
                MaxAlternatives = 1,
                Timeout = 6000,
                Confidence = 0.85f,
                FormalLanguage = true,
                ClearDiction = true
            },
            
            AgeRange.YoungSenior => new VoiceSettings
            {
                Language = language,
                Continuous = false,
                InterimResults = false,
                MaxAlternatives = 1,
                Timeout = 8000,
                Confidence = 0.9f,
                PatientResponse = true,
                RespectfulTone = true,
                MedicalTerms = true
            },
            
            AgeRange.Senior => new VoiceSettings
            {
                Language = language,
                Continuous = false,
                InterimResults = false,
                MaxAlternatives = 1,
                Timeout = 12000,
                Confidence = 0.95f,
                SlowSpeech = true,
                LoudnessCompensation = true,
                HealthKeywords = true,
                FamilyContext = true
            },
            
            AgeRange.ElderSenior => new VoiceSettings
            {
                Language = language,
                Continuous = false,
                InterimResults = false,
                MaxAlternatives = 1,
                Timeout = 15000,
                Confidence = 0.95f,
                AssistiveMode = true,
                EmergencyKeywords = true,
                CaregiverIntegration = true,
                MemorySupport = true
            },
            
            _ => new VoiceSettings
            {
                Language = language,
                Continuous = true,
                InterimResults = true,
                MaxAlternatives = 2,
                Timeout = 5000,
                Confidence = 0.8f
            }
        };
    }

    [JSInvokable]
    public void OnInterimResult(string text)
    {
        OnTranscriptionUpdate?.Invoke(text);
    }

    [JSInvokable]
    public void OnFinalResult(string text)
    {
        OnFinalTranscription?.Invoke(text);
    }

    [JSInvokable]
    public void OnRecognitionError(string error)
    {
        IsListening = false;
        OnError?.Invoke(error);
    }

    [JSInvokable]
    public void OnRecognitionStart()
    {
        IsListening = true;
        OnStartListening?.Invoke();
    }

    [JSInvokable]
    public void OnRecognitionEnd()
    {
        IsListening = false;
        OnStopListening?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (IsListening)
        {
            await StopListeningAsync();
        }
        
        if (_voiceModule != null)
        {
            await _voiceModule.DisposeAsync();
        }
        
        _dotNetRef?.Dispose();
    }
}

public class VoiceSettings
{
    public string Language { get; set; } = "en-US";
    public bool Continuous { get; set; } = true;
    public bool InterimResults { get; set; } = true;
    public int MaxAlternatives { get; set; } = 2;
    public int Timeout { get; set; } = 5000;
    public float Confidence { get; set; } = 0.8f;
    public string? Grammar { get; set; }
    public bool Encouragement { get; set; } = false;
    public string[]? SpecialKeywords { get; set; }
    public bool SlangRecognition { get; set; } = false;
    public bool SocialContext { get; set; } = false;
    public bool CasualLanguage { get; set; } = false;
    public string BackgroundNoiseTolerance { get; set; } = "medium";
    public bool BusinessTerms { get; set; } = false;
    public bool ProfessionalMode { get; set; } = false;
    public bool FormalLanguage { get; set; } = false;
    public bool ClearDiction { get; set; } = false;
    public bool PatientResponse { get; set; } = false;
    public bool RespectfulTone { get; set; } = false;
    public bool MedicalTerms { get; set; } = false;
    public bool SlowSpeech { get; set; } = false;
    public bool LoudnessCompensation { get; set; } = false;
    public bool HealthKeywords { get; set; } = false;
    public bool FamilyContext { get; set; } = false;
    public bool AssistiveMode { get; set; } = false;
    public bool EmergencyKeywords { get; set; } = false;
    public bool CaregiverIntegration { get; set; } = false;
    public bool MemorySupport { get; set; } = false;
}
```

#### **Voice Recognition JavaScript Module**
```javascript
// wwwroot/js/voice-recognition.js
let recognition = null;
let dotNetHelper = null;
let currentSettings = null;
let isListening = false;

export function checkSupport() {
    return 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
}

export function initialize(dotNetRef) {
    dotNetHelper = dotNetRef;
    
    if (!checkSupport()) {
        return false;
    }
    
    // Initialize SpeechRecognition
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    recognition = new SpeechRecognition();
    
    setupEventHandlers();
    return true;
}

export function configureRecognition(settings) {
    if (!recognition) return;
    
    currentSettings = settings;
    
    recognition.lang = settings.language;
    recognition.continuous = settings.continuous;
    recognition.interimResults = settings.interimResults;
    recognition.maxAlternatives = settings.maxAlternatives;
    
    // Age-specific configurations
    if (settings.slowSpeech) {
        // Allow longer pauses for seniors
        recognition.grammars = createSeniorGrammar();
    }
    
    if (settings.assistiveMode) {
        // Enable accessibility features
        enableAssistiveFeatures(settings);
    }
    
    if (settings.emergencyKeywords) {
        // Set up emergency keyword detection
        setupEmergencyDetection();
    }
}

export function startListening() {
    if (!recognition || isListening) return;
    
    try {
        recognition.start();
        isListening = true;
    } catch (error) {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnRecognitionError', error.message);
        }
    }
}

export function stopListening() {
    if (!recognition || !isListening) return;
    
    try {
        recognition.stop();
        isListening = false;
    } catch (error) {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnRecognitionError', error.message);
        }
    }
}

function setupEventHandlers() {
    if (!recognition) return;
    
    recognition.onstart = () => {
        isListening = true;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnRecognitionStart');
        }
    };
    
    recognition.onend = () => {
        isListening = false;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnRecognitionEnd');
        }
    };
    
    recognition.onresult = (event) => {
        let interimTranscript = '';
        let finalTranscript = '';
        
        for (let i = event.resultIndex; i < event.results.length; i++) {
            const transcript = event.results[i][0].transcript;
            
            if (event.results[i].isFinal) {
                finalTranscript += transcript;
            } else {
                interimTranscript += transcript;
            }
        }
        
        // Check confidence threshold
        if (event.results[event.resultIndex] && 
            event.results[event.resultIndex][0].confidence < (currentSettings?.confidence || 0.8)) {
            // Low confidence - request repeat for seniors
            if (currentSettings?.assistiveMode) {
                requestRepeat();
            }
        }
        
        if (interimTranscript && currentSettings?.interimResults && dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnInterimResult', interimTranscript);
        }
        
        if (finalTranscript && dotNetHelper) {
            // Process age-specific enhancements
            const processedTranscript = processAgeSpecificTranscript(finalTranscript);
            dotNetHelper.invokeMethodAsync('OnFinalResult', processedTranscript);
        }
    };
    
    recognition.onerror = (event) => {
        isListening = false;
        let errorMessage = event.error;
        
        // Age-specific error handling
        if (currentSettings?.assistiveMode) {
            errorMessage = makeErrorUserFriendly(event.error);
        }
        
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnRecognitionError', errorMessage);
        }
    };
    
    recognition.onnomatch = () => {
        if (currentSettings?.encouragement && dotNetHelper) {
            // Encourage children to try again
            dotNetHelper.invokeMethodAsync('OnRecognitionError', 'I didn\'t quite catch that. Can you try again?');
        }
    };
}

function processAgeSpecificTranscript(transcript) {
    if (!currentSettings) return transcript;
    
    let processed = transcript;
    
    // Slang recognition for teens
    if (currentSettings.slangRecognition) {
        processed = expandSlang(processed);
    }
    
    // Professional terminology for adults
    if (currentSettings.businessTerms) {
        processed = expandBusinessTerms(processed);
    }
    
    // Medical terminology for seniors
    if (currentSettings.medicalTerms) {
        processed = expandMedicalTerms(processed);
    }
    
    return processed;
}

function expandSlang(text) {
    const slangMap = {
        'ur': 'your',
        'u': 'you',
        'omg': 'oh my god',
        'tbh': 'to be honest',
        'rn': 'right now',
        'imo': 'in my opinion',
        'irl': 'in real life'
    };
    
    let expanded = text;
    for (const [slang, expansion] of Object.entries(slangMap)) {
        const regex = new RegExp(`\\b${slang}\\b`, 'gi');
        expanded = expanded.replace(regex, expansion);
    }
    
    return expanded;
}

function expandBusinessTerms(text) {
    const businessMap = {
        'roi': 'return on investment',
        'kpi': 'key performance indicator',
        'b2b': 'business to business',
        'b2c': 'business to consumer',
        'crm': 'customer relationship management'
    };
    
    let expanded = text;
    for (const [term, expansion] of Object.entries(businessMap)) {
        const regex = new RegExp(`\\b${term}\\b`, 'gi');
        expanded = expanded.replace(regex, expansion);
    }
    
    return expanded;
}

function expandMedicalTerms(text) {
    const medicalMap = {
        'bp': 'blood pressure',
        'meds': 'medications',
        'doc': 'doctor',
        'appt': 'appointment'
    };
    
    let expanded = text;
    for (const [term, expansion] of Object.entries(medicalMap)) {
        const regex = new RegExp(`\\b${term}\\b`, 'gi');
        expanded = expanded.replace(regex, expansion);
    }
    
    return expanded;
}

function enableAssistiveFeatures(settings) {
    // Longer timeout for seniors
    if (settings.timeout > 10000) {
        recognition.grammars = createSeniorGrammar();
    }
    
    // Emergency keyword detection
    if (settings.emergencyKeywords) {
        setupEmergencyDetection();
    }
}

function setupEmergencyDetection() {
    const emergencyKeywords = ['help', 'emergency', 'call 911', 'medical emergency', 'fall', 'chest pain'];
    
    recognition.addEventListener('result', (event) => {
        const transcript = event.results[event.resultIndex][0].transcript.toLowerCase();
        
        for (const keyword of emergencyKeywords) {
            if (transcript.includes(keyword)) {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnEmergencyKeywordDetected', keyword);
                }
                break;
            }
        }
    });
}

function requestRepeat() {
    // Provide gentle feedback for low confidence recognition
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnRecognitionError', 'I didn\'t hear that clearly. Could you please repeat?');
    }
}

function makeErrorUserFriendly(error) {
    switch (error) {
        case 'no-speech':
            return 'I didn\'t hear anything. Please try speaking again.';
        case 'audio-capture':
            return 'Microphone not available. Please check your microphone settings.';
        case 'not-allowed':
            return 'Microphone access was denied. Please allow microphone access to use voice features.';
        case 'network':
            return 'Network error occurred. Please check your internet connection.';
        default:
            return 'Voice recognition error occurred. Please try again.';
    }
}

function createSeniorGrammar() {
    // Create grammar that's more tolerant of slower speech
    if ('SpeechGrammarList' in window) {
        const grammar = '#JSGF V1.0; grammar commands; public <command> = yes | no | help | stop | continue | repeat;';
        const speechRecognitionList = new SpeechGrammarList();
        speechRecognitionList.addFromString(grammar, 1);
        return speechRecognitionList;
    }
    return null;
}

// Cleanup function
export function cleanup() {
    if (recognition) {
        recognition.abort();
        recognition = null;
    }
    dotNetHelper = null;
    currentSettings = null;
    isListening = false;
}
```

#### **Voice Input Component**
```razor
@* Components/AdaptiveVoiceInput.razor *@
@using SpinnerNet.App.Services
@inject VoiceRecognitionService VoiceService
@inject ILocalizationService LocalizationService
@implements IAsyncDisposable

<div class="adaptive-voice-input @GetInputClasses()">
    
    <MudStack Row="true" Spacing="2" AlignItems="Center">
        
        <!-- Text Input -->
        <MudTextField @bind-Value="@TextValue"
                      Label="@Placeholder"
                      Variant="Variant.Outlined"
                      Class="voice-text-input"
                      Style="@GetTextInputStyles()"
                      ReadOnly="@IsListening"
                      OnKeyPress="@HandleKeyPress"
                      Adornment="@(VoiceEnabled ? Adornment.End : Adornment.None)"
                      AdornmentIcon="@GetMicrophoneIcon()"
                      OnAdornmentClick="@ToggleVoiceAsync" />
        
        <!-- Voice Button (separate for better accessibility) -->
        @if (VoiceEnabled && !IsEmbeddedMicrophone)
        {
            <MudButton Variant="Variant.Filled"
                       Color="@GetVoiceButtonColor()"
                       Size="@GetButtonSize()"
                       Style="@GetVoiceButtonStyles()"
                       OnClick="@ToggleVoiceAsync"
                       Disabled="@(!IsVoiceSupported)"
                       Class="voice-toggle-button">
                
                <MudIcon Icon="@GetMicrophoneIcon()" />
                
                @if (ShowButtonText)
                {
                    <MudText Class="ml-2">@GetVoiceButtonText()</MudText>
                }
                
            </MudButton>
        }
        
        <!-- Send Button -->
        @if (ShowSendButton)
        {
            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       Size="@GetButtonSize()"
                       Style="@GetSendButtonStyles()"
                       OnClick="@HandleSubmitAsync"
                       Disabled="@(string.IsNullOrWhiteSpace(TextValue) || IsListening)">
                <MudIcon Icon="@Icons.Material.Filled.Send" />
                @if (ShowButtonText)
                {
                    <MudText Class="ml-2">@LocalizationService.GetString("VoiceInput_Send")</MudText>
                }
            </MudButton>
        }
        
    </MudStack>
    
    <!-- Voice Status Indicator -->
    @if (IsListening || !string.IsNullOrEmpty(_voiceStatus))
    {
        <div class="voice-status mt-2">
            <MudAlert Severity="@GetStatusSeverity()" 
                      Dense="true"
                      Class="voice-status-alert">
                
                @if (IsListening)
                {
                    <MudStack Row="true" Spacing="2" AlignItems="Center">
                        <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                        <MudText Typo="Typo.body2">@LocalizationService.GetString("VoiceInput_Listening")</MudText>
                    </MudStack>
                }
                else
                {
                    <MudText Typo="Typo.body2">@_voiceStatus</MudText>
                }
                
            </MudAlert>
        </div>
    }
    
    <!-- Live Transcription (for supported age groups) -->
    @if (ShowLiveTranscription && !string.IsNullOrEmpty(_interimTranscript))
    {
        <div class="live-transcription mt-2">
            <MudPaper Class="interim-transcript pa-2" Elevation="1">
                <MudText Typo="Typo.body2" Style="color: #666; font-style: italic;">
                    @_interimTranscript
                </MudText>
            </MudPaper>
        </div>
    }
    
</div>

@code {
    [Parameter] public string TextValue { get; set; } = "";
    [Parameter] public EventCallback<string> TextValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnSubmit { get; set; }
    [Parameter] public string Placeholder { get; set; } = "";
    [Parameter] public bool VoiceEnabled { get; set; } = true;
    [Parameter] public bool ShowSendButton { get; set; } = true;
    [Parameter] public bool ShowButtonText { get; set; } = false;
    [Parameter] public bool IsEmbeddedMicrophone { get; set; } = true;
    [Parameter] public AgeRange UserAge { get; set; } = AgeRange.Unknown;
    [Parameter] public string Language { get; set; } = "en-US";
    
    private bool IsVoiceSupported => VoiceService.IsSupported;
    private bool IsListening => VoiceService.IsListening;
    private bool ShowLiveTranscription => UserAge <= AgeRange.EarlyAdult; // Younger users prefer live feedback
    private bool ShowButtonText => UserAge >= AgeRange.YoungSenior; // Older users prefer text labels
    
    private string _voiceStatus = "";
    private string _interimTranscript = "";

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to voice service events
        VoiceService.OnTranscriptionUpdate += OnTranscriptionUpdate;
        VoiceService.OnFinalTranscription += OnFinalTranscription;
        VoiceService.OnError += OnVoiceError;
        VoiceService.OnStartListening += OnStartListening;
        VoiceService.OnStopListening += OnStopListening;
        
        // Initialize voice service
        if (VoiceEnabled && !IsVoiceSupported)
        {
            await VoiceService.InitializeAsync();
        }
        
        // Configure for user's age
        if (IsVoiceSupported)
        {
            await VoiceService.ConfigureForAgeAsync(new AgeProfile 
            { 
                EstimatedAgeRange = UserAge 
            }, Language);
        }
    }

    private async Task ToggleVoiceAsync()
    {
        if (!IsVoiceSupported) return;
        
        if (IsListening)
        {
            await VoiceService.StopListeningAsync();
        }
        else
        {
            // Clear current text when starting voice input
            TextValue = "";
            await TextValueChanged.InvokeAsync(TextValue);
            
            await VoiceService.StartListeningAsync();
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (!string.IsNullOrWhiteSpace(TextValue))
        {
            await OnSubmit.InvokeAsync(TextValue);
            TextValue = "";
            await TextValueChanged.InvokeAsync(TextValue);
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !IsListening)
        {
            await HandleSubmitAsync();
        }
    }

    private void OnTranscriptionUpdate(string transcript)
    {
        _interimTranscript = transcript;
        InvokeAsync(StateHasChanged);
    }

    private async void OnFinalTranscription(string transcript)
    {
        TextValue = transcript;
        _interimTranscript = "";
        await TextValueChanged.InvokeAsync(TextValue);
        InvokeAsync(StateHasChanged);
    }

    private void OnVoiceError(string error)
    {
        _voiceStatus = error;
        InvokeAsync(StateHasChanged);
        
        // Clear error after a delay
        _ = Task.Delay(3000).ContinueWith(_ => 
        {
            _voiceStatus = "";
            InvokeAsync(StateHasChanged);
        });
    }

    private void OnStartListening()
    {
        _voiceStatus = "";
        InvokeAsync(StateHasChanged);
    }

    private void OnStopListening()
    {
        InvokeAsync(StateHasChanged);
    }

    // Age-adaptive styling methods
    private string GetInputClasses()
    {
        var classes = new List<string> { "adaptive-voice-input" };
        
        classes.Add(UserAge switch
        {
            AgeRange.Child => "child-voice-input",
            AgeRange.Teen => "teen-voice-input",
            AgeRange.YoungAdult => "young-adult-voice-input",
            AgeRange.EarlyAdult => "early-adult-voice-input",
            AgeRange.MiddleAdult => "middle-adult-voice-input",
            AgeRange.YoungSenior => "young-senior-voice-input",
            AgeRange.Senior => "senior-voice-input",
            AgeRange.ElderSenior => "elder-senior-voice-input",
            _ => "default-voice-input"
        });
        
        if (IsListening)
            classes.Add("listening-active");
        
        return string.Join(" ", classes);
    }

    private string GetTextInputStyles()
    {
        return UserAge switch
        {
            AgeRange.Child => "font-size: 1.1rem; min-height: 56px;",
            AgeRange.Teen => "font-size: 1rem; min-height: 48px;",
            AgeRange.YoungAdult => "font-size: 0.95rem; min-height: 44px;",
            AgeRange.EarlyAdult => "font-size: 0.9rem; min-height: 40px;",
            AgeRange.MiddleAdult => "font-size: 0.95rem; min-height: 42px;",
            AgeRange.YoungSenior => "font-size: 1.05rem; min-height: 50px;",
            AgeRange.Senior => "font-size: 1.2rem; min-height: 56px;",
            AgeRange.ElderSenior => "font-size: 1.3rem; min-height: 64px;",
            _ => "font-size: 0.9rem; min-height: 40px;"
        };
    }

    private Size GetButtonSize()
    {
        return UserAge switch
        {
            AgeRange.Child => Size.Large,
            AgeRange.Senior => Size.Large,
            AgeRange.ElderSenior => Size.Large,
            _ => Size.Medium
        };
    }

    private Color GetVoiceButtonColor()
    {
        if (IsListening) return Color.Error;
        return UserAge <= AgeRange.Teen ? Color.Secondary : Color.Primary;
    }

    private string GetMicrophoneIcon()
    {
        return IsListening ? Icons.Material.Filled.MicOff : Icons.Material.Filled.Mic;
    }

    private string GetVoiceButtonText()
    {
        return IsListening 
            ? LocalizationService.GetString("VoiceInput_StopListening")
            : LocalizationService.GetString("VoiceInput_StartListening");
    }

    private Severity GetStatusSeverity()
    {
        if (IsListening) return Severity.Info;
        return string.IsNullOrEmpty(_voiceStatus) ? Severity.Success : Severity.Warning;
    }

    public async ValueTask DisposeAsync()
    {
        VoiceService.OnTranscriptionUpdate -= OnTranscriptionUpdate;
        VoiceService.OnFinalTranscription -= OnFinalTranscription;
        VoiceService.OnError -= OnVoiceError;
        VoiceService.OnStartListening -= OnStartListening;
        VoiceService.OnStopListening -= OnStopListening;
        
        if (IsListening)
        {
            await VoiceService.StopListeningAsync();
        }
    }
}
```

---

## 🏗️ **PHASE 2 TECHNICAL ARCHITECTURE**

### **System Architecture Diagram**
```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Client                            │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Enhanced      │  │    Dynamic      │  │    Voice     │ │
│  │   Persona       │  │  Environment    │  │ Integration  │ │
│  │   Creation      │  │    System       │  │              │ │
│  │                 │  │                 │  │              │ │
│  │ • Multi-phase   │  │ • Age-specific  │  │ • Web Speech │ │
│  │   Interview     │  │   Themes        │  │   API        │ │
│  │ • AI Analysis   │  │ • Particle      │  │ • Age-       │ │
│  │ • Real-time     │  │   Systems       │  │   Adaptive   │ │
│  │   Insights      │  │ • Mood Response │  │   Settings   │ │
│  │ • Progress      │  │ • Cultural      │  │ • Real-time  │ │
│  │   Tracking      │  │   Adaptation    │  │   Speech     │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
│                            ↓                               │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              Foundation (Phase 1)                      │ │
│  │        WebLLM Engine + Age-Adaptive UI                 │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### **Component Integration Flow**
```
User Input → Voice/Text Processing → AI Analysis → Environment Update
     ↓              ↓                    ↓              ↓
Age Detection → Voice Config → Persona Insights → Theme Adaptation
     ↓              ↓                    ↓              ↓
UI Adaptation → Accessibility → Progress Update → Visual Feedback
```

---

## 🧪 **TESTING REQUIREMENTS**

### **Unit Tests**
- **PersonaInterviewService**: Multi-phase flow, AI analysis, insight extraction
- **EnvironmentService**: Environment selection, mood adaptation, accessibility
- **VoiceRecognitionService**: Age-adaptive settings, error handling, transcription
- **Components**: User interactions, state management, accessibility

### **Integration Tests**
- **Interview + Environment**: Environment changes based on interview progress
- **Voice + Text**: Seamless switching between input methods
- **Age Adaptation**: All components respond consistently to age changes
- **Accessibility**: Screen reader and keyboard navigation support

### **User Experience Tests**
- **Age-Specific Flows**: Test complete flows for each age group
- **Voice Recognition**: Accuracy across different age groups and accents
- **Environment Performance**: Smooth transitions and animations
- **Error Handling**: Graceful degradation when features unavailable

---

## 📋 **IMPLEMENTATION TIMELINE**

### **Week 1-3: Enhanced Persona Creation**
- [ ] Multi-phase interview service
- [ ] AI analysis and insight extraction
- [ ] Progress tracking and visualization
- [ ] Interview flow components
- [ ] Testing and optimization

### **Week 4-6: Dynamic Environment System**
- [ ] Environment service and templates
- [ ] Age-specific environment selection
- [ ] Particle systems and animations
- [ ] Cultural and mood adaptations
- [ ] Performance optimization

### **Week 7-8: Voice Integration**
- [ ] Voice recognition service
- [ ] Age-adaptive voice settings
- [ ] Voice input components
- [ ] Accessibility features
- [ ] Cross-browser testing

### **Week 9-10: Integration & Polish**
- [ ] Full system integration
- [ ] Performance optimization
- [ ] Accessibility compliance
- [ ] User experience testing
- [ ] Documentation and deployment

---

## 🎯 **SUCCESS CRITERIA**

### **Functional Requirements**
- [ ] Complete 4-phase interview flow with AI analysis
- [ ] Age-appropriate environments for all 8 age groups
- [ ] Voice recognition working in supported browsers
- [ ] Real-time persona insights generation
- [ ] Seamless integration between all components

### **Performance Requirements**
- [ ] Interview processing under 2 seconds per response
- [ ] Environment transitions under 500ms
- [ ] Voice recognition accuracy >85% for target age groups
- [ ] No memory leaks during extended sessions
- [ ] Responsive design across all device sizes

### **Quality Requirements**
- [ ] 90%+ unit test coverage for all services
- [ ] WCAG 2.1 AA accessibility compliance
- [ ] Cross-browser compatibility verified
- [ ] User experience validated with target age groups
- [ ] Performance benchmarks met

---

*This Phase 2 PRP builds upon Phase 1 foundation to deliver core user-facing features. Success here creates a complete, usable persona creation system ready for advanced enhancements in Phase 3.*