# Impact Analysis Agent — Team Guide

This guide explains how the **Impact Analysis subagent system** works in GitHub Copilot, why it exists in this project, and how your team can use it daily before making changes to LINQ-Converter-AI.

---

## Why This Exists

LINQ-Converter-AI has several **silent failure points** that are easy to miss during code review:

| Failure | What Happens |
|---------|--------------|
| Rename a JSON field in `ConvertCodeResponse` | Angular UI silently receives `undefined` — no compile error |
| Remove a required `VertexAIConfiguration` property | App crashes on startup with `InvalidOperationException` |
| Change an `ICodeConversionService` method signature | Compile error in controller, but callers in tests are also broken |
| Change the CORS origin in `Program.cs` | All Angular HTTP requests are blocked in the browser |

The Impact Analysis agent exists to **surface these risks before you write a single line of code**, not after code review.

---

## What Was Created

Three agent files and one prompt file were added to `.github/`:

```
.github/
├── agents/
│   ├── impact-analysis.agent.md      ← Orchestrator (the one you invoke)
│   ├── dependency-tracer.agent.md    ← Subagent: finds all affected files
│   └── risk-assessor.agent.md        ← Subagent: classifies risk level
└── prompts/
    └── analyze-change-impact.prompt.md  ← Slash command entry point
```

You only ever interact with **Impact Analysis** or the slash command. The two subagents run automatically in the background.

---

## Prerequisites

You need:
- **VS Code** with the **GitHub Copilot** extension installed and signed in
- The repo cloned locally (the `.github/agents/` folder must be present)
- Copilot **Agent mode** enabled in VS Code Chat

> **Check agent mode**: Open Copilot Chat (`` Ctrl+Alt+I ``), click the agent selector dropdown at the top of the chat panel, and confirm you can see **Impact Analysis** in the list.

---

## How to Use It — 3 Ways

### Method 1: Slash Command (Quickest)

In Copilot Chat, type:

```
/analyze-change-impact
```

Copilot will prompt you to describe your change. Enter something specific:

```
/analyze-change-impact rename the LinqCode property to ConvertedCode in ConvertCodeResponse
```

---

### Method 2: Agent Selector

1. Open Copilot Chat (`` Ctrl+Alt+I ``)
2. Click the **agent selector** at the top of the chat panel
3. Select **Impact Analysis**
4. Type your change description and press Enter

```
add a new required field MaxRetries to VertexAIConfiguration
```

---

### Method 3: Natural Language (Auto-trigger)

With any agent active, use trigger phrases and Copilot will route to Impact Analysis automatically:

```
before I change the ConvertCodeAsync method signature in ICodeConversionService, what will break?
```

```
what's the ripple effect of removing the /health endpoint?
```

```
is it safe to change the CORS origin in Program.cs?
```

---

## What the Agent Produces

For every change you describe, you receive a structured **Impact Analysis Report** with:

### 1. Files Directly Affected
A table of every file that directly references the thing you are changing, with its layer (Backend/Frontend/Config) and role.

### 2. Ripple Effects
Files that are indirectly affected — e.g., a test that mocks a service interface you are modifying.

### 3. Zone-by-Zone Risk Table

The agent checks 7 risk zones specific to this application:

| Zone | What It Checks |
|------|---------------|
| **Zone 1 — API Contract** | JSON field renames that silently break Angular |
| **Zone 2 — Service Interfaces** | `ICodeConversionService` / `IVertexAIService` signatures |
| **Zone 3 — Configuration** | Required `VertexAIConfiguration` fields (startup crash risk) |
| **Zone 4 — Vertex AI Integration** | Agent prompts, auth, HTTP request format |
| **Zone 5 — DI Registration** | `Program.cs` service registrations |
| **Zone 6 — CORS** | Origin policy that blocks Angular requests |
| **Zone 7 — Test Coverage** | What has no test coverage and will have no regression safety net |

Each zone gets a traffic-light rating: **🔴 HIGH**, **🟡 MEDIUM**, or **🟢 LOW**.

### 4. Critical Actions Checklist
A `[ ]` checklist of specific things that **must be done before merging** — with exact file names.

### 5. Verification Steps
Concrete commands to run to confirm nothing broke after the change.

---

## Example Session

**Developer types:**

> Switch `ICodeConversionService.ConvertCodeAsync` to return `Task<string>` instead of `Task<ConvertCodeResponse>`

**Agent produces:**

