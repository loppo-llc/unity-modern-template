# UnityModernTemplate

[日本語版はこちら](README.ja.md)

A template for building Unity DLLs from an **external .NET project**, enabling modern C# development, fast iteration, and test-driven development outside the Unity Editor.

## Why?

Unity's built-in scripting workflow (asmdef) has limitations:

- **No latest .NET APIs** — Unity's compiler lags behind mainstream .NET
- **Slow iteration** — Every code change triggers a domain reload in the Editor
- **Limited TDD** — Unity Test Framework requires the Editor to run
- **AI agent unfriendly** — AI coding agents can't easily build/test without Unity

This template solves all of these by compiling code in a standard .NET project, then auto-copying the resulting DLLs into Unity.

## Project Structure

```
UnityModernTemplate/
├── unity/                              # Unity 6 (6000.3.7f1) project
│   └── Assets/_Main/Scripts/           # DLLs auto-copied here by post-build
│
├── csharp/                             # .NET solution
│   ├── Directory.Build.props           # Unity path resolution + shared settings
│   ├── Directory.Build.targets         # Post-build copy to Unity
│   ├── src/
│   │   ├── UnityDotNetSample/          # Runtime library → csharp.dll
│   │   │   ├── Components/             #   MonoBehaviour samples
│   │   │   ├── Core/                   #   Pure C# utilities
│   │   │   └── ScriptableObjects/      #   ScriptableObject samples
│   │   │
│   │   └── EditorConsoleLogProxy/      # Editor library → editor-console-log-proxy.dll
│   │                                   #   Proxies Unity console logs to a local file
│   │
│   └── tests/
│       └── UnityDotNetSample.Tests/    # xUnit tests (net9.0)
│
├── .editorconfig                       # Code style rules
└── .gitignore
```

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Unity 6 (6000.3.x)](https://unity.com/releases/editor/whats-new/6000.3.7)

## Getting Started

### Build

```bash
cd csharp
dotnet build
```

This compiles all projects and auto-copies the DLLs to `unity/Assets/_Main/Scripts/`.

### Test

```bash
dotnet test
```

All 28 tests run outside Unity via xUnit — no Editor required.

### Open in Unity

Open `unity/` in Unity Hub. The DLLs are already in place from the build step.

## How DLL Auto-Copy Works

1. Each `.csproj` opts in with `<CopyToUnityPlugins>true</CopyToUnityPlugins>`
2. `Directory.Build.targets` runs a post-build step that copies DLL + PDB to `Assets/_Main/Scripts/`
3. Unity detects the file change and reimports only the changed DLL

This means editing runtime code only rebuilds the runtime DLL, and editing editor code only rebuilds the editor DLL — **minimal domain reload**.

## Unity Path Configuration

The Unity Editor path is resolved in 3 tiers (highest priority first):

1. **Environment variable**: `UNITY_EDITOR_PATH`
2. **Local user file**: `Directory.Build.props.user` (gitignored)
3. **Default**: `C:\Program Files\Unity\Hub\Editor\6000.3.7f1`

## Adding a New Project

1. Create a new folder under `src/` (e.g., `src/MyFeature/`)
2. Create a `.csproj` targeting `netstandard2.1`
3. Set `<CopyToUnityPlugins>true</CopyToUnityPlugins>`
4. Add Unity module references with `<Private>false</Private>`
5. Add the project to `csharp.slnx`
6. Add a `<ProjectReference>` in the test project
7. Add `.gitignore` entries for the new DLL/PDB/meta files

## EditorConsoleLogProxy

A built-in Editor extension that writes Unity console output to `Logs/console.log` in JSON Lines format. This enables AI agents and external tools to read Unity logs without opening the Editor UI.

- Auto-initializes via `[InitializeOnLoad]`
- Thread-safe file writing with `FileShare.Read` for concurrent access
- File is cleared on every domain reload

Log format:
```json
{"t":"2025-02-06T23:45:12.345Z","l":"Log","m":"Hello World"}
{"t":"2025-02-06T23:45:12.346Z","l":"Error","m":"NullRef","s":"at Foo.Bar()"}
```

## Technical Notes

### Avoid CS0433 — Do NOT Reference UnityEngine.dll (Facade)

Reference only the **module DLLs** (`UnityEngine.CoreModule.dll`, etc.), never the facade `UnityEngine.dll`. Referencing both causes `CS0433: type exists in both assemblies` errors.

### Avoid Assets/Plugins/

Do **not** place external DLLs in `Assets/Plugins/`. Unity treats this folder specially and may interpret DLLs as Roslyn analyzers, causing spurious warnings. Use any non-special folder (e.g., `Assets/_Main/Scripts/`).

### ImplicitUsings = disable

This is required to avoid `System.Object` vs `UnityEngine.Object` ambiguity when both namespaces are implicitly imported.

## License

[MIT LICENSE](LICENSE)
