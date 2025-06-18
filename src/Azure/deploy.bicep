// Azure Bicep template for Spinner.Net Zeitsparkasse deployment
// Creates production-ready environment for time banking platform

targetScope = 'resourceGroup'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Unique suffix for resource names')
param uniqueSuffix string = uniqueString(resourceGroup().id)

@description('OpenAI API Key')
@secure()
param openAiApiKey string

// Variables
var shortSuffix = substring(uniqueSuffix, 0, 6)
var appName = 'spinnernet-${shortSuffix}'
var cosmosAccountName = 'cosmos-${appName}'
var appServicePlanName = 'asp-${appName}'
var keyVaultName = 'kv-${appName}'
var signalRName = 'signalr-${appName}'
var appInsightsName = 'ai-${appName}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1' // Basic tier for prototype
    tier: 'Basic'
    size: 'B1'
    capacity: 1
  }
  properties: {
    reserved: false
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
    Purpose: 'Zeitsparkasse-Foundation'
  }
}

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless' // Serverless for prototype cost optimization
      }
    ]
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
    Purpose: 'Time-Banking-Data'
  }
}

// Cosmos DB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: 'SpinnerNetDb'
  properties: {
    resource: {
      id: 'SpinnerNetDb'
    }
  }
}

// Cosmos DB Containers
resource usersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'Users'
  properties: {
    resource: {
      id: 'Users'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
      }
    }
  }
}

resource tasksContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'Tasks'
  properties: {
    resource: {
      id: 'Tasks'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
      }
    }
  }
}

resource buddiesContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'Buddies'
  properties: {
    resource: {
      id: 'Buddies'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
      }
    }
  }
}

resource conversationsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDatabase
  name: 'Conversations'
  properties: {
    resource: {
      id: 'Conversations'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
      }
    }
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
  }
}

// Store secrets in Key Vault
resource openAiSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'OpenAI-ApiKey'
  properties: {
    value: openAiApiKey
  }
}

resource cosmosConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'CosmosDb-ConnectionString'
  properties: {
    value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
  }
}

// SignalR Service
resource signalRService 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: signalRName
  location: location
  sku: {
    name: 'Free_F1' // Free tier for prototype
    tier: 'Free'
    capacity: 1
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
    cors: {
      allowedOrigins: ['*'] // Restrict in production
    }
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'CosmosDb__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=CosmosDb-ConnectionString)'
        }
        {
          name: 'CosmosDb__DatabaseName'
          value: 'SpinnerNetDb'
        }
        {
          name: 'OpenAI__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=OpenAI-ApiKey)'
        }
        {
          name: 'OpenAI__ModelName'
          value: 'gpt-4o-mini'
        }
        {
          name: 'SignalR__ConnectionString'
          value: signalRService.listKeys().primaryConnectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
  tags: {
    Environment: environment
    Project: 'SpinnerNet'
    Purpose: 'Zeitsparkasse-Web-App'
  }
}

// Outputs
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output cosmosAccountName string = cosmosAccount.name
output keyVaultName string = keyVault.name
output resourceGroupName string = resourceGroup().name