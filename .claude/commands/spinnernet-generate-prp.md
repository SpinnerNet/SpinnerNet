# Create SpinnerNet PRP

YOU MUST DO IN-DEPTH RESEARCH, FOLLOW THE <RESEARCH PROCESS>

<RESEARCH PROCESS>

   - Don't only research one page, and don't use your own webscraping tool - instead scrape many relevant pages from all documentation links mentioned in the INITIAL.md file
   - Take my tech as sacred truth, for example if I say a model name then research that model name for LLM usage - don't assume from your own knowledge at any point
   - When I say don't just research one page, I mean do incredibly in-depth research, like to the point where it's just absolutely ridiculous how much research you've actually done, then when you create the PRP document you need to put absolutely everything into that including INCREDIBLY IN DEPTH CODE EXAMPLES so any AI can pick up your PRP and generate WORKING and COMPLETE production ready code.
   - ALWAYS use Context7 MCP documentation lookup FIRST before any web search
   - Research WebLLM, MudBlazor, Blazor Server, Semantic Kernel, Cosmos DB patterns specific to SpinnerNet

</RESEARCH PROCESS>

## Feature file: $ARGUMENTS

Generate a complete SpinnerNet PRP for feature implementation with thorough research specific to SpinnerNet's architecture. Ensure context is passed to the AI agent to enable self-validation and iterative refinement. Read the feature file first to understand what needs to be created, how the examples provided help, and any other considerations.

The AI agent only gets the context you are appending to the PRP and training data. Assume the AI agent has access to the codebase and the same knowledge cutoff as you, so it's important that your research findings are included or referenced in the PRP. The Agent has WebSearch capabilities, so pass URLs to documentation and examples.

## SpinnerNet Research Process

1. **MANDATORY: Read Foundation Documents**
   - Read CLAUDE.md for current project state and critical rules
   - Read INITIAL.md for project vision and Semantic Kernel integration details
   - Read docs/ARCHITECTURE.md for system design patterns
   - Read docs/VERTICAL_SLICE_ARCHITECTURE.md for implementation patterns
   - Read docs/AI_INTEGRATION_GUIDE.md for AI integration patterns
   - Read docs/Age-Adaptive-UI-Implementation.md for age adaptation requirements
   - Read docs/DATA_MODEL_DESIGN.md for Cosmos DB patterns
   - Read docs/PROJECT_STRUCTURE.md for file organization standards
   - Read docs/DEVELOPMENT.md for development workflow
   - Read docs/AZURE_DEPLOYMENT_GUIDE.md for deployment patterns
   - Note: Phase documents are temporary input examples only, not foundation

2. **Codebase Analysis (SpinnerNet Specific)**
   - Search for similar features/patterns in src/ directory
   - Identify Cosmos DB document patterns in SpinnerNet.Shared/Models/CosmosDb/
   - Check existing MediatR handlers in SpinnerNet.Core/Features/
   - Review Blazor components in SpinnerNet.App/Components/
   - Examine WebLLM integration in wwwroot/js/webllm-integration.js
   - Check localization patterns in SpinnerNet.Shared/Resources/
   - Note MudBlazor theming in SpinnerNet.App/Themes/
   - Review existing service patterns and dependency injection setup

3. **External Research (Context7 MCP First - COMPREHENSIVE)**
   
   **MANDATORY Library Documentation (Use mcp__context7__resolve-library-id for each):**
   - WebLLM: "@mlc-ai/web-llm" - Client-side AI processing
   - MudBlazor: "MudBlazor" - UI component library and theming
   - Blazor Server: "Microsoft.AspNetCore.Components" - Server-side Blazor patterns
   - Semantic Kernel: "Microsoft.SemanticKernel" - AI orchestration and reasoning
   - MediatR: "MediatR" - Vertical slice architecture patterns
   - Cosmos DB: "Microsoft.Azure.Cosmos" - NoSQL database patterns
   - FluentValidation: "FluentValidation" - Input validation patterns
   - Azure Identity: "Azure.Identity" - Authentication and authorization
   - Azure KeyVault: "Azure.Security.KeyVault.Secrets" - Secrets management
   - Microsoft Identity Web: "Microsoft.Identity.Web" - Azure AD integration
   - SignalR: "Microsoft.AspNetCore.SignalR" - Real-time communication
   - Entity Framework: "Microsoft.EntityFrameworkCore" - Data access patterns
   - Localization: "Microsoft.Extensions.Localization" - Internationalization
   - Dependency Injection: "Microsoft.Extensions.DependencyInjection" - Service registration
   - bUnit: "bUnit" - Blazor component testing framework
   - OpenTelemetry: "OpenTelemetry" - Monitoring and telemetry
   
   **Research Depth Requirements:**
   - Get comprehensive documentation for each library above
   - Focus on integration patterns, best practices, and common pitfalls
   - Include code examples and configuration patterns
   - Document version compatibility and breaking changes
   - Only use WebSearch as fallback if Context7 doesn't have the specific library

4. **SpinnerNet Architecture Validation**
   - Verify vertical slice architecture compliance
   - Check Microsoft NoSQL patterns (PascalCase, no JsonPropertyName)
   - Validate MudBlazor design system adherence
   - Confirm age-adaptive UI considerations
   - Review TypeLeap ultra-low latency requirements

