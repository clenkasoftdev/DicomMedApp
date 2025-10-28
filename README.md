# DICOM Medical App

Goal
----
This repository provides a backend Web API for ingesting, storing and serving DICOM medical images and metadata and a separate Angular 15 frontend deployed to Azure Static Web Apps.

- Backend: .NET 6 minimal API — parses DICOM files, stores metadata in PostgreSQL, stores files locally or in Azure Blob Storage (configurable), exposes endpoints for upload/download and browsing patients/studies/series/instances.
- Frontend: Angular 15 SPA (separate project) deployed to Azure Static Web Apps (example origin used during development: https://dicommedapp.z6.web.core.windows.net).

What’s included
---------------
- Backend:
  - Program.cs (minimal API + DI + CORS)
  - Endpoints/ — grouped minimal API endpoints (patients, studies, series, files, weather)
  - Infrastructure/Data/ApplicationDbContext.cs — EF Core DbContext + model configuration
  - Infrastructure/Migrations/ — EF Core migrations
  - Services/ — Local and Azure blob implementations for DICOM files
  - Dockerfile + docker‑compose guidance for local development
  - Swagger (only enabled in Development)
- Frontend:
  - Angular 15 project (not inside this backend repo). Built output is deployed to Azure Static Web Apps.

Quick facts
-----------
- Backend: .NET 6 (C# 10), EF Core (Npgsql), fo-dicom for DICOM parsing
- Storage: PostgreSQL (production) + local filesystem or Azure Blob Storage for DICOM files
- Dev containers: Docker / docker-compose; optional Azurite for local blob emulation
- API base paths (examples):
  - GET /api/patients
  - GET /api/patients/{id}/studies
  - POST /api/dicom/upload (multipart/form-data `file`)
  - GET /api/dicom/download/{instanceId}

Configuration overview
----------------------
- Use environment variables or platform config (Azure App Settings). Nested keys use double underscore:
  - ConnectionStrings__DefaultConnection — Npgsql connection string
  - UseCloudStorage — true|false
  - AzureStorage__ConnectionString — blob connection string (or UseDevelopmentStorage)
  - AzureStorage__UseManagedIdentity — true|false
  - AzureStorage__BlobServiceEndpoint — when using managed identity
  - AzureStorage__ContainerName — e.g. `dicomfiles`
  - Cors__AllowedOrigins__0, Cors__AllowedOrigins__1, ... — explicit frontend origins (scheme + host + port)
  - ASPNETCORE_ENVIRONMENT — Development|Production

CORS and frontend
-----------------
- Frontend origin must be listed exactly (scheme + host + port) in the backend configuration (example: `Cors__AllowedOrigins__0=https://dicommedapp.z6.web.core.windows.net`).
- If __AllowCredentials()__ is used on the server, the browser must send credentials (fetch with `credentials: 'include'` or Angular `withCredentials: true`).

Local quick start
-----------------
1. Configure .env or pass env vars (ConnectionStrings__DefaultConnection, AzureStorage settings).
2. Start dependent services and backend:
   - Recommended: docker compose up --build (API, Postgres, Azurite)
3. Open API at http://localhost:7208 (docker-compose maps port 80→7208 by default in examples).
4. Swagger UI available in Development: http://localhost:7208/swagger

Migrations
----------
- Migrations live under `Infrastructure/Migrations/`.
- To create/apply locally:
  - dotnet ef migrations add InitialCreate -o Infrastructure/Migrations
  - dotnet ef database update --connection "<your-connection-string>"
- You can run EF commands inside the SDK container if you don't have the SDK locally.

Security & production notes
---------------------------
- Never commit production secrets (use __User Secrets__ locally and __Azure App Settings__ in cloud).
- Prefer managed identity for Azure Blob access (DefaultAzureCredential) in production.
- Apply DB migrations explicitly during deployment (avoid automatic Database.Migrate() in Production unless you fully control the environment).
- Use immutable Docker tags (v1.0.0, SHA) rather than `:latest` for production.

Where to go next
----------------
- See DEPLOYMENT.md for full deployment flows: local (docker-compose), Docker Hub publish, Azure App Service (container) and Azure Static Web Apps (Angular frontend).

Contributing
------------
- Follow the .editorconfig / coding conventions used in the project.
- Run tests and linting for the Angular project before pushing front-end changes.