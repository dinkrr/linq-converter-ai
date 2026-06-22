---
description: "Review Acceptance Criteria (AC), implement the required code changes, then run dotnet build to verify the solution compiles without errors."
name: "Implement AC"
argument-hint: "Paste your Acceptance Criteria here..."
agent: "agent"
tools: [read_file, grep_search, file_search, semantic_search, replace_string_in_file, create_file, run_in_terminal]
---

You are implementing a feature based on Acceptance Criteria (AC) for the **linq-converter-ai** project (.NET 9 API + Angular 17 UI).

## Input

The Acceptance Criteria to implement:

```
$args
```

## Workflow

### Step 1 — Review the AC

Carefully read each acceptance criterion and:
- Identify the affected layers (API controller, service, model, Angular component, test)
- Note any new endpoints, models, or behaviour changes required
- Flag anything ambiguous before writing code — ask the user once if clarification is needed

### Step 2 — Explore relevant code

Before writing anything, locate the files that need to change:
- Controllers live in `Converter.API/Controllers/`
- Services and interfaces live in `Converter.API/Services/`
- Models live in `Converter.API/Models/`
- Angular components and services live in `Converter.UI/src/app/`

Follow the conventions in [csharp-dotnet instructions](../instructions/csharp-dotnet.instructions.md) and [controller instructions](../instructions/controller.instructions.md).

### Step 3 — Implement the code

Apply only the changes required to satisfy the AC. Do **not**:
- Refactor unrelated code
- Add comments or docstrings to unchanged code
- Add error handling for scenarios not covered by the AC

Key project conventions to respect:
- All async methods use the `*Async` suffix
- Always define an `IFooService` interface alongside the implementation
- Register new services as `Scoped` in `Program.cs`
- **All API endpoints return 200**; use `success: false` in the response body to signal failures
- Response shape: `{ success, message, linqCode, analysis, suggestions }`
- Angular: use standalone components, `providedIn: 'root'` services, and the `.pipe(retry({ count: 2, delay: 1000 }), catchError(...))` HTTP pattern

### Step 4 — Build and verify

After all edits are made, run:

```bash
dotnet build Converter.API/Converter.API.csproj
```

If the build **fails**:
1. Read the compiler errors carefully
2. Fix each error in the relevant file
3. Re-run the build until it succeeds with **0 errors**

### Step 5 — Report

Once the build is green, provide a concise summary:
- Which files were created or modified
- How each AC item is satisfied
- Any trade-offs or follow-up work needed
