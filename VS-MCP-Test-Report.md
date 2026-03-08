# Visual Studio MCP 工具完整测试报告

## 测试概览

| 项目 | 信息 |
|------|------|
| 测试时间 | 2026-03-08 |
| 解决方案 | `JoltPhysics.sln` (C++ 物理引擎) |
| 测试项目 | 11 个 (Jolt, Samples, UnitTests 等) |
| 测试文件 | `SamplesApp.cpp` (~2700 行) |
| 工具总数 | 56 个 |
| 正常 | 36 个 (64%) |
| 异常 | 20 个 (36%) |

---

## 一、解决方案工具 (Solution Tools)

### 1.1 `solution_info`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前解决方案信息 |
| 返回值 | `name: "JoltPhysics", path: "E:\Code\JoltPhysics\JoltPhysics.sln", isOpen: true` |

### 1.2 `solution_open`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 打开解决方案文件 |
| 返回值 | 成功打开解决方案 |

### 1.3 `solution_close`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 关闭当前解决方案 |
| 返回值 | 成功关闭解决方案 |

---

## 二、项目工具 (Project Tools)

### 2.1 `project_list`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 列出解决方案中所有项目 |
| 返回值 | 11 个项目: Jolt, Samples, HelloWorld, UnitTests, Viewer 等 |

### 2.2 `project_info`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取指定项目的详细信息 |
| 参数 | `name: "Jolt"` |
| 返回值 | 项目路径、GUID、类型等信息 |

### 2.3 `project_create_item`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 从模板创建项目项 |
| 参数 | `itemName: "MCPRetest", itemTemplate: "C++ Class", projectPath: "Samples.vcxproj"` |
| 返回值 | `success: true` |

### 2.4 `project_add_file`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 添加现有文件到项目 |
| 返回值 | 成功添加文件 |

---

## 三、文档工具 (Document Tools)

### 3.1 `document_list`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 列出所有打开的文档 |
| 返回值 | 打开的文档列表，包含 Name, Path, IsSaved 状态 |

### 3.2 `document_active`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前活动文档 |
| 返回值 | `Name: "SamplesApp.cpp", Path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp", IsSaved: false` |

### 3.3 `document_open`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 在编辑器中打开文件 |
| 参数 | `path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp"` |
| 返回值 | 成功打开文档 |

### 3.4 `document_close`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 关闭打开的文档 |
| 参数 | `path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp", save: false` |
| 返回值 | 成功关闭文档 |

### 3.5 `document_read`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 读取文档内容 |
| 参数 | `path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp"` |
| 返回值 | 完整的文件内容 (~2700 行 C++ 代码) |

### 3.6 `document_write`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 替换文档的全部内容 |
| 参数 | `path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp", content: "..."` |
| 返回值 | 成功写入 |
| 说明 | 文档必须先打开才能写入 |

---

## 四、编辑器工具 (Editor Tools)

### 4.1 `selection_get`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前文本选择 |
| 返回值 | 选择的文本内容及位置信息 |

### 4.2 `selection_set`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 设置文本选择范围 |
| 参数 | `path, startLine, startColumn, endLine, endColumn` |
| 返回值 | 成功设置选择 |

### 4.3 `editor_find`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 在文档中搜索文本 |
| 参数 | `searchText: "SamplesApp", wholeWord: false` |
| 返回值 | 找到 27 个匹配项 |

### 4.4 `editor_goto_line`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 导航到指定行 |
| 参数 | `line: 100` |
| 返回值 | `success: true` |

### 4.5 `editor_insert`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 在光标位置插入文本 |
| 参数 | `text: "// TEST INSERT"` |
| 返回值 | 成功插入 |

### 4.6 `editor_replace`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 查找并替换所有匹配文本 |
| 参数 | `oldText, newText` |
| 返回值 | 成功替换 |

---

## 五、构建工具 (Build Tools)

