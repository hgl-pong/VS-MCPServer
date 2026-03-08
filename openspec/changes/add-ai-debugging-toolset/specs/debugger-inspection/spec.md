## ADDED Requirements

### Requirement: Evaluate expression
The system SHALL provide a `debugger_evaluate` tool that evaluates an expression in the current debug context and returns its value and type.

#### Scenario: Evaluate simple variable
- **WHEN** debugger is in break mode and variable name is provided
- **THEN** tool returns variable value, type, and whether evaluation succeeded

#### Scenario: Evaluate complex expression
- **WHEN** debugger is in break mode and expression like `obj.Property + 1` is provided
- **THEN** tool returns evaluated result or error if expression is invalid

#### Scenario: Evaluate when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

#### Scenario: Evaluate out-of-scope variable
- **WHEN** variable is not in current scope
- **THEN** tool returns error indicating variable not found

---

### Requirement: Get local variables
The system SHALL provide a `debugger_get_locals` tool that returns all local variables in the current stack frame.

#### Scenario: Get locals in break mode
- **WHEN** debugger is in break mode
- **THEN** tool returns array of local variables with name, value, and type

#### Scenario: Get locals when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Get method arguments
The system SHALL provide a `debugger_get_arguments` tool that returns all arguments of the current method in the active stack frame.

#### Scenario: Get arguments in break mode
- **WHEN** debugger is in break mode inside a method
- **THEN** tool returns array of arguments with name, value, and type

#### Scenario: Get arguments when not in break mode
- **WHEN** debugger is not in break mode
- **THEN** tool returns error indicating not in break mode

---

### Requirement: Inspect variable deeply
The system SHALL provide a `debugger_inspect_variable` tool that returns detailed information about a variable including its members (properties, fields).

#### Scenario: Inspect object variable
- **WHEN** debugger is in break mode and object variable name is provided
- **THEN** tool returns variable value, type, and list of members with their values

#### Scenario: Inspect collection
- **WHEN** debugger is in break mode and collection variable is provided
- **THEN** tool returns collection items (up to a reasonable limit)

#### Scenario: Inspect primitive variable
- **WHEN** debugger is in break mode and primitive variable is provided
- **THEN** tool returns value and type without members

---

### Requirement: Set variable value
The system SHALL provide a `debugger_set_variable` tool that modifies a variable's value during debugging.

#### Scenario: Set variable to new value
- **WHEN** debugger is in break mode, variable exists, and valid new value is provided
- **THEN** variable is updated and tool returns success with new value

#### Scenario: Set variable with incompatible type
- **WHEN** new value type is incompatible with variable type
- **THEN** tool returns error indicating type mismatch

---

### Requirement: Get watch expressions
The system SHALL provide a `debugger_get_watch` tool that returns all watch expressions and their current values.

#### Scenario: Get watch with expressions
- **WHEN** watch expressions have been added
- **THEN** tool returns array of watch items with expression, value, type, and validity

#### Scenario: Get empty watch
- **WHEN** no watch expressions have been added
- **THEN** tool returns empty array

---

### Requirement: Add watch expression
The system SHALL provide a `debugger_add_watch` tool that adds an expression to the watch list.

#### Scenario: Add valid watch expression
- **WHEN** expression is provided
- **THEN** expression is added to watch list and tool returns success

---

### Requirement: Remove watch expression
The system SHALL provide a `debugger_remove_watch` tool that removes an expression from the watch list.

#### Scenario: Remove existing watch
- **WHEN** expression exists in watch list
- **THEN** expression is removed and tool returns success

#### Scenario: Remove non-existent watch
- **WHEN** expression does not exist in watch list
- **THEN** tool returns error indicating not found

---

### Requirement: Clear all watches
The system SHALL provide a `debugger_clear_watch` tool that removes all expressions from the watch list.

#### Scenario: Clear all watches
- **WHEN** tool is called
- **THEN** all watch expressions are removed and tool returns success
