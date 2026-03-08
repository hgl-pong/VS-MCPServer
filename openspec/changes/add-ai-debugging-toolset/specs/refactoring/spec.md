## ADDED Requirements

### Requirement: Rename symbol
The system SHALL provide a `refactor_rename` tool that renames a symbol (variable, method, class, etc.) across the entire solution.

#### Scenario: Rename local variable
- **WHEN** file, line, column of variable and new name are provided
- **THEN** variable is renamed in current scope and tool returns list of changed files

#### Scenario: Rename method
- **WHEN** file, line, column of method and new name are provided
- **THEN** method is renamed across all references and tool returns list of changed files

#### Scenario: Rename class
- **WHEN** file, line, column of class and new name are provided
- **THEN** class is renamed including file name if matching, and tool returns list of changed files

#### Scenario: Rename with conflicts
- **WHEN** new name conflicts with existing symbol
- **THEN** tool returns error indicating conflict

---

### Requirement: Extract method
The system SHALL provide a `refactor_extract_method` tool that extracts selected code into a new method.

#### Scenario: Extract valid selection to method
- **WHEN** file and valid selection range are provided
- **THEN** selected code is moved to new method, original code replaced with method call, and tool returns new method name and modified content

#### Scenario: Extract invalid selection
- **WHEN** selection is not valid for extraction (e.g., partial statement)
- **THEN** tool returns error indicating invalid selection

---

### Requirement: Organize usings
The system SHALL provide a `refactor_organize_usings` tool that sorts and removes unused using directives.

#### Scenario: Organize usings in file
- **WHEN** file path is provided
- **THEN** usings are sorted alphabetically, unused usings are removed, and tool returns modified content

#### Scenario: Organize usings with preservation option
- **WHEN** file path and preserve option (keep system usings first) are provided
- **THEN** usings are sorted with System.* usings placed first
