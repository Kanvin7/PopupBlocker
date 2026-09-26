# lib 目录说明

本目录下的 `Interop.TaskScheduler.dll` 是任务计划程序 COM 类型库（`taskschd.dll`）的互操作程序集，
由 Windows SDK 的 `tlbimp` 生成，供 `PopupBlocker.Core` 操作开机自启任务使用。

## 为什么放进仓库

.NET SDK 自带的 MSBuild 不支持 `COMReference`（会报 MSB4803），
所以命令行编译时需要这个已生成好的互操作程序集：

\`\`\`
dotnet build -p:SkipComReference=true
\`\`\`

在 Visual Studio 中不要传这个开关，工程仍走 `COMReference` 正常生成。

## 如何重新生成

需要 Visual Studio 或 Windows SDK：

\`\`\`
tlbimp %windir%\System32\taskschd.dll /out:Interop.TaskScheduler.dll /namespace:TaskScheduler
\`\`\`