5. **Azure Integration Testing Strategy (Safari MCP)**
   - Plan for testing deployed Azure applications using Safari MCP tools
   - Define WebLLM browser compatibility testing across different environments
   - Design age-adaptive UI validation in real Azure deployments
   - Plan real-time feature testing (SignalR, TypeLeap interface)
   - Include performance testing on Azure infrastructure
   - Test localization across different cultures in live environment

6. **User Clarification** (if needed)
   - Specific SpinnerNet patterns to mirror and where to find them?
   - Integration requirements with WebLLM/Semantic Kernel?
   - Age-adaptive UI requirements for this feature?
   - Azure deployment and testing requirements?

## SpinnerNet PRP Generation

Generate 3 phases following SpinnerNet patterns:

**Phase 1**: Skeleton Code with detailed implementation comments on exactly how to implement it using SpinnerNet architecture
**Phase 2**: Full and complete production ready code following SpinnerNet conventions
**Phase 3**: Unit Tests using SpinnerNet testing patterns

Using PRPs/templates/spinnernet_prp_base.md as template:

### Critical SpinnerNet Context to Include
- **Documentation**: Context7 URLs with specific sections for all libraries
- **Code Examples**: Real snippets from SpinnerNet codebase showing patterns
- **Architecture**: Vertical slice structure, MediatR patterns, Cosmos DB Microsoft NoSQL
- **Gotchas**: WebLLM browser compatibility, MudBlazor theming, Cosmos partitioning
- **Patterns**: Existing SpinnerNet component and service approaches
- **Localization**: ILocalizationService usage with .resx files
- **AI Integration**: WebLLM client-side + Semantic Kernel server-side hybrid

### SpinnerNet Implementation Blueprint
- Start with Cosmos document model (Microsoft NoSQL pattern)
- Create MediatR command/query handlers (vertical slice)
- Build Blazor component with MudBlazor UI
- Add localization strings to .resx files
- Integrate WebLLM/Semantic Kernel (if AI features)
- Include error handling and logging strategies
- List tasks in correct implementation order for SpinnerNet

### SpinnerNet Validation Gates (Must be Executable)
```bash
# .NET Build and Validation
cd src && dotnet build SpinnerNet.sln --configuration Release
cd src && dotnet format --verify-no-changes

# Unit Tests
cd src && dotnet test SpinnerNet.Tests --configuration Release --logger "console;verbosity=detailed"

# SpinnerNet App Deployment Check
cd src && dotnet publish SpinnerNet.App/*.csproj -c Release -o publish
```

*** CRITICAL AFTER YOU ARE DONE RESEARCHING AND EXPLORING THE SPINNERNET CODEBASE BEFORE YOU START WRITING THE PRP ***

*** ULTRATHINK ABOUT THE SPINNERNET PRP AND PLAN YOUR APPROACH THEN START WRITING THE PRP ***

## Output
Save as: `PRPs/{feature-name}-spinnernet.md`

## SpinnerNet Quality Checklist
- [ ] All necessary SpinnerNet foundation documents referenced (docs/*)
- [ ] Comprehensive Context7 documentation for ALL required libraries:
  - [ ] WebLLM (@mlc-ai/web-llm)
  - [ ] MudBlazor (MudBlazor)
  - [ ] Blazor Server (Microsoft.AspNetCore.Components)
  - [ ] Semantic Kernel (Microsoft.SemanticKernel)
  - [ ] MediatR (MediatR)
  - [ ] Cosmos DB (Microsoft.Azure.Cosmos)
  - [ ] FluentValidation (FluentValidation)
  - [ ] Azure Identity (Azure.Identity)
  - [ ] Azure KeyVault (Azure.Security.KeyVault.Secrets)
  - [ ] Microsoft Identity Web (Microsoft.Identity.Web)
  - [ ] SignalR (Microsoft.AspNetCore.SignalR)
  - [ ] Entity Framework (Microsoft.EntityFrameworkCore)
  - [ ] Localization (Microsoft.Extensions.Localization)
  - [ ] Dependency Injection (Microsoft.Extensions.DependencyInjection)
  - [ ] bUnit (bUnit)
  - [ ] OpenTelemetry (OpenTelemetry)
- [ ] Follows vertical slice architecture patterns from docs/VERTICAL_SLICE_ARCHITECTURE.md
- [ ] Uses Microsoft NoSQL Cosmos DB patterns from docs/DATA_MODEL_DESIGN.md (PascalCase, no JsonPropertyName)
- [ ] Implements proper localization with ILocalizationService
- [ ] MudBlazor components follow SpinnerNet design system
- [ ] WebLLM integration patterns (if AI features) from docs/AI_INTEGRATION_GUIDE.md
- [ ] Age-adaptive UI considerations from docs/Age-Adaptive-UI-Implementation.md
- [ ] Safari MCP Azure integration testing strategy included
- [ ] Validation gates are executable by AI
- [ ] References existing SpinnerNet patterns from codebase
- [ ] Clear implementation path for SpinnerNet architecture
- [ ] Error handling and logging documented
- [ ] Azure KeyVault integration (for secrets)
- [ ] Two-app architecture considerations (Web vs App)
- [ ] Azure deployment testing with Safari MCP tools
- [ ] WebLLM browser compatibility testing strategy
- [ ] Performance testing on Azure infrastructure
- [ ] Localization testing across cultures

Score the SpinnerNet PRP on a scale of 1-10 (confidence level to succeed in one-pass implementation using Claude Code)

Remember: The goal is one-pass implementation success through comprehensive SpinnerNet-specific context and patterns.