# Technical FAQ

This document answers frequently asked technical questions about the Spinner.Net project.

## General Questions

### What technologies does Spinner.Net use?

Spinner.Net is built with:
- **.NET 9**: For backend and cross-platform capabilities
- **Blazor**: For web UI components and client-side logic
- **MudBlazor**: For UI component library
- **Entity Framework Core**: For data access
- **MAUI**: For cross-platform desktop and mobile applications
- **Vector databases**: For semantic knowledge storage and retrieval
- **ASP.NET Core**: For API endpoints and server infrastructure

### Is Spinner.Net a single application or multiple applications?

Spinner.Net is a modular platform that supports multiple deployment scenarios:
- Web application
- Desktop application (Windows, macOS, Linux)
- Mobile application (Android, iOS)
- Server-side API for custom integrations

The core functionality is shared across all deployments, with platform-specific adaptations.

### How does Spinner.Net relate to LichtFlow?

Spinner.Net builds upon and expands the architecture of LichtFlow, which was originally designed as a platform for photographers. While LichtFlow focused on image assets and photographer workflows, Spinner.Net broadens the scope to support a wide range of digital assets, services, and community functions. LichtFlow will eventually be integrated as one specialized project under the Spinner.Net umbrella.

## Architecture Questions

### What is the "asset-centric architecture"?

The asset-centric architecture treats all content as assets with standardized interfaces and behaviors. This means:

1. Every piece of content (messages, events, tasks, etc.) is an asset
2. Assets are owned by core services and accessed by modules
3. Assets share common base functionality while allowing type-specific behaviors
4. Cross-cutting concerns like permissions and tagging are standardized

This approach enables consistent handling of diverse content types while maintaining flexibility.

### How does the modular system work?

Spinner.Net uses a plugin-based modular architecture:

1. Core systems provide fundamental capabilities (assets, identity, storage)
2. Modules add specific functionality (email, calendar, projects)
3. Modules register with the core through a standardized interface
4. The core dynamically loads and manages modules
5. Modules declare which asset types they support

This allows for extensibility and customization while maintaining a consistent architecture.

### How does Spinner.Net handle data storage?

Spinner.Net uses a hybrid storage approach:

1. **Relational Database**: For structured data, relationships, and system information
2. **Vector Database**: For semantic knowledge storage and similarity search
3. **Document Database**: For flexible schema storage of complex assets
4. **Local Storage**: For user-specific data that remains on-device
5. **External Services**: Data remains in original services, accessed via connectors

The specific storage mechanism for each piece of data depends on its nature, sensitivity, and access patterns.

### What is the "Persona System"?

The Persona System provides personalized digital companions that:

1. Learn and adapt to user preferences
2. Provide consistent experiences across devices and services
3. Assist with managing permissions and privacy
4. Facilitate connections with the community
5. Adapt interfaces based on user context

It's implemented through a combination of preference tracking, machine learning models, and contextual adaptation.

## Development Questions

### How do I create a new asset type?

To create a new asset type:

1. Create a class that inherits from `BaseAsset`
2. Implement any type-specific properties and methods
3. Register the asset type with the system
4. Create repository and service implementations for the asset type
5. Implement validation and business logic

See the [Getting Started](GETTING_STARTED.md) guide for code examples.

### How do I integrate with an external service?

To integrate with an external service:

1. Implement the `IServiceConnector` interface
2. Handle authentication with the service
3. Map between service data and Spinner.Net assets
4. Implement synchronization logic
5. Register the connector with the system

The service connector framework handles common tasks like credential management and permission enforcement.

### How does the vector database integration work?

Vector database integration works as follows:

1. Assets are processed to extract semantic meaning
2. This semantic meaning is converted to vector embeddings
3. Embeddings are stored in a vector database along with metadata
4. Queries are converted to vectors and used for similarity search
5. Results are ranked by relevance and returned to the application

This enables powerful semantic search and relationship discovery across content.

### What's the purpose of the "Knowledge Engine"?

The Knowledge Engine provides:

1. Schema-less storage of information using vector embeddings
2. Natural language querying of stored knowledge
3. Contextual information retrieval based on user needs
4. Relationship discovery between pieces of information
5. Information organization without rigid structures

It's particularly useful for personal knowledge management, learning resources, and community knowledge sharing.

## Testing Questions

### What testing frameworks are used?

Spinner.Net uses:
- **xUnit**: For unit and integration tests
- **Moq**: For mocking dependencies
- **FluentAssertions**: For readable assertions
- **Respawn**: For database reset between tests
- **WebApplicationFactory**: For API testing
- **Playwright**: For UI testing

