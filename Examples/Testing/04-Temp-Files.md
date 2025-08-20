# Testing - Temporary File Management Pattern

## Problem Statement
Test artifacts (screenshots, deployment files, logs) clutter the git repository when not properly managed, leading to commit pollution and repository bloat.

## Working Solution

### Directory Structure
```bash
# Project structure with temp handling
Spinner.Net/Public/
├── src/                    # Source code (CLEAN)
├── .claude/
│   ├── scripts/
│   │   └── cleanup-temp-files.sh  # Automated cleanup
│   └── TEMP-FILE-GUIDELINES.md    # Guidelines
├── /tmp/spinnernet-temp/   # All temp files go here
└── .gitignore              # Ignore temp patterns
```

### Automated Cleanup Script
```bash
#!/bin/bash
# .claude/scripts/cleanup-temp-files.sh

echo "🧹 Cleaning up temporary files from Spinner.Net repository..."

# Project root cleanup
cd /Users/peterclinton/Desktop/repos/Spinner.Net/Public

# Remove common temp file patterns
rm -f *.png *.jpg *.jpeg *.gif
rm -f *screenshot* *-screenshot* screenshot-*
rm -f *-test* *test* test-*
rm -f *-form-* *form* form-*
rm -f after-* before-* during-*
rm -f manual-* auto-* fixed-*
rm -f persona-* step*-* 
rm -f deployment*.zip *.zip

echo "  ✅ Cleaned project root"

# Src directory cleanup  
cd src
rm -f *.png *.jpg *.jpeg *.gif
rm -f *screenshot* *-screenshot* screenshot-*
rm -f deployment*.zip *.zip

echo "  ✅ Cleaned src directory"

# Ensure temp directory exists and is clean
mkdir -p /tmp/spinnernet-temp
rm -rf /tmp/spinnernet-temp/*

echo "  ✅ Reset temp directory: /tmp/spinnernet-temp/"
echo "🎉 Cleanup complete! Repository is clean of temporary files."
```

### Safari MCP Testing with Temp Files
```javascript
// ✅ CORRECT: Use temp directory for screenshots
await mcp__safari__take_screenshot({ 
    filename: "/tmp/spinnernet-temp/form-test-step1" 
});

await mcp__safari__take_screenshot({ 
    filename: "/tmp/spinnernet-temp/after-navigation" 
});

// ❌ WRONG: Saving in project directory
await mcp__safari__take_screenshot({ 
    filename: "test-screenshot"  // Goes to project root!
});
```

### Azure Deployment with Temp Files
```bash
# ✅ CORRECT: Build and deploy using temp folder
cd src
dotnet publish SpinnerNet.App/*.csproj -c Release -o /tmp/spinnernet-temp/publish

cd /tmp/spinnernet-temp
zip -r deployment.zip publish/

az webapp deploy -g rg-spinnernet-proto \
                -n spinnernet-app-3lauxg \
                --src-path /tmp/spinnernet-temp/deployment.zip \
                --type zip

# Clean up immediately
rm -rf /tmp/spinnernet-temp/*

# ❌ WRONG: Building in source directory
cd src
dotnet publish SpinnerNet.App/*.csproj -c Release -o publish
zip -r deployment.zip publish/  # Creates deployment.zip in src/
```

## Best Practices

### 1. Pre-Test Setup
```bash
# Always start with clean temp directory
mkdir -p /tmp/spinnernet-temp
rm -rf /tmp/spinnernet-temp/*
```

### 2. Consistent Naming Convention
```bash
# Use descriptive names with context
/tmp/spinnernet-temp/persona-creation-step1-filled.png
/tmp/spinnernet-temp/webllm-initialization-success.png
/tmp/spinnernet-temp/form-validation-error-state.png

# NOT generic names
/tmp/spinnernet-temp/test1.png
/tmp/spinnernet-temp/screenshot.png
```

### 3. Post-Test Cleanup
```bash
# After testing session
.claude/scripts/cleanup-temp-files.sh

# Verify repository is clean
git status
# Should show no new untracked files
```

### 4. Gitignore Patterns
```gitignore
# .gitignore - Prevent common temp file commits
*.png
*.jpg
*.jpeg
*.gif
*screenshot*
*-test*
*test-*
*-form-*
deployment*.zip
after-*
before-*
step*-*
manual-*
```

## Common Mistake Patterns

