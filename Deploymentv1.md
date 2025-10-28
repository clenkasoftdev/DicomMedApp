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
