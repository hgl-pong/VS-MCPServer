## ADDED Requirements

### Requirement: Get code diagnostics
The system SHALL provide a `diagnostics_get` tool that returns Roslyn diagnostics (errors, warnings, suggestions) for the specified file or entire solution.

#### Scenario: Get diagnostics for specific file
- **WHEN** file path is provided
- **THEN** tool returns array of diagnostics with severity, message, line, column, and error code

#### Scenario: Get diagnostics for solution
- **WHEN** no file path is provided
- **THEN** tool returns all diagnostics in the solution

#### Scenario: Get diagnostics with severity filter
- **WHEN** severity filter (error/warning/info) is provided
- **THEN** tool returns only diagnostics matching that severity

---

### Requirement: Get error list
The system SHALL provide a `error_list_get` tool that returns the contents of the Visual Studio Error List window.

#### Scenario: Get all errors
- **WHEN** tool is called
- **THEN** tool returns array of errors with description, file, line, column, project, and error code

#### Scenario: Get filtered errors
- **WHEN** filter parameters are provided
- **THEN** tool returns filtered results

---

### Requirement: Apply code fix
The system SHALL provide a `code_fix_apply` tool that applies a suggested code fix for a diagnostic.

#### Scenario: Apply available code fix
- **WHEN** diagnostic has an available code fix and fix ID is provided
- **THEN** code fix is applied and tool returns success with modified file content

#### Scenario: Apply fix with no available fixes
- **WHEN** diagnostic has no available code fixes
- **THEN** tool returns error indicating no fixes available

#### Scenario: Preview code fix
- **WHEN** preview flag is set to true
- **THEN** tool returns what would change without applying the fix
