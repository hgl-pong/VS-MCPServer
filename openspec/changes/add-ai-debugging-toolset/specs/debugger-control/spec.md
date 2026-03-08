## ADDED Requirements

### Requirement: Get debugger state
The system SHALL provide a `debugger_state` tool that returns the current debugging state including mode (Design/Break/Run), current process, and current thread.

#### Scenario: Debugger in design mode
- **WHEN** no debugging session is active
- **THEN** tool returns `mode: "design"` with no process info

#### Scenario: Debugger in break mode
- **WHEN** execution is paused at a breakpoint
- **THEN** tool returns `mode: "break"` with current process ID, name, and thread ID

#### Scenario: Debugger in run mode
- **WHEN** program is executing (not paused)
- **THEN** tool returns `mode: "run"` with process info

---

### Requirement: Start debugging session
The system SHALL provide a `debugger_start` tool that starts debugging the current solution's startup project.

#### Scenario: Start with solution loaded
- **WHEN** a solution is loaded and startup project is configured
- **THEN** debugging begins and tool returns success with process info

#### Scenario: Start without startup project
- **WHEN** no startup project is configured
- **THEN** tool returns error indicating no startup project

---

### Requirement: Stop debugging session
The system SHALL provide a `debugger_stop` tool that terminates the current debugging session.

#### Scenario: Stop active debug session
- **WHEN** debugging is active (any mode except design)
- **THEN** debugging stops and tool returns success

#### Scenario: Stop when not debugging
- **WHEN** no debug session is active
- **THEN** tool returns error indicating no active session

---

### Requirement: Continue execution
The system SHALL provide a `debugger_continue` tool that resumes execution from a breakpoint.

#### Scenario: Continue from breakpoint
- **WHEN** debugger is in break mode
- **THEN** execution resumes and tool returns success

#### Scenario: Continue when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Step into
The system SHALL provide a `debugger_step_into` tool that steps into the next statement, entering function calls.

#### Scenario: Step into function call
- **WHEN** current statement is a function call and debugger is in break mode
- **THEN** execution enters the called function and pauses at first statement

#### Scenario: Step into when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Step over
The system SHALL provide a `debugger_step_over` tool that executes the next statement without entering function calls.

#### Scenario: Step over function call
- **WHEN** current statement is a function call and debugger is in break mode
- **THEN** function executes completely and pauses at next statement

#### Scenario: Step over when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Step out
The system SHALL provide a `debugger_step_out` tool that executes until the current function returns.

#### Scenario: Step out of function
- **WHEN** debugger is in break mode inside a function
- **THEN** execution continues until function returns, then pauses at caller

#### Scenario: Step out when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Run to cursor
The system SHALL provide a `debugger_run_to_cursor` tool that executes until reaching the cursor position in the specified file.

#### Scenario: Run to cursor position
- **WHEN** debugger is in break mode and valid cursor position is specified
- **THEN** execution continues until reaching that line, then pauses

#### Scenario: Run to cursor with invalid position
- **WHEN** specified file or line is invalid
- **THEN** tool returns error indicating invalid position
