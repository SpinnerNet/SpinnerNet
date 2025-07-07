# Create Proper Packages Command

## Description
Create proper package structure with comments for SpinnerNet components following the established patterns and conventions.

## Command
```bash
# Create package structure for Phase 1 Foundation implementation
mkdir -p src/SpinnerNet.App/Services/WebLLM
mkdir -p src/SpinnerNet.App/Components/AgeAdaptive
mkdir -p src/SpinnerNet.App/Components/PersonaCreation
mkdir -p src/SpinnerNet.App/Themes/AgeAdaptive
mkdir -p src/SpinnerNet.Core/Features/PersonaCreation/Commands
mkdir -p src/SpinnerNet.Core/Features/PersonaCreation/Queries
mkdir -p src/SpinnerNet.Core/Features/PersonaCreation/Validators
mkdir -p src/SpinnerNet.Core/Extensions/WebLLM
mkdir -p tests/SpinnerNet.App.Tests/Services/WebLLM
mkdir -p tests/SpinnerNet.App.Tests/Components/AgeAdaptive
mkdir -p tests/SpinnerNet.Core.Tests/Features/PersonaCreation

# Create package documentation files
cat > src/SpinnerNet.App/Services/WebLLM/README.md << 'EOF'
# WebLLM Service Package

## Overview
Client-side AI processing services for SpinnerNet using WebLLM with Hermes-2-Pro-Mistral-7B model.

## Components
- `WebLLMService.cs`: Main service for WebLLM integration
- `WebLLMOptions.cs`: Configuration options and models
- `WebLLMStatusEventArgs.cs`: Event arguments for status updates

## Features
- Client-side AI processing for privacy
- Age-adaptive system prompts
- Streaming response support
- Error handling and fallbacks
- JavaScript interop with proper disposal

## Usage
```csharp
// Register in Program.cs
builder.Services.AddWebLLMServices();

// Use in components
@inject IWebLLMService WebLLMService
```

## Dependencies
- Microsoft.JSInterop
- SpinnerNet.Shared.Localization
- WebLLM JavaScript library (@mlc-ai/web-llm)
EOF

cat > src/SpinnerNet.App/Components/AgeAdaptive/README.md << 'EOF'
# Age-Adaptive Components Package

## Overview
Age-adaptive UI components that automatically adjust styling, fonts, and interactions based on user age.

## Components
- `AgeAdaptiveContainer.razor`: Main container with age-based styling
- `AgeAdaptiveButton.razor`: Age-appropriate button sizing and styling
- `AgeAdaptiveText.razor`: Age-appropriate text sizing and fonts

## Age Groups
- **Children (<13)**: Larger fonts, playful colors, simple language
- **Teens (13-17)**: Modern design, vibrant colors, engaging UI
- **Adults (18-64)**: Professional design, standard sizing
- **Seniors (65+)**: Larger fonts, high contrast, simplified interactions

## Features
- Automatic font scaling based on age
- Age-appropriate color schemes
- Touch-friendly sizing for children and seniors
- Accessibility compliance

## Usage
```razor
<AgeAdaptiveContainer UserAge="@userAge">
    <AgeAdaptiveText>Your content here</AgeAdaptiveText>
</AgeAdaptiveContainer>
```
EOF

cat > src/SpinnerNet.App/Components/PersonaCreation/README.md << 'EOF'
# Persona Creation Components Package

## Overview
Complete persona creation wizard with age-adaptive questions and WebLLM integration.

## Components
- `PersonaCreationWizard.razor`: Main wizard component
- `PersonaBasicInfo.razor`: Basic persona information form
- `PersonaInterviewQuestions.razor`: Age-adaptive interview questions
- `PersonaAIGeneration.razor`: AI-powered persona generation
- `PersonaReview.razor`: Review and finalization component

## Features
- Multi-step wizard with validation
- Age-adaptive questions and prompts
- Real-time AI generation with WebLLM
- Progress tracking and status updates
- Error handling and recovery

## Models
- `PersonaBasicInfoModel`: Basic persona data
- `GeneratedPersonaModel`: AI-generated persona traits
- `FinalPersonaModel`: Complete persona configuration
- `PersonaCreationResult`: Creation result with success status

## Usage
```razor
<PersonaCreationWizard 
    UserId="@userId" 
    UserAge="@userAge" 
    OnPersonaCreated="HandlePersonaCreated" />