### Files That Should NEVER Appear in Git
```bash
# Screenshot patterns
after-form-fill
form-filled-test
step1-completed
manual-step3-navigation
persona-creation-test
webllm-status-check

# Deployment patterns
deployment.zip
deployment-clean.zip
publish/

# Test patterns
test-screenshot.png
browser-test-results.png
validation-test.png
```

### Warning Signs in Git Status
```bash
# ❌ BAD: These indicate temp file leak
git status
Untracked files:
  after-form-navigation.png
  deployment.zip
  test-results.json
  step2-validation.png

# ✅ GOOD: Clean repository
git status
On branch main
nothing to commit, working tree clean
```

## Automated Validation

### Pre-Commit Hook
```bash
#!/bin/sh
# .git/hooks/pre-commit

# Check for temp files
if git diff --cached --name-only | grep -E '\.(png|jpg|jpeg|gif|zip)$|screenshot|test|form|deployment'; then
    echo "❌ ERROR: Temporary files detected in commit!"
    echo "Run: .claude/scripts/cleanup-temp-files.sh"
    exit 1
fi
```

### CI/CD Check
```yaml
# .github/workflows/temp-file-check.yml
name: Temp File Check
on: [push, pull_request]

jobs:
  check-temp-files:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Check for temp files
        run: |
          if find . -name "*.png" -o -name "*screenshot*" -o -name "deployment*.zip" | grep -q .; then
            echo "❌ Temporary files found in repository"
            find . -name "*.png" -o -name "*screenshot*" -o -name "deployment*.zip"
            exit 1
          fi
          echo "✅ Repository is clean"
```

## Safari MCP Workflow Pattern

### Complete Testing Session
```javascript
// 1. Setup
function setupTesting() {
    console.log("🧪 Starting test session");
    return Date.now(); // Session ID for file naming
}

// 2. Take screenshot with proper naming
async function captureTestState(sessionId, description) {
    const filename = `/tmp/spinnernet-temp/test-${sessionId}-${description}`;
    await mcp__safari__take_screenshot({ filename });
    console.log(`📸 Captured: ${description}`);
}

// 3. Complete test with cleanup verification
async function completeTestSession(sessionId) {
    console.log("🏁 Test session complete");
    
    // Verify temp files created
    const tempFiles = await executeShellCommand("ls /tmp/spinnernet-temp/");
    console.log("📁 Temp files created:", tempFiles);
    
    // Verify project is clean
    const gitStatus = await executeShellCommand("cd /Users/peterclinton/Desktop/repos/Spinner.Net/Public && git status --porcelain");
    if (gitStatus.includes('.png') || gitStatus.includes('.zip')) {
        console.error("❌ WARNING: Repository contains temp files!");
    } else {
        console.log("✅ Repository is clean");
    }
}
```

## Integration with Examples

### When Creating New Examples
```markdown
# Pattern: Include temp file instructions
## Testing Approach

```bash
# Setup
mkdir -p /tmp/spinnernet-temp

# Test with screenshots
mcp__safari__take_screenshot({ filename: "/tmp/spinnernet-temp/pattern-test" })

# Cleanup
.claude/scripts/cleanup-temp-files.sh
```

### Example Documentation
```markdown
# Remember to cleanup temp files
After running this example, always run:
`.claude/scripts/cleanup-temp-files.sh`
```

## Troubleshooting

### Repository Already Polluted
```bash
# Remove all temp files from repository
find . -name "*.png" -delete
find . -name "*screenshot*" -delete  
find . -name "deployment*.zip" -delete
find . -name "*test*" -type f -delete

# Commit cleanup
git add -A
git commit -m "🧹 Clean up temporary files"
```

### Temp Directory Permission Issues
```bash
# Fix permissions
sudo chown -R $(whoami) /tmp/spinnernet-temp/
chmod -R 755 /tmp/spinnernet-temp/
```

## Related Patterns
- [01-SafariMCP-Guide.md] - Using Safari MCP properly
- [03-Form-Automation.md] - Form testing patterns
- [../Azure/01-Deployment-Blazor.md] - Clean deployment patterns

## Key Principles
1. **Never commit temp files** - Use .gitignore and automation
2. **Use descriptive names** - Include context and timestamp
3. **Clean up immediately** - Don't let temp files accumulate
4. **Automate verification** - Scripts and hooks prevent mistakes
5. **Document workflow** - Make temp file handling explicit in examples