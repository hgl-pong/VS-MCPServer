## ADDED Requirements

### Requirement: Get build output
The system SHALL provide a `output_get_build` tool that returns the contents of the Build output window.

#### Scenario: Get build output after build
- **WHEN** a build has been performed
- **THEN** tool returns build output text including errors, warnings, and build messages

#### Scenario: Get build output when empty
- **WHEN** no build has been performed
- **THEN** tool returns empty string or indicates no output available

---

### Requirement: Get debug output
The system SHALL provide a `output_get_debug` tool that returns the contents of the Debug output window (including Debug.WriteLine output).

#### Scenario: Get debug output during debugging
- **WHEN** debugging session has produced output
- **THEN** tool returns debug output text including Debug/Trace messages

#### Scenario: Get debug output when empty
- **WHEN** no debug output has been produced
- **THEN** tool returns empty string or indicates no output available