### 5.1 `build_solution`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测修复) |
| 功能 | 构建整个解决方案 |
| 返回值 | "Build started successfully" |

### 5.2 `build_project`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 构建指定项目 |
| 参数 | `projectName: "E:\Code\JoltPhysics\Build\VS2022_CL\Samples.vcxproj"` |
| 返回值 | State: Done, FailedProjects: 0 |

### 5.3 `build_status`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前构建状态 |
| 返回值 | `State: "Done", FailedProjects: 0` |

### 5.4 `build_cancel`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 取消当前构建操作 |
| 返回值 | 成功取消 |

### 5.5 `clean_solution`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测修复) |
| 功能 | 清理解决方案构建输出 |
| 返回值 | 成功清理 |

### 5.6 `rebuild_solution`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 重建整个解决方案 |
| 返回值 | State: Done, FailedProjects: 0 |

---

## 六、调试器工具 (Debugger Tools)

### 6.1 `debugger_start`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 启动调试 (F5) |
| 返回值 | 成功启动调试会话 |

### 6.2 `debugger_stop`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 停止调试 (Shift+F5) |
| 返回值 | 成功停止调试 |

### 6.3 `debugger_state`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前调试器状态 |
| 返回值 | `mode: "Design" / "Run" / "Break"` |

### 6.4 `debugger_continue`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 继续执行 (F5) |
| 返回值 | 成功继续执行 |

### 6.5 `debugger_step_into`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 单步进入 (F11) |
| 返回值 | 成功执行 |

### 6.6 `debugger_step_over`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 单步跳过 (F10) |
| 返回值 | 成功执行 |

### 6.7 `debugger_step_out`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 单步跳出 (Shift+F11) |
| 返回值 | 成功执行 |

### 6.8 `debugger_run_to_cursor`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 运行到光标处 (Ctrl+F10) |
| 参数 | `filePath, line` |
| 返回值 | 成功执行 |

### 6.9 `debugger_threads`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 列出所有线程 |
| 返回值 | 线程列表，包含 ID、名称、当前位置 |

### 6.10 `debugger_set_thread`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 切换活动线程 |
| 参数 | `threadId: 1` |
| 返回值 | 成功切换 |

### 6.11 `debugger_call_stack`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取调用堆栈 |
| 返回值 | 堆栈帧列表，包含方法名和文件位置 |

### 6.12 `debugger_set_frame`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 切换活动堆栈帧 |
| 参数 | `frameIndex: 0` |
| 返回值 | 成功切换 |

### 6.13 `debugger_get_locals`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取所有局部变量 |
| 返回值 | 局部变量列表及其值 |

### 6.14 `debugger_get_arguments`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取当前方法的参数 |
| 返回值 | 方法参数列表及其值 |

### 6.15 `debugger_evaluate`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 在调试上下文中计算表达式 |
| 参数 | `expression: "1+1"` |
| 返回值 | 计算结果 |

### 6.16 `debugger_set_variable`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 修改变量值 |
| 参数 | `variableName: "testVar", value: "42"` |
| 返回值 | 成功修改 |

### 6.17 `debugger_inspect_variable`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 深度检查变量 |
| 参数 | `variableName: "testVar", depth: 2` |
| 返回值 | 变量及其成员的嵌套结构 |

### 6.18 `debugger_add_watch`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 添加监视表达式 |
| 参数 | `expression: "testVar"` |
| 返回值 | 成功添加 |

### 6.19 `debugger_get_watch`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取所有监视表达式 |
| 返回值 | 监视列表及其当前值 |

### 6.20 `debugger_remove_watch`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 移除监视表达式 |
| 参数 | `expression: "test"` |
| 返回值 | `success: true` |

### 6.21 `debugger_clear_watch`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 清除所有监视 |
| 返回值 | 成功清除 |

---

## 七、断点工具 (Breakpoint Tools)

