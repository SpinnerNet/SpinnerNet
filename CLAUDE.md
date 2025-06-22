# Spinner.Net Development Guide

**Two-app architecture**: Public Razor + Authenticated Blazor hybrid

## 🏗️ Apps
- **SpinnerNet.Web**: Public Razor Pages (anonymous users)
- **SpinnerNet.App**: Blazor hybrid with MudBlazor (registered users only)

## 🔐 Azure (rg-spinnernet-proto)
- Web: spinnernet-3lauxg.azurewebsites.net  
- App: spinnernet-app-3lauxg.azurewebsites.net
- KeyVault: kv-spinnernet-3lauxg

## ✅ DONE
- Azure AD multi-tenant auth (personal + work accounts)
- Authentication endpoints: /Account/login, /Account/logout

## 🏃 CURRENT SPRINT
**Public Razor Pages + Registered User Hybrid App**
- Working on public site content and navigation
- Building authenticated features in SpinnerNet.App for registered users

## 🚨 CRITICAL RULES

**SECRETS**: NEVER in code! KeyVault/env vars only
```json
"AzureAd": { "ClientId": "", "Domain": "" }  // Empty in appsettings!
```

**MUDBLAZOR**: Follow official docs exactly - no custom CSS overrides
**TEXT**: ALL in .resx files (XML-encoded: & → &amp;)
**ARCHITECTURE**: Vertical slice - one file per feature
**AI**: Only via Semantic Kernel, never direct HTTP

## 🚀 Deploy SpinnerNet.App
```bash
cd src && dotnet publish SpinnerNet.App/*.csproj -c Release -o publish
cd publish && zip -r ../deployment.zip . && cd ..
az webapp deploy -g rg-spinnernet-proto -n spinnernet-app-3lauxg --src-path deployment.zip --type zip
```