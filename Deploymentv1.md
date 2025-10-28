# Deployment Guide

This file documents the current deployment flows: local development (docker-compose), pushing backend image to Docker Hub, deploying the backend to Azure App Service (container) and the Angular 15 frontend to Azure Static Web Apps.

Prerequisites
-------------
- Docker & docker-compose
- .NET 6 SDK (optional if using SDK container)
- Azure CLI (az)
- Docker Hub account (username & token)
- Azure subscription
- Optional local tools: Azure Storage Explorer, VS Code with Azure extensions

1 — Local development (recommended)
-----------------------------------
- Use docker-compose that runs Postgres, Azurite (optional), and the API.
- Example environment (in a .env or docker-compose service env):
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=dicom_medical_db;Username=postgres;Password=postgres
  - AzureStorage__ConnectionString=UseDevelopmentStorage=true
  - AzureStorage__ContainerName=dicomfiles
  - UseCloudStorage=false
  - Cors__AllowedOrigins__0=http://localhost:4200
- Start:
  - docker compose up --build
- Verify:
  - http://localhost:7208/api/patients
  - Swagger (Development): http://localhost:7208/swagger

2 — Migrations
--------------
- Create/refresh migrations after model changes:
  - Remove old migrations if starting fresh: use PowerShell `Remove-Item -Recurse -Force .\Infrastructure\Migrations` or bash `rm -rf Infrastructure/Migrations`
  - dotnet ef migrations add InitialCreate -o Infrastructure/Migrations
- Apply migrations to the target DB explicitly:
  - dotnet ef database update --connection "Host=<host>;Port=5432;Database=<db>;Username=<user>@<server>;Password=<pw>;Ssl Mode=Require;Trust Server Certificate=True"
- If you do not have the SDK locally, run EF from the SDK container:
  - docker run --rm -v "$(pwd)":/src -w /src mcr.microsoft.com/dotnet/sdk:6.0 bash -c "dotnet tool restore || true; dotnet ef database update --connection \"<your-connection-string>\""

3 — Build and push backend image to Docker Hub
-----------------------------------------------
- Build:
  - docker build -t <dockerhub-user>/dicom-med-app:latest .
- Tag (recommended immutable tag):
  - docker tag <dockerhub-user>/dicom-med-app:latest <dockerhub-user>/dicom-med-app:v1.0.0
- Push:
  - docker login
  - docker push <dockerhub-user>/dicom-med-app:v1.0.0

4 — Deploy backend to Azure App Service (container)
---------------------------------------------------
- Create resources (example):
  - az group create -n rg-dicom -l <location>
  - az appservice plan create -g rg-dicom -n dicom-plan --is-linux --sku P1V2
- Create webapp with Docker Hub image:
  - For public image:
    - az webapp create -g rg-dicom -p dicom-plan -n <appname> --deployment-container-image-name docker.io/<dockerhub-user>/dicom-med-app:v1.0.0
  - For private image:
    - az webapp config container set -g rg-dicom -n <appname> --docker-custom-image-name <dockerhub-user>/dicom-med-app:v1.0.0 --docker-registry-server-url https://index.docker.io --docker-registry-server-user <user> --docker-registry-server-password <token>
- Configure App Settings (environment variables). Use double underscores for nested keys:
  - az webapp config appsettings set -g rg-dicom -n <appname> --settings \
    "ConnectionStrings__DefaultConnection=Host=<pg-host>;Port=5432;Database=dicom_medical_db;Username=myuser@pgclenkatest;Password=<pw>;Ssl Mode=Require;Trust Server Certificate=True" \
    "UseCloudStorage=true" \
    "AzureStorage__ConnectionString=<storage-conn-or-UseDevelopmentStorage=true>" \
    "AzureStorage__ContainerName=dicomfiles" \
    "Cors__AllowedOrigins__0=https://dicommedapp.z6.web.core.windows.net" \
    "ASPNETCORE_ENVIRONMENT=Production"
- Restart & monitor:
  - az webapp restart -g rg-dicom -n <appname>
  - az webapp log tail -g rg-dicom -n <appname>

5 — Enable Managed Identity for Azure Blob access (recommended)
----------------------------------------------------------------
- Assign system identity to the Web App:
  - az webapp identity assign -g rg-dicom -n <appname>
- Grant Storage role to the identity:
  - PRINCIPAL_ID=$(az webapp show -g rg-dicom -n <appname> --query identity.principalId -o tsv)
  - az role assignment create --assignee $PRINCIPAL_ID --role "Storage Blob Data Contributor" --scope /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Storage/storageAccounts/<storageAccount>
- Set app settings:
  - az webapp config appsettings set -g rg-dicom -n <appname> --settings "AzureStorage__UseManagedIdentity=true" "AzureStorage__BlobServiceEndpoint=https://<storageAccount>.blob.core.windows.net/"

6 — Deploy Angular 15 frontend to Azure Static Web Apps
--------------------------------------------------------
- Build frontend (example):
  - ng build --configuration production
- Deploy options:
  - Use Azure Static Web Apps (recommended for SPA):
    - Create a Static Web App in Azure and wire it to your frontend repo (or use `az` / GitHub Actions).
    - The deployed site origin must be added to backend CORS (example origin: `https://dicommedapp.z6.web.core.windows.net`)
  - Or deploy to Azure Storage Static Website + CDN (then add that origin to backend CORS)
- If using GitHub Actions for Static Web Apps, set the correct build and output folder (typically `dist/<app-name>`).

7 — CORS notes (critical)
--------------------------
- Backend reads `Cors:AllowedOrigins` as a string[] from configuration.
- When setting App Settings in Azure, use indexed keys:
  - Cors__AllowedOrigins__0=https://dicommedapp.z6.web.core.windows.net
  - Cors__AllowedOrigins__1=https://localhost:4200
- If server uses `.AllowCredentials()` then the client must send credentials (`withCredentials: true` in Angular or `credentials: 'include'` in fetch). If you do not use credentials, remove `.AllowCredentials()`.

8 — CI / CD (recommended)
--------------------------
- Build image, tag with commit SHA or semver, push to Docker Hub.
- Use GitHub Actions to:
  - Build and push image to Docker Hub.
  - Optionally run `az webapp config container set` to update App Service image and restart it.
- Store secrets in GitHub Secrets (DOCKERHUB_TOKEN, AZURE_CREDENTIALS).

Troubleshooting
---------------
- CORS blocked: confirm backend returns `Access-Control-Allow-Origin` for the frontend origin (use curl with `-H "Origin: <origin>"`).
- DB connection errors: verify ConnectionStrings__DefaultConnection in App Settings and that PostgreSQL firewall allows the App Service outbound IPs or use VNet/private endpoint.
- Migration issues: ensure migrations were generated after any model/ToTable changes and applied to the correct database.
- Image not updated in App Service: use an immutable tag and update the webapp container settings to point to the new tag.

Appendix: Useful commands
-------------------------
- Build & push:
  - docker build -t <user>/dicom-med-app:v1.0.0 .
  - docker push <user>/dicom-med-app:v1.0.0
- Force App Service to pull latest:
  - az webapp config container set -g rg-dicom -n <appname> --docker-custom-image-name docker.io/<user>/dicom-med-app:v1.0.0
  - az webapp restart -g rg-dicom -n <appname>

If you want, I can generate:
- A GitHub Actions workflow to build/push the backend to Docker Hub and update the App Service image, and
- A recommended Static Web Apps GitHub Actions workflow for the Angular frontend.