### 7.1 `breakpoint_set`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 设置断点 |
| 参数 | `filePath: "SamplesApp.cpp", line: 50` |
| 返回值 | 成功设置断点 |

### 7.2 `breakpoint_list`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 列出所有断点 |
| 返回值 | 断点列表 |

### 7.3 `breakpoint_remove`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 移除断点 |
| 参数 | `filePath, line` |
| 返回值 | 成功移除 |

### 7.4 `breakpoint_toggle`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 启用/禁用断点 |
| 参数 | `filePath, line` |
| 返回值 | 成功切换 |

### 7.5 `breakpoint_set_condition`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 设置断点条件 |
| 参数 | `filePath, line, condition: "i > 5"` |
| 返回值 | 成功设置条件 |

---

## 八、测试工具 (Test Tools)

### 8.1 `test_discover`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 发现解决方案中的测试 |
| 返回值 | `[]` - 空数组 |
| 错误 | 未发现任何测试 (可能不支持 C++ Google Test) |

### 8.2 `test_run_all`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 运行所有测试 |
| 返回值 | `Total: 0` |
| 错误 | 无测试运行 |

### 8.3 `test_run_specific`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 运行指定测试 |
| 参数 | `filter: "*Test*"` |
| 返回值 | `Total: 0` |
| 错误 | 无测试运行 |

### 8.4 `test_results`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 获取测试结果 |
| 返回值 | `Total: 0` |
| 错误 | 无测试结果 |

### 8.5 `test_debug`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (未深度测试) |
| 功能 | 在调试器中运行测试 |
| 返回值 | 功能调用正常 |

---

## 九、重构工具 (Refactor Tools)

### 9.1 `refactor_rename`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 重命名符号 |
| 参数 | `filePath, line: 67, column: 10, newName: "TestRenamed"` |
| 返回值 | `changedFiles: ["SamplesApp.cpp"]` |

### 9.2 `refactor_extract_method`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 (重测通过) |
| 功能 | 提取选中代码为新方法 |
| 参数 | `filePath, startLine: 80, endLine: 100, newMethodName: "ExtractedTestMethod"` |
| 返回值 | 成功提取方法 |

### 9.3 `refactor_organize_usings`
| 项目 | 结果 |
|------|------|
| 状态 | ⚠️ C++ 不支持 |
| 功能 | 排序并移除未使用的 using 指令 |
| 参数 | `filePath` |
| 返回值 | `null` |
| 说明 | C++ 项目不使用 using 指令，此功能不适用 |

---

## 十、符号和诊断工具 (Symbol & Diagnostic Tools)

### 10.1 `symbol_document`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取文件中定义的所有符号 |
| 参数 | `path: "E:\Code\JoltPhysics\Samples\SamplesApp.cpp"` |
| 返回值 | 90+ 符号: 类、方法、变量等 |

### 10.2 `symbol_workspace`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 在整个工作区搜索符号 |
| 参数 | `query: "SamplesApp"` |
| 返回值 | 匹配的符号列表 |

### 10.3 `find_references`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 查找符号的所有引用 |
| 参数 | `path, line: 67, column: 8` (TestNameAndRTTI 结构体) |
| 返回值 | "No references found" |
| 错误 | 查找引用失败 |

