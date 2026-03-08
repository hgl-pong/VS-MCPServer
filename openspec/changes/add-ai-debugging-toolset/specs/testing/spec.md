## ADDED Requirements

### Requirement: Discover tests
The system SHALL provide a `test_discover` tool that finds all tests in the solution.

#### Scenario: Discover all tests
- **WHEN** tool is called
- **THEN** tool returns array of tests with test ID, name, type (unit/integration/etc), location (file, line), and parent class

#### Scenario: Discover tests for specific project
- **WHEN** project name is provided
- **THEN** tool returns tests only from that project

---

### Requirement: Run all tests
The system SHALL provide a `test_run_all` tool that executes all tests in the solution.

#### Scenario: Run all tests
- **WHEN** tool is called
- **THEN** all tests are executed and tool returns summary with passed, failed, and skipped counts

#### Scenario: Run tests for specific project
- **WHEN** project name is provided
- **THEN** only tests in that project are executed

---

### Requirement: Run specific tests
The system SHALL provide a `test_run_specific` tool that executes specified test(s).

#### Scenario: Run single test
- **WHEN** single test ID or name is provided
- **THEN** that test is executed and result is returned

#### Scenario: Run multiple tests
- **WHEN** array of test IDs or names is provided
- **THEN** specified tests are executed and results are returned

#### Scenario: Run test by pattern
- **WHEN** pattern/filter is provided (e.g., "ClassName.TestMethod*")
- **THEN** matching tests are executed

---

### Requirement: Debug test
The system SHALL provide a `test_debug` tool that runs a test under the debugger.

#### Scenario: Debug single test
- **WHEN** test ID or name is provided
- **THEN** test runs under debugger, hitting any breakpoints

#### Scenario: Debug with existing breakpoints
- **WHEN** breakpoints are set in test code
- **THEN** execution pauses at breakpoints

---

### Requirement: Get test results
The system SHALL provide a `test_results` tool that returns the results of the last test run.

#### Scenario: Get results after test run
- **WHEN** tests have been run
- **THEN** tool returns detailed results including pass/fail status, duration, error messages for failures

#### Scenario: Get results for specific test
- **WHEN** test ID is provided
- **THEN** tool returns result for that specific test

#### Scenario: Get results when no tests run
- **WHEN** no tests have been run
- **THEN** tool returns empty or indicates no results available
