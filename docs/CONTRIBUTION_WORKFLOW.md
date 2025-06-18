# Contribution Workflow

This document outlines the step-by-step workflow for contributing to the Spinner.Net project.

## Overview

Contributing to Spinner.Net follows this general process:

1. Find or create an issue
2. Fork the repository
3. Set up your development environment
4. Create a branch
5. Make your changes
6. Run tests
7. Submit a pull request
8. Respond to feedback

## Detailed Workflow

### 1. Find or Create an Issue

Before starting work:

- Check the [issue tracker](https://github.com/spinner-net/spinner-net/issues) for existing issues
- Look for issues labeled `good first issue` if you're new
- If you don't find a relevant issue, create a new one:
  - Clearly describe the problem or enhancement
  - Include any relevant context
  - Wait for maintainer feedback before starting work

### 2. Fork the Repository

- Navigate to [Spinner.Net repository](https://github.com/spinner-net/spinner-net)
- Click the "Fork" button in the top-right corner
- This creates your own copy of the repository

### 3. Set Up Your Development Environment

- Clone your fork to your local machine:
  ```bash
  git clone https://github.com/YOUR-USERNAME/spinner-net.git
  cd spinner-net
  ```

- Add the upstream repository as a remote:
  ```bash
  git remote add upstream https://github.com/spinner-net/spinner-net.git
  ```

- Install dependencies:
  ```bash
  dotnet restore
  cd src/Spinner.Net.Web
  npm install
  cd ../..
  ```

- Set up the database:
  ```bash
  dotnet ef database update --project src/Spinner.Net.Core
  ```

### 4. Create a Branch

- Create a branch for your work:
  ```bash
  git checkout -b feature/your-feature-name
  ```

- Use a descriptive name related to the issue:
  - `feature/add-email-connector`
  - `fix/vector-search-performance`
  - `docs/improve-contribution-guide`

### 5. Make Your Changes

- Follow the [coding standards](../CONTRIBUTING.md#coding-standards)
- Keep changes focused on the specific issue
- Break large changes into smaller, logical commits
- Write clear commit messages:
  ```
  feat: add Gmail connector implementation
  
  Adds new connector for Gmail with:
  - OAuth authentication
  - Message synchronization
  - Basic filtering
  
  Resolves #123
  ```

- Update relevant documentation
- Add comments for complex logic

### 6. Run Tests

- Create tests for your changes:
  ```bash
  # For a new feature
  dotnet new xunit -n YourFeature.Tests -o test/Spinner.Net.YourFeature.Tests
  ```

- Run tests to ensure everything works:
  ```bash
  # Run all tests
  dotnet test
  
  # Run specific tests
  dotnet test test/Spinner.Net.Core.Tests
  ```

- Fix any test failures

### 7. Submit a Pull Request

- Push your branch to your fork:
  ```bash
  git push -u origin feature/your-feature-name
  ```

- Navigate to your fork on GitHub
- Click "Compare & pull request"
- Fill out the pull request template:
  - Clear title summarizing the change
  - Detailed description of changes
  - Reference to the relevant issue(s)
  - Any additional context or screenshots

### 8. Respond to Feedback

- Watch for CI/CD results
- Address any feedback from reviewers
- Make additional commits to fix issues
- Keep the PR updated with the latest changes from main:
  ```bash
  git fetch upstream
  git rebase upstream/main
  git push -f origin feature/your-feature-name
  ```

- Once approved, a maintainer will merge your PR

## Tips for Successful Contributions

### Keeping Your Fork Updated

Regularly sync your fork with the upstream repository:

```bash
git fetch upstream
git checkout main
git merge upstream/main
git push origin main
```

### Working with Multiple Features

Create separate branches for each feature or fix:

```bash
# Create a new branch from the updated main
git checkout main
git pull upstream main
git checkout -b feature/another-feature
```

### Addressing Review Feedback

When addressing review feedback:

1. Make the requested changes
2. Add new commits for the changes
3. Push to the same branch
4. Respond to the review comments

### Handling Conflicts

If you encounter merge conflicts:

```bash
git fetch upstream
git rebase upstream/main
# Resolve conflicts in your editor
git add .
git rebase --continue
git push -f origin your-branch-name
```

## Common Scenarios

### Small Bug Fixes

For small, straightforward fixes:

1. Create a branch
2. Make the fix
3. Add a test case
4. Submit a PR with a clear explanation

### New Features

For larger features:

1. Discuss the approach in the issue first
2. Break the work into smaller, logical pieces
3. Consider creating a design document
4. Implement incrementally with tests
5. Submit a PR with comprehensive documentation

### Documentation Improvements

For documentation updates:

1. Create a branch
2. Make the documentation changes
3. Ensure proper formatting and links
4. Submit a PR describing the improvements

## Contribution Review Process

Pull requests are reviewed based on:

1. **Functionality**: Does it work as expected?
2. **Code Quality**: Does it follow project standards?
3. **Tests**: Does it include adequate tests?
4. **Documentation**: Is it properly documented?
5. **Performance**: Does it perform efficiently?
6. **Security**: Does it follow security best practices?
7. **Compatibility**: Does it maintain backward compatibility?

## After Your Contribution is Merged

- Delete your branch:
  ```bash
  git checkout main
  git branch -d feature/your-feature-name
  git push origin --delete feature/your-feature-name
  ```

- Update your fork:
  ```bash
  git pull upstream main
  git push origin main
  ```

- Look for a new issue to tackle!

Thank you for contributing to Spinner.Net!