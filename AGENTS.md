# LINQ Converter AI — Agent Instructions

AI-powered full-stack application: .NET 9 API + Angular 17 UI + Google Cloud Vertex AI (Gemini).

## Project Structure

```
Converter.API/        # .NET 9 Web API (port 5037)
Converter.UI/         # Angular 17 SPA (port 4200)
Converter.API.Tests/  # xUnit test project (multi-target: net9.0, net10.0)
docs/                 # All documentation — prefer linking, not duplicating
api-tests.http        # HTTP request examples for all API endpoints
```

See [Architecture Overview](docs/architecture/ARCHITECTURE.md) for component diagrams and boundaries.

---

## Build & Run Commands

### Backend
```bash
dotnet restore Converter.API/Converter.API.csproj
dotnet build Converter.API/Converter.API.csproj
dotnet run --project Converter.API/Converter.API.csproj      # http://localhost:5037
dotnet watch run --project Converter.API/Converter.API.csproj # watch mode
dotnet test Converter.API.Tests/Converter.API.Tests.csproj
```

### Frontend
```bash
cd Converter.UI
npm install
ng serve --port 4200   # http://localhost:4200
ng build --configuration production
ng test --watch=false --browsers=ChromeHeadless
```

### VS Code Tasks (preferred for full-stack)
- `build-all` — restore + build + npm install (sequential)
- `watch` — .NET API in watch mode
- `ng serve` — Angular dev server
- **F5** "API + UI (Full Stack Debug)" — starts everything with debugger attached

---

## API Design Conventions

- Controller route: `[Route("api/[controller]")]` → `/api/codeconversion/*`
- All async methods use `*Async` suffix
- **All endpoints return 200** (even errors); `success: false` signals failures at the application layer
- Use `BadRequest` (400) only for missing/null inputs; use `StatusCode(500)` for unhandled exceptions
- Response shape always includes `{ success, message, linqCode, analysis, suggestions }`

### Endpoints
| Method | Path | Purpose |
|--------|------|---------|
| POST | `/api/codeconversion/convert` | C# → LINQ conversion |
| POST | `/api/codeconversion/explain` | AI code explanation |
| GET  | `/api/codeconversion/health` | Health check |
| GET  | `/api/codeconversion/patterns` | List LINQ patterns |

Use [api-tests.http](api-tests.http) for request examples.

---

## Service Layer Conventions

- Always define an interface (`IFooService`) and register both with DI
- `IVertexAIService` — low-level: HTTP calls, auth, agent prompt selection
- `ICodeConversionService` — orchestration: delegates to `IVertexAIService`, no direct HTTP
- `IConfigurationValidationService` — validates config at startup; **app will not start if invalid**
- Register services as `Scoped` (per-request); use `AddHttpClient<IFoo, Foo>()` for HTTP clients

---

## Vertex AI / AI Integration

Three internal agents are pre-loaded in `VertexAIService`:
- **CodeConverter** — C# → LINQ, method-syntax preferred
- **CodeExplainer** — structure/performance analysis, educational tone
- **PerformanceAnalyzer** — bottleneck detection (reserved for future multi-agent use)

Agent selection is hardcoded per operation — do not add `EnableMultiAgentConversation` logic unless explicitly requested (currently `false` by default).

Authentication uses [Google Application Default Credentials](https://cloud.google.com/docs/authentication/application-default-credentials); optionally overridden by `VertexAI:ServiceAccountKeyPath`. Keep key files **outside the repo** (e.g., `C:\secrets\`).

See [Vertex AI Integration Guide](docs/setup/VERTEX_AI_INTEGRATION_GUIDE.md) and [Google Cloud Setup](docs/setup/GOOGLE_CLOUD_SETUP.md) for configuration details.

---

## Required Configuration (`appsettings.json`)

```json
{
  "VertexAI": {
    "ProjectId": "<google-cloud-project-id>",   // REQUIRED — app won't start without it
    "Location": "us-central1",                   // REQUIRED
    "ModelName": "gemini-2.0-flash-lite-001",    // REQUIRED
    "Temperature": 0.3,                          // 0.0–1.0
    "MaxOutputTokens": 2048                      // 1–8192
  }
}
```

`ProjectId`, `Location`, and `ModelName` are validated at startup by `IConfigurationValidationService`. Place secrets in `appsettings.Development.json` (git-ignored) or environment variables — never in `appsettings.json`.

---

## Testing Conventions

- Framework: **xUnit** with Moq for mocking
- Test project targets **net9.0** and **net10.0**
- Directory mirrors `Converter.API/` structure: `Controllers/`, `Services/`, `Integration/`, `Models/`, `TestUtilities/`
- All test directories currently empty — follow xUnit patterns when adding tests
- Use `TestUtilities/` for shared mocks and factory helpers

Run with coverage:
```bash
dotnet test Converter.API.Tests/Converter.API.Tests.csproj --collect:"XPlat Code Coverage"
```

---

## Angular Conventions

- Angular 17 **standalone components** — no `NgModule`
- Services use `providedIn: 'root'` (singleton); inject via constructor
- API base URL is hardcoded in `CodeConverterService` as `http://localhost:5037/api/codeconversion`; update for other environments
- HTTP calls use `.pipe(retry({ count: 2, delay: 1000 }), catchError(...))` — keep this pattern
- CORS is restricted to `http://localhost:4200` — update `Program.cs` for production deployments

See [UI Documentation](Converter.UI/README.md) and [Agent Mode Guide](docs/guides/AGENT_MODE_GUIDE.md).

---

## Common Pitfalls

- **App won't start** if `VertexAI:ProjectId`, `Location`, or `ModelName` are missing — check config first
- **CORS errors** in browser: the Angular origin must exactly match the policy in `Program.cs`
- **Auth failures**: ensure `GOOGLE_APPLICATION_CREDENTIALS` env var points to a valid service account JSON, or run `gcloud auth application-default login`
- **Model name validation**: non-standard model names log a warning but don't fail — verify the model exists in your region
- Swagger UI is only enabled in `Development` environment — use `api-tests.http` in other environments

See [Debug Guide](docs/guides/DEBUG_GUIDE.md) for troubleshooting steps.
