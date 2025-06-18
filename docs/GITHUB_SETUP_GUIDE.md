# GitHub Setup Guide for Spinner.Net

This guide will walk you through setting up your GitHub account, creating a repository for Spinner.Net, and connecting your local code to GitHub.

## 1. Creating a GitHub Account

1. Go to [github.com](https://github.com)
2. Click "Sign up" in the upper right corner
3. Follow the prompts to create your account:
   - Enter your email
   - Create a password
   - Choose a username
   - Complete the verification process
4. Verify your email address by clicking the link sent to your email

## 2. Creating a New Repository

1. Log in to GitHub
2. Click the "+" icon in the upper right corner
3. Select "New repository"
4. Fill in the repository details:
   - **Repository name**: `spinner-net`
   - **Description**: "Digital backbone for community, creativity, and connection"
   - **Visibility**: Choose "Public" if you want others to contribute or "Private" if you want to start privately
   - **Initialize this repository with**: Check "Add a README file"
   - **Choose a license**: Select "GNU Affero General Public License v3.0" for the core
5. Click "Create repository"

## 3. Setting Up Your Local Project

### Option A: Start With Existing Code (LichtFlow)

If you want to start with your existing LichtFlow code:

1. Open Terminal or Command Prompt
2. Navigate to your LichtFlow directory:
   ```bash
   cd /Users/peterclinton/Desktop/repos/LichtFlow
   ```
3. Create a new directory for Spinner.Net if needed:
   ```bash
   mkdir -p ~/Desktop/repos/Spinner.Net
   ```
4. Copy the necessary files:
   ```bash
   cp -r Spinner.Net-Public/* ~/Desktop/repos/Spinner.Net/
   ```
5. Navigate to your new project directory:
   ```bash
   cd ~/Desktop/repos/Spinner.Net
   ```

### Option B: Start Fresh

If you want to start with a fresh project:

1. Create a new directory for your project:
   ```bash
   mkdir -p ~/Desktop/repos/Spinner.Net
   cd ~/Desktop/repos/Spinner.Net
   ```
2. Initialize a new .NET solution:
   ```bash
   dotnet new sln -n Spinner.Net
   ```
3. Create initial projects:
   ```bash
   dotnet new webapi -n Spinner.Net.Core -o src/Spinner.Net.Core
   dotnet new classlib -n Spinner.Net.Assets -o src/Spinner.Net.Assets
   dotnet new classlib -n Spinner.Net.Identity -o src/Spinner.Net.Identity
   dotnet new xunit -n Spinner.Net.Core.Tests -o test/Spinner.Net.Core.Tests
   ```
4. Add projects to solution:
   ```bash
   dotnet sln add src/Spinner.Net.Core/Spinner.Net.Core.csproj
   dotnet sln add src/Spinner.Net.Assets/Spinner.Net.Assets.csproj
   dotnet sln add src/Spinner.Net.Identity/Spinner.Net.Identity.csproj
   dotnet sln add test/Spinner.Net.Core.Tests/Spinner.Net.Core.Tests.csproj
   ```

## 4. Connecting Your Local Project to GitHub

1. Initialize Git in your project folder (if not already done):
   ```bash
   git init
   ```

2. Add the GitHub repository as the remote origin:
   ```bash
   git remote add origin https://github.com/YOUR-USERNAME/spinner-net.git
   ```

3. Create a .gitignore file (if not already copied from Spinner.Net-Public):
   ```bash
   # Download a standard .NET gitignore if needed
   curl -o .gitignore https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore
   ```

4. Make your initial commit:
   ```bash
   git add .
   git commit -m "Initial commit"
   ```

5. Push to GitHub:
   ```bash
   # If starting with a fresh repo
   git push -u origin main
   
   # If GitHub repo was initialized with files, first pull them
   git pull origin main --allow-unrelated-histories
   git push -u origin main
   ```

## 5. Branch Strategy

For Spinner.Net development, we recommend using the following branch strategy:

1. Keep `main` branch as your stable, production-ready code
2. Create a `develop` branch for ongoing development:
   ```bash
   git checkout -b develop
   git push -u origin develop
   ```

3. For new features, create feature branches from `develop`:
   ```bash
   git checkout develop
   git checkout -b feature/persona-system
   ```

4. For bug fixes, create fix branches from `main` or `develop` depending on urgency:
   ```bash
   git checkout main
   git checkout -b fix/login-issue
   ```

## 6. Uploading Content

1. To add documentation from the Spinner.Net-Public folder:
   ```bash
   # Ensure you're in your project directory
   cd ~/Desktop/repos/Spinner.Net
   
   # Copy documentation files
   mkdir -p docs
   cp -r /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/docs/* docs/
   cp /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/README.md .
   cp /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/CONTRIBUTING.md .
   
   # Add to git
   git add docs README.md CONTRIBUTING.md
   git commit -m "Add initial documentation"
   git push
   ```

2. To add GitHub templates:
   ```bash
   mkdir -p .github/ISSUE_TEMPLATE
   cp -r /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/.github/* .github/
   
   git add .github
   git commit -m "Add GitHub templates"
   git push
   ```

## 7. Simple Workflow for Ongoing Development

1. Work on the `develop` branch for general changes:
   ```bash
   git checkout develop
   
   # Make your changes...
   
   git add .
   git commit -m "Add feature X"
   git push
   ```

2. For major features, use feature branches:
   ```bash
   git checkout develop
   git checkout -b feature/new-feature
   
   # Make your changes...
   
   git add .
   git commit -m "Implement new feature"
   git push -u origin feature/new-feature
   ```

3. Create a pull request on GitHub:
   - Go to your repository on GitHub
   - Click "Pull requests"
   - Click "New pull request"
   - Select base branch (`develop`) and compare branch (`feature/new-feature`)
   - Add a description
   - Click "Create pull request"

## 8. Collaboration Settings

Once your repository is set up, consider these GitHub settings:

1. **Protection rules** for main branch:
   - Go to Settings > Branches
   - Add rule for the `main` branch
   - Require pull request reviews before merging
   - Require status checks to pass before merging
   
2. **Collaborators**:
   - Go to Settings > Collaborators
   - Add team members with appropriate permissions

3. **GitHub Pages** for documentation:
   - Go to Settings > Pages
   - Source: Deploy from a branch
   - Branch: main, folder: /docs
   - Click Save

## Next Steps

Once your repository is set up, you should:

1. Fill out the README.md with project details
2. Set up any GitHub Actions for CI/CD (you can copy workflows from the Spinner.Net-Public folder)
3. Invite collaborators to your repository
4. Begin implementing your first features

For help with GitHub-specific features, refer to the [GitHub documentation](https://docs.github.com/).