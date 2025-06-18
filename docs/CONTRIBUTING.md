# Contributing to Spinner.Net

First of all, thank you for considering contributing to Spinner.Net! This project exists because of community collaboration, and your help makes a tremendous difference.

This document provides guidelines and instructions for contributing. Following these guidelines helps communicate that you respect the time of the developers managing and developing this open source project.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Important Resources](#important-resources)
- [Types of Contributions](#types-of-contributions)
- [Development Process](#development-process)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Community Principles](#community-principles)

## Code of Conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

## Important Resources

- **Documentation**: [Architecture Overview](docs/ARCHITECTURE.md), [Development Setup](docs/DEVELOPMENT.md)
- **Communication**: [Community Forum](https://community.spinner.net)
- **Issue Tracker**: [GitHub Issues](https://github.com/spinner-net/spinner-net/issues)

## Types of Contributions

We welcome many types of contributions:

### Code

- Bug fixes
- New features
- Performance improvements
- Security enhancements
- Tests and test infrastructure

### Documentation

- Technical documentation
- User guides and tutorials
- Code examples
- Architecture diagrams
- Translation to other languages

### Design

- User interface designs
- User experience improvements
- Accessibility enhancements
- Visual design assets

### Community

- Answering questions in issues or forums
- Organizing community events
- Mentoring new contributors
- Improving contributing guides

### Idea Generation

- Feature proposals
- Use case documentation
- User stories and scenarios
- Architectural discussions

## Development Process

### 1. Find an Issue

- Check the [issue tracker](https://github.com/spinner-net/spinner-net/issues) for open issues
- Look for issues labeled `good first issue` if you're new
- Feel free to ask questions on issues you're interested in

### 2. Development Environment

Set up your development environment following our [Development Guide](docs/DEVELOPMENT.md).

### 3. Development Workflow

1. **Fork the repository** and create a branch from `main`
2. **Make your changes**, keeping in mind our coding standards
3. **Add or update tests** to cover your changes
4. **Ensure all tests pass** by running the test suite
5. **Update documentation** to reflect any changes
6. **Submit a pull request**

## Pull Request Process

1. **Create pull requests** against the `main` branch
2. **Fill out the pull request template** completely
3. **Reference any related issues** in the PR description
4. **Update documentation** if your change affects it
5. **Ensure CI passes** on your PR
6. **Respond to feedback** from maintainers
7. **Allow maintainers to merge** your PR when approved

## Coding Standards

### General Principles

- **Clarity over cleverness**: Write code that others can easily understand
- **Modularity**: Keep components focused on single responsibilities
- **Documentation**: Document interfaces and complex logic
- **Tests**: Write tests for all new features and fixes

### C# Guidelines

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use the [.NET Core Code Style](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/) with Roslyn analyzers
- StyleCop rules are enforced on build
- Keep in mind:
  - Use PascalCase for classes, interfaces, public members
  - Use camelCase with underscore for private fields: `_privateField`
  - Interface names prefixed with 'I': `IMyInterface`
  - Async methods suffixed with 'Async': `DoSomethingAsync`
  - Use XML documentation on public APIs

### Blazor/Web Guidelines

- Follow the [MudBlazor 8.3 Guidelines](docs/MUDBLAZOR_GUIDELINES.md)
- Use TypeScript for complex JavaScript functionality
- Ensure accessibility (WCAG 2.1 AA compliance)
- Ensure responsive design for multi-device support

## Community Principles

### Open Communication

- Discuss major changes before implementation
- Document decisions and their rationales
- Keep discussions public where possible
- Be respectful of differing viewpoints

### Quality First

- All code changes must pass tests
- Documentation should be clear and complete
- User experience should be accessible and intuitive
- Security must be considered in all changes

### Personal Data Respect

- Never commit real personal data
- Use mock data for testing
- Consider privacy implications of all features
- Adhere to data minimization principles

### Inclusive Community

- Welcome contributors of all backgrounds
- Create safe spaces for learning
- Value different perspectives and expertise
- Provide mentorship for newcomers

## Recognition

All contributors are recognized in our [CONTRIBUTORS.md](CONTRIBUTORS.md) file. Your contributions, whether code, documentation, design, or community support, make a real difference to this project.

Thank you for contributing to Spinner.Net!