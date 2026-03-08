<p align="center">
  <img src="https://raw.githubusercontent.com/CodingWithCalvin/VS-MCPServer/main/resources/logo.png" alt="VS MCP Server Logo" width="128" height="128">
</p>

<h1 align="center">VS MCP Server</h1>

<p align="center">
  <strong>Let AI assistants like Claude control Visual Studio through the Model Context Protocol!</strong>
</p>

<p align="center">
  <a href="https://github.com/CodingWithCalvin/VS-MCPServer/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/CodingWithCalvin/VS-MCPServer?style=for-the-badge" alt="License">
  </a>
  <a href="https://github.com/CodingWithCalvin/VS-MCPServer/actions/workflows/build.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/CodingWithCalvin/VS-MCPServer/build.yml?style=for-the-badge" alt="Build Status">
  </a>
</p>

<p align="center">
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-MCPServer">
    <img src="https://img.shields.io/visual-studio-marketplace/v/CodingWithCalvin.VS-MCPServer?style=for-the-badge" alt="Marketplace Version">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-MCPServer">
    <img src="https://img.shields.io/visual-studio-marketplace/i/CodingWithCalvin.VS-MCPServer?style=for-the-badge" alt="Marketplace Installations">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-MCPServer">
    <img src="https://img.shields.io/visual-studio-marketplace/d/CodingWithCalvin.VS-MCPServer?style=for-the-badge" alt="Marketplace Downloads">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-MCPServer">
    <img src="https://img.shields.io/visual-studio-marketplace/r/CodingWithCalvin.VS-MCPServer?style=for-the-badge" alt="Marketplace Rating">
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Tools-84-blue?style=for-the-badge" alt="84 Tools">
  <img src="https://img.shields.io/badge/VS-2022%20%7C%202026-purple?style=for-the-badge" alt="Visual Studio 2022/2026">
</p>

---

## 🤔 What is this?

**VS MCP Server** exposes Visual Studio features through the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/), enabling AI assistants like Claude to interact with your IDE programmatically. Open files, read code, build projects, debug applications, and more - all through natural conversation!

## ✨ Features

### 📂 Solution & Project Tools

| Tool | Description |
|------|-------------|
| `solution_info` | Get information about the current solution |
| `solution_open` | Open a solution file |
| `solution_close` | Close the current solution |
| `solution_add_project` | Add an existing project to the solution |
| `solution_remove_project` | Remove a project from the solution |
| `project_list` | List all projects in the solution |
| `project_info` | Get detailed project information |
| `project_add_file` | Add a file to a project |
| `project_remove_file` | Remove a file from a project |
| `project_create_item` | Create a new project item from template |
| `project_add_reference` | Add a project-to-project reference |
| `project_remove_reference` | Remove a project reference |

### 📝 Document Tools

| Tool | Description |
|------|-------------|
| `document_list` | List all open documents |
| `document_active` | Get the active document |
| `document_open` | Open a file in the editor |
| `document_close` | Close a document |
| `document_read` | Read document contents |
| `document_write` | Write to a document |

### ✏️ Editor Tools

| Tool | Description |
|------|-------------|
| `selection_get` | Get the current text selection |
| `selection_set` | Set the selection range |
| `editor_insert` | Insert text at cursor position |
| `editor_replace` | Find and replace text |
| `editor_goto_line` | Navigate to a specific line |
| `editor_find` | Search within documents |

### 🔍 Search & Navigation Tools

| Tool | Description |
|------|-------------|
| `find_in_files` | Search for text across all files in the solution |
| `symbol_document` | Get all symbols defined in a file |
| `symbol_workspace` | Search for symbols across the entire solution |
| `goto_definition` | Navigate to the definition of a symbol |
| `find_references` | Find all references to a symbol |

### 🔨 Build Tools

| Tool | Description |
|------|-------------|
| `build_solution` | Build the entire solution |
| `build_project` | Build a specific project |
| `rebuild_solution` | Rebuild the entire solution (clean + build) |
| `clean_solution` | Clean the solution |
| `build_cancel` | Cancel a running build |
| `build_status` | Get current build status |
| `build_get_errors` | Get all build errors from the Error List |

### 🐛 Debug Control Tools

| Tool | Description |
|------|-------------|
| `debugger_state` | Get the current debugger state |
| `debugger_start` | Start debugging (F5) |
| `debugger_stop` | Stop debugging (Shift+F5) |
| `debugger_continue` | Continue execution (F5) |
| `debugger_step_into` | Step into (F11) |
| `debugger_step_over` | Step over (F10) |
| `debugger_step_out` | Step out (Shift+F11) |
| `debugger_run_to_cursor` | Run to cursor (Ctrl+F10) |

### 🔴 Breakpoint Tools

| Tool | Description |
|------|-------------|
| `breakpoint_list` | List all breakpoints |
| `breakpoint_set` | Set a breakpoint at a location |
| `breakpoint_remove` | Remove a breakpoint |
| `breakpoint_toggle` | Toggle a breakpoint |
| `breakpoint_set_condition` | Set a conditional breakpoint |

### 🧵 Thread & Stack Tools