```
# Impact Analysis Report
Change: Change ConvertCodeAsync return type from Task<ConvertCodeResponse> to Task<string>
Overall Risk: 🔴 HIGH

## Files Directly Affected (3 files)
| File | Layer | Role |
|------|-------|------|
| Services/ICodeConversionService.cs | Backend | Interface definition |
| Services/CodeConversionService.cs | Backend | Implementation |
| Controllers/ExpressionController.cs | Backend | Caller — deserializes return value |

## Ripple Effects (2 files)
| File | Layer | Why Affected |
|------|-------|-------------|
| Converter.UI/src/app/services/code-converter.service.ts | Frontend | Maps ConversionResult fields from response |
| api-tests.http | Contract | Response shape examples become invalid |

## Risk Assessment
| Zone | Affected? | Risk | Reason |
|------|-----------|------|--------|
| Zone 1 — API Contract | Yes | 🔴 HIGH | ConvertCodeResponse fields (LinqCode, Analysis, Suggestions) will be lost |
| Zone 2 — Service Interfaces | Yes | 🔴 HIGH | Interface + implementation + controller all need coordinated update |
...

## Critical Actions Before Merging
- [ ] Update ExpressionController.cs — re-parse string return into response fields
- [ ] Update CodeConverterService.ts — adjust TypeScript interface mapping
- [ ] Update api-tests.http — update expected response shape
```

---

## How It Works Internally

```
You describe a change
        │
        ▼
┌─────────────────────────┐
│   Impact Analysis        │  ← Orchestrator agent
│   (impact-analysis       │     Reads your request
│    .agent.md)            │     Decides what to trace
└────────┬────────────────┘
         │ delegates to
         ▼
┌─────────────────────────┐
│   Dependency Tracer      │  ← Subagent (read-only)
│   (dependency-tracer     │     Searches all files
│    .agent.md)            │     for references
└────────┬────────────────┘
         │ returns trace report
         ▼
┌─────────────────────────┐
│   Risk Assessor          │  ← Subagent (read-only)
│   (risk-assessor         │     Classifies each risk
│    .agent.md)            │     zone based on trace
└────────┬────────────────┘
         │ returns risk report
         ▼
┌─────────────────────────┐
│   Final Impact Report    │  Compiled by orchestrator
│   delivered to you       │  No code was changed
└─────────────────────────┘
```

The subagents (`Dependency Tracer`, `Risk Assessor`) only have `read` and `search` tools — they **cannot edit files**. The orchestrator has `agent` tool access to delegate to them. Nothing in this system modifies your codebase.

---

## Team Workflow Recommendation

Adopt this as a **pre-implementation gate** — run it before opening a branch:

```
1. Developer identifies a change to make
          ↓
2. Run /analyze-change-impact in Copilot Chat
          ↓
3. Review the Impact Report
          ↓
4. If Overall Risk is 🔴 HIGH → discuss with team before proceeding
   If Overall Risk is 🟡 MEDIUM → note the critical actions checklist
   If Overall Risk is 🟢 LOW → proceed with confidence
          ↓
5. Make the change, following the checklist from the report
          ↓
6. Run verification steps from the report
          ↓
7. Open PR
```

---

## Adding This to a New Repository

If you want to replicate this agent system in another project, copy these four files:

```
.github/agents/impact-analysis.agent.md
.github/agents/dependency-tracer.agent.md
.github/agents/risk-assessor.agent.md
.github/prompts/analyze-change-impact.prompt.md
```

Then update the **Codebase Map** section in `dependency-tracer.agent.md` and the **7 Risk Zones** in `risk-assessor.agent.md` to match the architecture of the new project. The orchestrator (`impact-analysis.agent.md`) and the prompt file require no changes.

---

## Troubleshooting

**Agent does not appear in the selector**
- Confirm `.github/agents/impact-analysis.agent.md` exists in the repo root
- Reload VS Code window (`` Ctrl+Shift+P `` → *Developer: Reload Window*)
- Verify Copilot Chat is in **Agent mode**, not Ask or Edit mode

**Agent responds but doesn't invoke subagents**
- Confirm `dependency-tracer.agent.md` and `risk-assessor.agent.md` are in `.github/agents/`
- The `agents:` frontmatter in `impact-analysis.agent.md` must exactly match the `name:` value in each subagent file
- Current names: `Dependency Tracer` and `Risk Assessor` (case-sensitive)

**Report is vague or misses files**
- Be more specific in your change description — name the exact class, method, or property
- Example of vague: *"change the response model"*
- Example of specific: *"rename the `LinqCode` property to `ConvertedCode` in `ConvertCodeResponse` in `Models/ApiModels.cs`"*

**Subagent says "unverified"**
- This means the agent found a reference but could not confirm the relationship by reading the file
- Open the flagged file manually and verify — do not assume it is safe to ignore
