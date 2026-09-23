# AGENTS.md

## Project

petit-net — a small .NET solution. The entry point is `petit-net.slnx`.

## Structure

- `src/` — production projects
- `tests/` — test projects
- `specs/` — design and specification documents (`CONSTITUTION.md` holds the project's guiding rules)
- `.opencode/` — opencode local configuration

## Conventions

- Follow the rules in `specs/CONSTITUTION.md` when it has relevant guidance.
- Keep the solution lean: one project per concern, small focused tests.
- Do not add dependencies unless required; prefer the .NET base library.

## Commands

- `dotnet build` — build the solution
- `dotnet test` — run all tests
- `dotnet format` — ensure consistent formatting before committing