| Tool | Description |
|------|-------------|
| `debugger_call_stack` | Get the current call stack |
| `debugger_set_frame` | Set the current stack frame |
| `debugger_threads` | Get all threads in the debugged process |
| `debugger_set_thread` | Switch to a different thread |

### 🔬 Inspection Tools

| Tool | Description |
|------|-------------|
| `debugger_evaluate` | Evaluate an expression in the current context |
| `debugger_get_locals` | Get local variables in the current frame |
| `debugger_get_arguments` | Get method arguments in the current frame |
| `debugger_inspect_variable` | Inspect a variable and its children |
| `debugger_set_variable` | Set a variable's value |
| `debugger_get_watch` | Get current watch expressions |
| `debugger_add_watch` | Add a watch expression |
| `debugger_remove_watch` | Remove a watch expression |
| `debugger_clear_watch` | Clear all watch expressions |

### 🔧 Advanced Debug Tools

| Tool | Description |
|------|-------------|
| `debugger_attach` | Attach the debugger to a running process |
| `debugger_get_processes` | Get list of local processes |
| `debugger_get_modules` | Get loaded modules in the debugged process |
| `debugger_read_memory` | Read memory at a specific address |
| `debugger_get_registers` | Get current register values |

### 📤 Output Tools

| Tool | Description |
|------|-------------|
| `output_get_build` | Get the Build Output window contents |
| `output_get_debug` | Get the Debug Output window contents |
| `output_write` | Write a message to an Output window pane |

### 🩺 Diagnostics Tools

| Tool | Description |
|------|-------------|
| `diagnostics_get` | Get Roslyn diagnostics for a file or solution |
| `error_list_get` | Get the Error List window contents |
| `diagnostics_binding_errors` | Get XAML binding errors |
| `code_fix_apply` | Apply a suggested code fix |

### 🧪 Test Tools

| Tool | Description |
|------|-------------|
| `test_discover` | Discover tests in the solution |
| `test_run_all` | Run all tests |
| `test_run_specific` | Run specific tests |
| `test_debug` | Debug a specific test |
| `test_results` | Get test results |

### 🔄 Refactor Tools

| Tool | Description |
|------|-------------|
| `refactor_rename` | Rename a symbol across the solution |
| `refactor_extract_method` | Extract selected code to a new method |
| `refactor_organize_usings` | Organize using statements |

### 📦 NuGet Tools

| Tool | Description |
|------|-------------|
| `nuget_list` | List NuGet packages in a project |
| `nuget_search` | Search for NuGet packages |
| `nuget_install` | Install a NuGet package |
| `nuget_update` | Update a NuGet package |
| `nuget_uninstall` | Uninstall a NuGet package |

### ⚙️ General Tools

| Tool | Description |
|------|-------------|
| `execute_command` | Execute a Visual Studio command |
| `get_ide_status` | Get the current IDE status |

## 🛠️ Installation

### Visual Studio Marketplace

1. Open Visual Studio 2022 or 2026
2. Go to **Extensions > Manage Extensions**
3. Search for "MCP Server"
4. Click **Download** and restart Visual Studio

### Manual Installation

Download the latest `.vsix` from the [Releases](https://github.com/CodingWithCalvin/VS-MCPServer/releases) page and double-click to install.

## 🚀 Usage

### ▶️ Starting the Server

1. Open Visual Studio
2. Go to **Tools > MCP Server > Start Server** (or enable auto-start in settings)
3. The MCP server starts on `http://localhost:5050`

### 🤖 Configuring Claude Desktop

Add this to your Claude Desktop MCP settings:

```json
{
  "mcpServers": {
    "visual-studio": {
      "url": "http://localhost:5050/sse"
    }
  }
}
```

### ⚙️ Settings

Configure the extension at **Tools > Options > MCP Server**:

| Setting | Description | Default |
|---------|-------------|---------|
| Auto-start server | Start the MCP server when Visual Studio launches | Off |
| Binding Address | Address the server binds to | `localhost` |
| HTTP Port | Port for the MCP server | `5050` |
| Server Name | Name reported to MCP clients | `Visual Studio MCP` |
| Log Level | Minimum log level for output | `Information` |
| Log Retention | Days to keep log files | `7` |

## 🏗️ Architecture

```
+------------------+              +----------------------+   named pipes   +------------------+
|  Claude Desktop  |   HTTP/SSE  |  MCPServer.Server    | <-------------> |  VS Extension    |
|  (MCP Client)    | <---------> |  (MCP Server)        |    JSON-RPC     |  (Tool Impl)     |
+------------------+    :5050    +----------------------+                 +------------------+
```

## 🤝 Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or pull requests - all feedback helps make this extension better.

### 🔧 Development Setup

1. Clone the repository
2. Open `src/CodingWithCalvin.MCPServer.slnx` in Visual Studio 2022
3. Ensure you have the "Visual Studio extension development" workload installed
4. Ensure you have .NET 10.0 SDK installed
5. Press F5 to launch the experimental instance

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Contributors

<!-- readme: contributors -start -->
<a href="https://github.com/CalvinAllen"><img src="https://avatars.githubusercontent.com/u/41448698?v=4&s=64" width="64" height="64" alt="CalvinAllen"></a> 
<!-- readme: contributors -end -->

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/CodingWithCalvin">Coding With Calvin</a>
</p>
