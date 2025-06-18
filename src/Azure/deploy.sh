#!/bin/bash

# Spinner.Net Azure Deployment Script
# Deploys Zeitsparkasse foundation platform to Azure

set -e

# Configuration
RESOURCE_GROUP="rg-spinnernet-proto"
LOCATION="Germany West Central"
ENVIRONMENT="prototype"
SUBSCRIPTION_ID=""  # Set your subscription ID

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Spinner.Net Zeitsparkasse - Azure Deployment${NC}"
echo -e "${BLUE}=================================================${NC}"

# Check if logged in to Azure
echo -e "\n${YELLOW}📋 Checking Azure CLI authentication...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${RED}❌ Not logged in to Azure CLI${NC}"
    echo -e "${YELLOW}Please run: az login${NC}"
    exit 1
fi

# Get current subscription
CURRENT_SUB=$(az account show --query id --output tsv)
echo -e "${GREEN}✓ Logged in to Azure subscription: ${CURRENT_SUB}${NC}"

# Set subscription if provided
if [ ! -z "$SUBSCRIPTION_ID" ]; then
    echo -e "\n${YELLOW}🔄 Setting subscription to: ${SUBSCRIPTION_ID}${NC}"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# Create resource group
echo -e "\n${YELLOW}🏗️ Creating resource group: ${RESOURCE_GROUP}${NC}"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --tags \
        Environment="$ENVIRONMENT" \
        Project="SpinnerNet" \
        Purpose="Zeitsparkasse-Prototype"

echo -e "${GREEN}✓ Resource group created${NC}"

# Get OpenAI API Key
echo -e "\n${YELLOW}🔑 OpenAI API Key Configuration${NC}"
if [ -z "$OPENAI_API_KEY" ]; then
    echo -e "${YELLOW}Please enter your OpenAI API Key:${NC}"
    read -s OPENAI_API_KEY
    echo
fi

if [ -z "$OPENAI_API_KEY" ]; then
    echo -e "${RED}❌ OpenAI API Key is required${NC}"
    exit 1
fi

# Deploy infrastructure
echo -e "\n${YELLOW}🌩️ Deploying Azure infrastructure...${NC}"
DEPLOYMENT_NAME="spinnernet-deployment-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "deploy.bicep" \
    --name "$DEPLOYMENT_NAME" \
    --parameters \
        environment="$ENVIRONMENT" \
        openAiApiKey="$OPENAI_API_KEY"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Infrastructure deployed successfully${NC}"
else
    echo -e "${RED}❌ Infrastructure deployment failed${NC}"
    exit 1
fi

# Get deployment outputs
echo -e "\n${YELLOW}📊 Getting deployment information...${NC}"
WEB_APP_URL=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.webAppUrl.value \
    --output tsv)

WEB_APP_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.webAppName.value \
    --output tsv)

COSMOS_ACCOUNT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.cosmosAccountName.value \
    --output tsv)

KEY_VAULT_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.keyVaultName.value \
    --output tsv)

# Build and deploy application
echo -e "\n${YELLOW}🔨 Building Spinner.Net application...${NC}"
cd "../src"

# Restore packages
dotnet restore SpinnerNet.Web/SpinnerNet.Web.csproj

# Build application
dotnet build SpinnerNet.Web/SpinnerNet.Web.csproj --configuration Release

# Publish application
dotnet publish SpinnerNet.Web/SpinnerNet.Web.csproj \
    --configuration Release \
    --output "../azure-deployment/publish" \
    --self-contained false

echo -e "${GREEN}✓ Application built successfully${NC}"

# Deploy to Azure App Service
echo -e "\n${YELLOW}🚀 Deploying to Azure App Service...${NC}"
cd "../azure-deployment"

# Create deployment package
zip -r "spinnernet-deploy.zip" publish/

# Deploy using Azure CLI
az webapp deployment source config-zip \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_APP_NAME" \
    --src "spinnernet-deploy.zip"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Application deployed successfully${NC}"
else
    echo -e "${RED}❌ Application deployment failed${NC}"
    exit 1
fi

# Clean up
rm -rf publish/
rm -f spinnernet-deploy.zip

# Final output
echo -e "\n${GREEN}🎉 DEPLOYMENT COMPLETE!${NC}"
echo -e "${GREEN}================================${NC}"
echo -e "${BLUE}Web Application URL:${NC} $WEB_APP_URL"
echo -e "${BLUE}Resource Group:${NC} $RESOURCE_GROUP"
echo -e "${BLUE}App Service:${NC} $WEB_APP_NAME"
echo -e "${BLUE}Cosmos DB:${NC} $COSMOS_ACCOUNT"
echo -e "${BLUE}Key Vault:${NC} $KEY_VAULT_NAME"

echo -e "\n${YELLOW}📋 Next Steps:${NC}"
echo -e "1. Visit: $WEB_APP_URL"
echo -e "2. Complete user registration and persona creation"
echo -e "3. Test natural language task creation"
echo -e "4. Create AI buddy and test chat functionality"
echo -e "5. Review analytics dashboard"

echo -e "\n${GREEN}🌟 Zeitsparkasse foundation platform is now live in Azure!${NC}"
echo -e "${BLUE}Ready to start building time-to-value baselines for the ZeitCoin economy.${NC}"