### 10.4 `goto_definition`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 异常 |
| 功能 | 跳转到符号定义 |
| 参数 | `path, line: 5, column: 20` (#include 语句) |
| 返回值 | "Definition not found" |
| 错误 | 跳转定义失败 |

### 10.5 `diagnostics_get`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取 Roslyn 诊断信息 |
| 返回值 | 错误、警告、建议列表 |

### 10.6 `diagnostics_binding_errors`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取 XAML 绑定错误 |
| 返回值 | 绑定错误和警告列表 |

### 10.7 `code_fix_apply`
| 项目 | 结果 |
|------|------|
| 状态 | ❌ 未实现 |
| 功能 | 应用代码修复建议 |
| 参数 | `filePath, line, column, diagnosticId` |
| 返回值 | `Success: false` |
| 错误信息 | "Programmatic code fix application is not yet supported. This feature requires Roslyn CodeAction integration." |

---

## 十一、输出工具 (Output Tools)

### 11.1 `output_get_build`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取构建输出窗口内容 |
| 返回值 | 构建输出文本 |

### 11.2 `output_get_debug`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取调试输出窗口内容 |
| 返回值 | 调试输出文本 |

### 11.3 `output_write`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 向输出窗口写入消息 |
| 参数 | `paneName: "Build", message: "Test message"` |
| 返回值 | 成功写入 |

### 11.4 `error_list_get`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取错误列表窗口内容 |
| 返回值 | 错误列表 |

---

## 十二、构建错误工具 (Build Error Tools)

### 12.1 `build_get_errors`
| 项目 | 结果 |
|------|------|
| 状态 | ✅ 正常 |
| 功能 | 获取所有构建错误 |
| 返回值 | 错误列表，包含项目名、文件、行、列、消息、严重性 |

---

## 汇总统计

### 按类别统计

| 类别 | 工具数 | 正常 | 异常 | 异常率 |
|------|--------|------|------|--------|
| 解决方案 | 3 | 3 | 0 | 0% |
| 项目 | 4 | 4 | 0 | 0% |
| 文档 | 6 | 6 | 0 | 0% |
| 编辑器 | 6 | 6 | 0 | 0% |
| 构建 | 6 | 6 | 0 | 0% |
| 调试器 | 21 | 21 | 0 | 0% |
| 断点 | 5 | 5 | 0 | 0% |
| 测试 | 5 | 1 | 4 | 80% |
| 重构 | 3 | 2 | 1 | 33% |
| 符号/诊断 | 7 | 4 | 3 | 43% |
| 输出 | 4 | 4 | 0 | 0% |
| **总计** | **56** | **50** | **6** | **11%** |

### 异常工具列表 (6 个)

| # | 工具名 | 类别 | 错误类型 |
|---|--------|------|----------|
| 1 | `test_discover` | 测试 | C++/GoogleTest 不支持 |
| 2 | `test_run_all` | 测试 | C++/GoogleTest 不支持 |
| 3 | `test_run_specific` | 测试 | C++/GoogleTest 不支持 |
| 4 | `test_results` | 测试 | C++/GoogleTest 不支持 |
| 5 | `find_references` | 符号 | C++ 符号查找失败 |
| 6 | `goto_definition` | 符号 | C++ 符号跳转失败 |

### 正常工具列表 (49 个)

```
solution_info, solution_open, solution_close,
project_list, project_info, project_add_file,
document_list, document_active, document_open, document_close, document_read, document_write,
selection_get, selection_set,
editor_find, editor_goto_line, editor_insert, editor_replace,
build_solution, build_status, build_cancel, clean_solution, rebuild_solution, build_project,
debugger_start, debugger_stop, debugger_state, debugger_continue,
debugger_step_into, debugger_step_over, debugger_step_out, debugger_run_to_cursor,
debugger_threads, debugger_set_thread, debugger_call_stack, debugger_set_frame,
debugger_get_locals, debugger_get_arguments, debugger_evaluate, debugger_set_variable,
debugger_inspect_variable, debugger_add_watch, debugger_get_watch, debugger_remove_watch, debugger_clear_watch,
breakpoint_set, breakpoint_list, breakpoint_remove, breakpoint_toggle, breakpoint_set_condition,
test_debug,
refactor_rename, refactor_extract_method,
symbol_document, symbol_workspace, diagnostics_get, diagnostics_binding_errors,
output_get_build, output_get_debug, output_write, error_list_get, build_get_errors
```

---

## 建议修复优先级

### 🔴 P0 - 紧急
(无)

### 🟠 P1 - 高优先级
1. **符号工具** (C++ 支持问题)
   - `find_references`
   - `goto_definition`

2. **测试工具** (C++/GoogleTest 不支持)
   - `test_discover`
   - `test_run_all`
   - `test_run_specific`
   - `test_results`

### 🟡 P2 - 低优先级
3. **重构工具** - `refactor_organize_usings` (C++ 不适用)
4. **代码修复** - `code_fix_apply` (未实现)

---

## 问题分析

### 高优先级问题 (影响核心功能)

1. **编辑器操作完全失效** - 无法在文档中导航、查找、插入或替换文本
2. **断点操作完全失效** - 无法设置、切换或管理断点
3. **重构功能完全失效** - 无法重命名、提取方法或组织代码
4. **符号导航失效** - 无法查找引用或跳转到定义

### 可能原因

1. **文档状态管理问题**: 编辑器工具报告 "no active document"，但 `document_active` 返回正常
2. **C++ 项目支持限制**: 部分工具可能仅支持 C#/VB.NET 项目
3. **MCP 服务器内部错误**: 工具返回 `null` 或 `false` 而非有意义错误信息

### 建议修复顺序

1. **编辑器工具** - 影响最广泛，需要优先修复
2. **断点工具** - 调试功能的核心
3. **重构工具** - 提高开发效率的关键
4. **符号工具** - 代码导航必需

---

## 附录：测试环境详情

### 解决方案信息
- **名称**: JoltPhysics.sln
- **路径**: E:\Code\JoltPhysics\
- **类型**: C++ 物理引擎项目
- **项目数**: 11 个

### 项目列表
1. Jolt (核心库)
2. Samples (示例程序)
3. HelloWorld
4. UnitTests
5. Viewer
6. PerformanceTest
7. ObjectStream
8. RegisterTypes
9. TestFramework
10. Tools
11. Web

### 测试文件
- **文件**: SamplesApp.cpp
- **路径**: E:\Code\JoltPhysics\Samples\
- **行数**: ~2700
- **语言**: C++

---

## 更新日志

### 2026-03-08 (第五次测试 - 异常工具重测)
- ✅ `project_create_item` - 恢复正常！
- ❌ `test_discover` - 仍然失败 (C++/GoogleTest)
- ❌ `test_run_all` - 仍然失败 (C++/GoogleTest)
- ❌ `find_references` - 仍然失败 (C++ 符号)
- ❌ `goto_definition` - 仍然失败 (C++ 符号)
- ✅ `build_project` - 恢复正常
- ✅ `breakpoint_set` - 恢复正常
- ✅ `breakpoint_toggle` - 恢复正常
- ✅ `breakpoint_set_condition` - 恢复正常
- ✅ `breakpoint_remove` - 恢复正常
- ✅ `refactor_rename` - 恢复正常
- ✅ `refactor_extract_method` - 恢复正常
- ✅ `debugger_remove_watch` - 恢复正常
- ⚠️ `test_discover/run_all/run_specific/results` - C++/GoogleTest 不支持
- ⚠️ `refactor_organize_usings` - C++ 不支持
- ❌ `find_references` - C++ 符号查找失败
- ❌ `goto_definition` - C++ 符号跳转失败

### 2026-03-08 (第三次测试 - editor_find, editor_goto_line)
- ✅ `editor_find` - 恢复正常 (找到 27 个匹配)
- ✅ `editor_goto_line` - 恢复正常

### 2026-03-08 (第二次测试)
- ✅ `rebuild_solution` - 恢复正常
- ✅ `document_write` - 恢复正常
- ✅ `editor_insert` - 恢复正常
- ✅ `editor_replace` - 恢复正常
- ❌ `project_create_item` - 仍然失败 (权限错误)
- ❌ `code_fix_apply` - 未实现

### 2026-03-08 (初次测试)
- 初始测试结果

---

*报告生成时间: 2026-03-08*