### How should I test my contributions?

Follow these testing guidelines:

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test interactions between components
3. **UI Tests**: Test UI components and interactions
4. **End-to-End Tests**: Test complete workflows

Use in-memory databases where appropriate, and ensure tests are independent and repeatable.

### How do I test with the vector database?

For testing with vector databases:

1. Use the in-memory vector database implementation for unit tests
2. For integration tests, use a test-specific vector database instance
3. Mock the vector database service for component tests
4. Use the `IVectorStorage` abstraction rather than specific implementations

## Deployment Questions

### How is Spinner.Net deployed?

Spinner.Net supports multiple deployment scenarios:

1. **Web Application**: Deployed to any ASP.NET Core-compatible hosting
2. **Desktop Application**: Packaged as a MAUI application for Windows, macOS, and Linux
3. **Mobile Application**: Published to app stores as a MAUI application
4. **Self-Hosted**: Deployed to on-premises servers or personal devices

### What are the hosting requirements?

For basic hosting:
- **.NET 9 Runtime**: Required for all deployments
- **Database**: Either SQLite (simple) or SQL Server/PostgreSQL (production)
- **Vector Database**: Qdrant, Chroma, or other compatible service
- **Storage**: Local or cloud storage for assets
- **Memory**: 2GB minimum, 4GB+ recommended
- **CPU**: 2+ cores recommended

### Can Spinner.Net be used offline?

Yes, Spinner.Net is designed with offline capabilities:

1. The web application includes PWA features for offline use
2. The desktop and mobile applications work offline by default
3. Data is synchronized when connectivity is restored
4. Critical functions remain available without network connection
5. Local-first data storage ensures access to important information

## Performance Questions

### How does Spinner.Net handle large amounts of data?

Spinner.Net uses several strategies for performance with large datasets:

1. **Lazy Loading**: Only loading data when needed
2. **Pagination**: Breaking large datasets into manageable chunks
3. **Caching**: Storing frequently accessed data in memory
4. **Indexing**: Optimizing database queries
5. **Asset Segmentation**: Splitting large collections into smaller segments
6. **Background Processing**: Handling heavy operations asynchronously

### How is the vector database optimized for performance?

Vector database optimizations include:

1. **Dimensional Reduction**: Reducing vector size while preserving semantic meaning
2. **Indexing**: Using HNSW or other indexing for fast similarity search
3. **Batched Operations**: Processing vectors in batches
4. **Caching**: Storing frequent queries and results
5. **Selective Embedding**: Only vectorizing relevant content

## Contribution Questions

### What types of contributions are needed?

Spinner.Net welcomes various contributions:

1. **Core Components**: Persona system, vector storage, asset management
2. **Service Connectors**: Integration with external services
3. **UI Components**: Adaptive and accessible user interfaces
4. **Mobile Adaptations**: MAUI-specific implementations
5. **Testing**: Expanding test coverage and test infrastructure
6. **Documentation**: Technical documentation and examples
7. **Performance Optimization**: Improving efficiency and responsiveness

### How can I start contributing?

Start by:

1. Setting up your development environment (see [Getting Started](GETTING_STARTED.md))
2. Understanding the architecture (see [Architecture](ARCHITECTURE.md))
3. Finding issues labeled "good first issue"
4. Joining community discussions to understand current priorities
5. Reading through existing code to understand patterns and practices

We welcome contributors of all experience levels!

## Security Questions

### How does Spinner.Net handle data security?

Spinner.Net implements several security measures:

1. **Encryption**: Sensitive data is encrypted at rest and in transit
2. **Authentication**: Strong user authentication with MFA support
3. **Authorization**: Fine-grained permission system for all assets
4. **Audit Logging**: Comprehensive logging of access and changes
5. **Input Validation**: Strict validation of all inputs
6. **Local-First**: Sensitive data can remain on user devices

### How is user privacy protected?

Privacy protection includes:

1. **User Control**: Users decide what data is shared and with whom
2. **Transparent Permissions**: Clear visibility into data access
3. **Minimal Collection**: Only necessary data is collected
4. **Data Expiration**: Automatic removal of unnecessary data
5. **Export/Delete**: Easy data export and account deletion

### How are service integrations secured?

Service integration security includes:

1. **OAuth**: Standard OAuth flows for service authentication
2. **Token Storage**: Secure storage of access tokens
3. **Minimal Scope**: Requesting only necessary permissions
4. **Revocation**: Simple revocation of service access
5. **Transparency**: Clear indication of active integrations