```
EOF

cat > src/SpinnerNet.Core/Features/PersonaCreation/README.md << 'EOF'
# Persona Creation Features Package

## Overview
Vertical slice implementation of persona creation features using MediatR, FluentValidation, and Cosmos DB.

## Structure
- `Commands/`: Command handlers for persona creation operations
- `Queries/`: Query handlers for persona retrieval
- `Validators/`: FluentValidation validators for all operations
- `Models/`: Domain models and DTOs

## Commands
- `InitializePersonaCreationCommand`: Start persona creation session
- `SavePersonaProgressCommand`: Save interview progress
- `FinalizePersonaCommand`: Complete persona creation

## Queries
- `GetPersonaCreationSessionQuery`: Retrieve session data
- `GetUserPersonasQuery`: Get all user personas

## Features
- Age-adaptive question generation
- Session state management
- Progress tracking
- Validation with FluentValidation
- Cosmos DB integration with Microsoft NoSQL patterns

## Usage
```csharp
// In controllers or components
var result = await _mediator.Send(new InitializePersonaCreationCommand(userId, userAge));
```
EOF

cat > tests/SpinnerNet.Core.Tests/Features/PersonaCreation/README.md << 'EOF'
# Persona Creation Tests Package

## Overview
Comprehensive test suite for persona creation features with >90% code coverage.

## Test Categories
- **Unit Tests**: Individual component and service tests
- **Integration Tests**: End-to-end feature tests
- **Validation Tests**: FluentValidation rule tests
- **Edge Cases**: Error conditions and boundary tests

## Test Files
- `InitializePersonaCreationHandlerTests.cs`: Command handler tests
- `InitializePersonaCreationValidatorTests.cs`: Validation tests
- `PersonaCreationServiceTests.cs`: Service layer tests
- `PersonaCreationIntegrationTests.cs`: End-to-end tests

## Features
- Moq for mocking dependencies
- xUnit test framework
- Test data builders for complex objects
- Comprehensive edge case coverage
- Performance benchmarking

## Coverage Requirements
- Minimum 90% code coverage
- All public methods tested
- All validation rules tested
- All error conditions tested
EOF

echo "✅ Package structure created successfully!"
echo "📁 Created directories and documentation for:"
echo "   - WebLLM Services"
echo "   - Age-Adaptive Components"
echo "   - Persona Creation Features"
echo "   - Test Packages"
echo ""
echo "📝 Each package includes:"
echo "   - Proper directory structure"
echo "   - README.md with usage examples"
echo "   - Component descriptions"
echo "   - Dependency information"
```

## Package Structure Created
```
src/SpinnerNet.App/
├── Services/WebLLM/           # WebLLM integration services
├── Components/AgeAdaptive/    # Age-adaptive UI components
├── Components/PersonaCreation/ # Persona creation wizard
└── Themes/AgeAdaptive/        # Age-adaptive theme system

src/SpinnerNet.Core/
├── Features/PersonaCreation/  # Vertical slice features
│   ├── Commands/              # MediatR command handlers
│   ├── Queries/               # MediatR query handlers
│   └── Validators/            # FluentValidation validators
└── Extensions/WebLLM/         # Service registration extensions

tests/
├── SpinnerNet.App.Tests/      # App layer tests
└── SpinnerNet.Core.Tests/     # Core layer tests
```

## Usage
Run this command to:
- Create proper package directory structure
- Generate README documentation for each package
- Follow SpinnerNet architectural patterns
- Prepare for Phase 1 Foundation implementation