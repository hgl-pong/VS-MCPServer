## ADDED Requirements

### Requirement: Get call stack
The system SHALL provide a `debugger_call_stack` tool that returns the current call stack with frame information.

#### Scenario: Get call stack in break mode
- **WHEN** debugger is in break mode
- **THEN** tool returns array of stack frames with frame index, method name, file path, line number, and module name

#### Scenario: Get call stack when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Set active stack frame
The system SHALL provide a `debugger_set_frame` tool that changes the active stack frame for variable inspection.

#### Scenario: Switch to valid frame
- **WHEN** debugger is in break mode and valid frame index is provided
- **THEN** active frame changes and subsequent variable inspections use that frame's context

#### Scenario: Switch to invalid frame
- **WHEN** frame index is out of range
- **THEN** tool returns error indicating invalid frame index

---

### Requirement: List all threads
The system SHALL provide a `debugger_threads` tool that returns all threads in the current debugged process.

#### Scenario: List threads in break mode
- **WHEN** debugger is in break mode
- **THEN** tool returns array of threads with thread ID, name, and current location

#### Scenario: List threads when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Set active thread
The system SHALL provide a `debugger_set_thread` tool that switches the active thread for inspection.

#### Scenario: Switch to valid thread
- **WHEN** debugger is in break mode and valid thread ID is provided
- **THEN** active thread changes and subsequent inspections use that thread's context

#### Scenario: Switch to invalid thread
- **WHEN** thread ID does not exist in current process
- **THEN** tool returns error indicating invalid thread ID
