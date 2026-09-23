# petit-net

A standalone .NET 10 backend project, built from scratch as an experiment — not connected to or reusing code from any other project.

## Purpose of this repository

This repo exists to **test Spec-Driven Development (SDD) end-to-end using [OpenCode](https://opencode.ai) with a local, open-weight model** ([Qwen3.6](https://ollama.com), served via [Ollama](https://ollama.com)), rather than a hosted/commercial model.

Three things are being evaluated:

1. **The SDD flow itself** — does the intent → spec → plan → implementation pipeline, with explicit human review gates at each stage, actually catch problems earlier and produce better-structured code than a single "build me X" prompt?
2. **Feasibility with free, local tooling** — how usable is this workflow with a free coding agent (OpenCode) and a free, open-weight model, as opposed to a paid, hosted coding assistant?
3. **Performance and quality on local hardware** — throughput/speed of a local model on consumer hardware (see below), and the accuracy/quality of what it produces compared to what a hosted, larger model would be expected to produce.

This is a controlled experiment, not (yet) a production system — the domain (pet management) is intentionally simple so that the *process* is the thing under test, not the domain complexity.

## Hardware & model

| | |
|---|---|
| Machine | MacBook Pro, Apple M5 Pro, 48 GB unified memory |
| Model runtime | [Ollama](https://ollama.com) |
| Model | `qwen3.6:latest` (~23 GB on disk) |
| Coding agent | [OpenCode](https://opencode.ai) |

### Qwen3.6 configuration used here, and why

| Parameter | Value | Reason |
|---|---|---|
| `num_ctx` | `32768` | Ollama defaults to a 4096-token context window, which is too small for agentic tool-calling (system prompt + tool schemas + `AGENTS.md`/`CONSTITUTION.md` + conversation history can exceed that on their own). 32K was chosen as a value that comfortably fits the SDD workflow's context needs while staying well within the 48 GB unified memory budget on this machine (model weights alone occupy ~23 GB, leaving a shared pool for OS, other apps, and the KV cache). Qwen3.6 natively supports up to 262K context, so this is a deliberate, hardware-driven ceiling — not a model limitation. |
| `temperature` | `0.1` | SDD artifacts (intent/spec/plan documents, and later, code) benefit from determinism and consistency rather than creative variation — a low temperature reduces run-to-run drift when regenerating a document. |
| `tool_call` | `true` | Required for OpenCode's agentic tool-calling loop (file read/write, bash) to function against an OpenAI-compatible endpoint. |
| Thinking / reasoning mode | Disabled (`/set nothink`, saved as a separate model tag) | Qwen3.6 is a hybrid reasoning model that emits a visible "thinking" block before acting. In practice this occasionally caused the agent to stall mid-task (a known interoperability issue between Ollama's reasoning output and OpenCode's OpenAI-compatible client). Disabling thinking trades some multi-step reasoning depth for reliability in the tool-calling loop — an acceptable trade-off for the largely mechanical steps involved in implementing an approved plan. |

## How SDD works in this repo

Every feature goes through four stages, each producing a reviewed, version-controlled markdown artifact before the next stage may begin:

```
brainstorm → intent.md → [human approval] → spec.md → [human approval] → plan.md → [human approval] → implementation → verification
```

Nothing proceeds to the next stage while the current artifact's `status` is `draft`. A human must change it to `approved` after reviewing.

### Directory structure

```
petit-net/
├── AGENTS.md                     ← persistent context OpenCode reads on every session
├── .opencode/
│   └── agent/
│       ├── intent-writer.md      ← drafts intent.md
│       ├── spec-writer.md        ← drafts spec.md from an approved intent.md
│       └── planner.md            ← drafts plan.md from an approved spec.md
├── specs/
│   ├── CONSTITUTION.md           ← project-wide architecture rules & conventions
│   └── INTENT/
│       └── I-N-{feature-slug}/
│           ├── intent.md
│           ├── spec.md
│           └── plan.md
├── src/
│   └── Petit.WebApi/
│       └── Features/{Feature}/   ← one endpoint = one file, feature-folder structure
└── tests/
```

### Agents and their roles

Three custom OpenCode agents live in `.opencode/agent/`, each scoped to exactly one stage of the pipeline and explicitly forbidden from writing code:

| Agent | File | Responsibility |
|---|---|---|
| `intent-writer` | `.opencode/agent/intent-writer.md` | Turns a rough feature idea into a draft `intent.md` (Ask, Why, feature-specific Constraints, Open Questions). Does not design or code. |
| `spec-writer` | `.opencode/agent/spec-writer.md` | Reads an **approved** `intent.md` plus `CONSTITUTION.md`, produces `spec.md` (Requirements, Design). Flags anything that would violate the constitution instead of silently designing around it. |
| `planner` | `.opencode/agent/planner.md` | Reads an **approved** `spec.md` plus `CONSTITUTION.md` and the existing `src/` tree, produces `plan.md` (file-by-file plan, build order, risks, completion criteria). |

Actual code is written by OpenCode's **built-in `Build` agent** — switched to only after `plan.md` is approved — following the plan step by step (implemented incrementally, one or two steps at a time, to keep the local model's context load manageable and make deviations easier to catch).

### Human roles per artifact

This is a solo experiment, so one person (the repo owner) plays every role below — but the roles are kept conceptually distinct, matching how this would map onto a team:

| Artifact | Primary owner (role) | What they check |
|---|---|---|
| `intent.md` | Product Manager / Product Owner | Is this the right thing to build, and why — business justification and scope |
| `spec.md` | Tech Lead / Architect | Does the design respect `CONSTITUTION.md`? Are open questions (pagination, delete strategy, enum values, etc.) resolved deliberately, not silently assumed? |
| `plan.md` | Developer (implementer) | Is the file list complete and internally consistent? Is the build order actually buildable (e.g. no step referencing a type defined in a later step)? |
| Implementation | Developer + Build agent | Does each incremental step match the plan and the constitution before moving to the next? |
| Verification | QA / Tester | Do all Requirements (R1, R2, …) from `spec.md` have real test coverage, including the edge cases raised in "Flagged concerns"? |

## Stack

- .NET 10, C# 13, Minimal API (no controllers)
- EF Core 10 + Npgsql (PostgreSQL)
- Authentication: Keycloak (JWT Bearer)
- Full architecture and coding conventions: see [`specs/CONSTITUTION.md`](./specs/CONSTITUTION.md)

## Status

Features are added incrementally, each going through the full intent → spec → plan → implementation cycle. See `specs/INTENT/` for the list of features and their current stage (`draft` / `in_progress` / `complete` — tracked in each feature's `intent.md`/`spec.md`/`plan.md` frontmatter).
