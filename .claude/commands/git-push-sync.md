# Git Push and Sync Command

## Description
Push current changes to remote repository and sync with latest changes. Creates proper commit with package structure and comments following SpinnerNet conventions.

## Command
```bash
# Stage all changes
git add .

# Create commit with proper package structure and comments
git commit -m "$(cat <<'EOF'
feat: Add Phase 1 Foundation PRP with WebLLM integration and age-adaptive UI

## Package Structure
- **PRPs/Phase1-Foundation-spinnernet.md**: Complete PRP with 3-phase implementation
- **WebLLM Integration**: Client-side AI processing with Hermes-2-Pro-Mistral-7B
- **Age-Adaptive UI**: MudBlazor theming system for all age groups
- **Vertical Slice Architecture**: MediatR handlers and FluentValidation
- **Cosmos DB Patterns**: Microsoft NoSQL patterns with PascalCase properties

## Implementation Score: 9/10
- Complete skeleton, production, and test code
- Follows SpinnerNet architectural patterns exactly
- Comprehensive validation gates and quality checklist
- Production-ready with proper error handling

## Technical Details
- WebLLM JavaScript integration with proper interop
- Age-adaptive themes (Child, Teen, Adult, Senior)
- MediatR command/query handlers for persona creation
- FluentValidation with comprehensive test coverage
- Service registration following SpinnerNet patterns

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"

# Push to remote
git push origin main

# Sync with remote changes
git pull origin main --rebase
```

## Usage
Run this command when you want to:
- Push current Phase 1 Foundation PRP changes
- Sync with remote repository
- Create proper commit with package structure comments
- Follow SpinnerNet git conventions

## Notes
- Automatically stages all changes
- Creates descriptive commit following SpinnerNet patterns
- Includes implementation score and technical details
- Syncs with remote after pushing