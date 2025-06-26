#!/bin/bash

echo "🚀 Deploying SpinnerNet.App to Azure..."
echo "Target: spinnernet-app-3lauxg.azurewebsites.net"

# Set working directory
cd /Users/peterclinton/Desktop/repos/Spinner.Net/Public/src

# 1. Build and publish the app
echo "📦 Building SpinnerNet.App in Release mode..."
dotnet publish SpinnerNet.App/SpinnerNet.App.csproj -c Release -o ./publish/app

# Check if build succeeded
if [ $? -ne 0 ]; then
    echo "❌ Build failed! Please fix build errors first."
    exit 1
fi

# 2. Create deployment package
echo "📦 Creating deployment package..."
cd publish/app
zip -r ../../deployment-app.zip . -q
cd ../..

# Check if zip succeeded
if [ ! -f deployment-app.zip ]; then
    echo "❌ Failed to create deployment package!"
    exit 1
fi

echo "📦 Deployment package created: deployment-app.zip ($(du -h deployment-app.zip | cut -f1))"

# 3. Deploy to Azure
echo "☁️ Deploying to Azure Web App: spinnernet-app-3lauxg..."
az webapp deploy \
    --resource-group rg-spinnernet-proto \
    --name spinnernet-app-3lauxg \
    --src-path deployment-app.zip \
    --type zip

# Check deployment result
if [ $? -eq 0 ]; then
    echo "✅ Deployment successful!"
    echo "🌐 App URL: https://spinnernet-app-3lauxg.azurewebsites.net"
    echo ""
    echo "📋 Next steps:"
    echo "1. Visit https://spinnernet-app-3lauxg.azurewebsites.net"
    echo "2. Test user registration flow"
    echo "3. Complete persona creation wizard"
    echo "4. Verify Cosmos DB documents are created"
    
    # Optional: Open in browser
    echo ""
    read -p "Would you like to open the app in your browser? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        open "https://spinnernet-app-3lauxg.azurewebsites.net"
    fi
else
    echo "❌ Deployment failed! Check Azure CLI output above."
    exit 1
fi

# Cleanup
echo "🧹 Cleaning up temporary files..."
rm -f deployment-app.zip
rm -rf publish/app

echo "✨ Done!"