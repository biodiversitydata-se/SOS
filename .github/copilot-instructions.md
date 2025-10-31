# Repository Custom Instructions for GitHub Copilot

## Overview
- SOS (Species Observation System) aggregates species observations from many sources and exposes search, aggregation and export APIs. 
- The repository contains several backend APIs (Observations, Analysis, Administration, Harvest, Hangfire job server), shared libraries, frontend clients (Angular, Blazor).
- Key project: Observations API at `Src/SOS.Observations.Api` — most feature work and tests reference this.

## Tech stack (high-level)
- C# .NET (projects target .NET 9 and .NET Standard 2.1). Prefer C# for new backend work.
- ASP.NET Core.
- Elasticsearch (Elastic.Clients.Elasticsearch), MongoDB, Redis referenced.
- Tests: xUnit, integration tests use TestContainers / WebApplicationFactory.

## Project structure (high level, most important entry points)
- Src/ — all server and client projects.
  - `Src/SOS.Observations.Api` — main Observations API.
  - `Src/SOS.Analysis.Api` — Analysis API.
  - `Src/SOS.Administration.Api` — Administration API.
  - `Src/SOS.ElasticSearch.Proxy` — Elasticsearch proxy used by GeoServer.
  - `Src/SOS.Hangfire.JobServer` - background job server using Hangfire. Used for harvesting and processing observations.
  - `Src/SOS.Harvest`, `Src/SOS.Export`, — supporting services.
  - `Src/SOS.Lib` — shared library, models, repositories.
  - `Src/SOS.AppHost` — .Net Aspire AppHost used for local orchestration.
- Tests/
  - `Tests/Unit` — Unit tests.
  - `Tests/Integration` — Integration tests using WebApplicationFactory and TestContainers.
  - `Tests/LiveIntegration` - tests that call live services; Only used by developers when running against live services.
- Docs/ - design and API docs (useful references).

## Established patterns & coding guidelines (follow these)
- Follow existing C# conventions used in the codebase:
  - PascalCase for public members, XML doc comments are common.
  - Dependency Injection (constructor injection) is standard.
  - Use DTOs in `SOS.Shared.Api.Dtos` for API contracts.
  - Keep changes focused and unit-tested. Add xUnit tests for new behavior.
- Serialization / JSON options are centralized via `SetupJsonSerialization`. Avoid ad-hoc JSON settings.
- Keep top-level Program.cs minimal; use extension methods (see `Extensions/DependencyInjectionExtensions.cs`, `Extensions/SwaggerExtensions.cs`).
- Dependency Injection: Register all services and settings via DI. Use the provided `DependencyInjection` extensions.