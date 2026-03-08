## ADDED Requirements

### Requirement: List all breakpoints
The system SHALL provide a `breakpoint_list` tool that returns all breakpoints in the solution with their locations, conditions, hit counts, and enabled status.

#### Scenario: List breakpoints when present
- **WHEN** one or more breakpoints exist
- **THEN** tool returns array with file path, line number, condition, hit count, and enabled status for each

#### Scenario: List breakpoints when none exist
- **WHEN** no breakpoints are set
- **THEN** tool returns empty array

---

### Requirement: Set a breakpoint
The system SHALL provide a `breakpoint_set` tool that creates a breakpoint at the specified file and line with optional condition and hit count.

#### Scenario: Set simple breakpoint
- **WHEN** valid file path and line number are provided
- **THEN** breakpoint is created and tool returns breakpoint ID and location

#### Scenario: Set conditional breakpoint
- **WHEN** file, line, and condition expression are provided
- **THEN** breakpoint is created with condition and only breaks when condition is true

#### Scenario: Set hit count breakpoint
- **WHEN** file, line, and hit count are provided
- **THEN** breakpoint is created and only breaks after being hit the specified number of times

#### Scenario: Set breakpoint on invalid location
- **WHEN** file path or line number is invalid
- **THEN** tool returns error indicating invalid location

---

### Requirement: Remove a breakpoint
The system SHALL provide a `breakpoint_remove` tool that deletes the specified breakpoint.

#### Scenario: Remove existing breakpoint
- **WHEN** valid breakpoint ID or file:line is provided
- **THEN** breakpoint is removed and tool returns success

#### Scenario: Remove non-existent breakpoint
- **WHEN** specified breakpoint does not exist
- **THEN** tool returns error indicating breakpoint not found

---

### Requirement: Toggle breakpoint
The system SHALL provide a `breakpoint_toggle` tool that enables or disables a breakpoint without removing it.

#### Scenario: Enable disabled breakpoint
- **WHEN** breakpoint exists and is disabled
- **THEN** breakpoint is enabled and tool returns updated status

#### Scenario: Disable enabled breakpoint
- **WHEN** breakpoint exists and is enabled
- **THEN** breakpoint is disabled and tool returns updated status

#### Scenario: Toggle non-existent breakpoint
- **WHEN** specified breakpoint does not exist
- **THEN** tool returns error indicating breakpoint not found

---

### Requirement: Set breakpoint condition
The system SHALL provide a `breakpoint_set_condition` tool that modifies the condition or hit count of an existing breakpoint.

#### Scenario: Set condition on existing breakpoint
- **WHEN** breakpoint exists and condition expression is provided
- **THEN** breakpoint condition is updated and tool returns success

#### Scenario: Set hit count on existing breakpoint
- **WHEN** breakpoint exists and hit count is provided
- **THEN** breakpoint hit count is updated and tool returns success

#### Scenario: Clear condition
- **WHEN** breakpoint exists and empty condition is provided
- **THEN** breakpoint condition is removed
