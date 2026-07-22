# AGENTS.md

Guidance for agents working in this repository.

## Project overview

PSCommander is a Windows desktop automation tool distributed as a PowerShell module. It hosts a WPF tray application that loads a user `config.ps1`, turns emitted PowerShell objects into configuration, and wires those objects into Windows integration services such as schedules, shortcuts, file associations, Explorer context menus, custom protocols, hot keys, desktop widgets, data sources, events, and tray menus.

The solution has two projects:

- `pscommander\pscommander.csproj`: .NET 6 Windows WPF executable and bundled `PSCommander.psm1` / `PSCommander.psd1` module files.
- `pscommander.models\pscommander.models.csproj`: `netstandard2.0` shared model types returned from the PowerShell module and consumed by the WPF app.

## Important files and directories

- `PSCommander.slnx`: solution entry point.
- `pscommander\PSCommander.psm1`: public PowerShell cmdlet functions such as `New-CommanderHotKey`, `New-CommanderSchedule`, `Start-Commander`, and `Register-CommanderDataSource`.
- `pscommander\PSCommander.psd1`: PowerShell module manifest.
- `pscommander\App.xaml.cs`: command-line argument parsing. Secondary invocations forward file association, shortcut, context menu, and protocol commands over named pipes.
- `pscommander\MainWindow.xaml.cs`: composition root. Instantiates services, initializes the PowerShell runspace, loads config, starts event providers, and performs update checks.
- `pscommander\Services\`: Windows integration, PowerShell runspace, persistence, tray menu, widgets, jobs, named pipes, and configuration reload services.
- `pscommander.models\`: public model classes and enums used by both PowerShell functions and app services.
- `.github\workflows\ci.yml`: CI publishes the solution on `windows-latest` with .NET 6.
- `.github\workflows\production.yml`: manual release workflow publishes the module.

## Build and validation

Run commands from the repository root unless noted.

```powershell
dotnet restore .\PSCommander.slnx
dotnet publish
```

The CI build only runs `dotnet publish`, so that is the best repository-level validation command. There is currently no test project or configured lint command.

For targeted project builds:

```powershell
dotnet publish .\pscommander\pscommander.csproj
dotnet build .\pscommander.models\pscommander.models.csproj
```

This is a Windows-only WPF app (`net10.0-windows`, `UseWPF`, `UseWindowsForms`). Do not assume Linux/macOS validation will work.

## Runtime behavior to preserve

- The default config file is `%USERPROFILE%\Documents\PSCommander\config.ps1`, unless `--configFilePath` is supplied.
- `ConfigService` watches the config directory and reloads on file create/change.
- `PowerShellService` imports `PSCommander.psd1` into a single default runspace and injects service variables such as `DataService`, `MenuService`, `HotKeyService`, and `JobService`.
- `ConfigService.LoadAsync` executes the config script and selects objects by model type. Adding a new config feature usually requires:
  1. a model in `pscommander.models`,
  2. a constructor/cmdlet function in `PSCommander.psm1`,
  3. a service or service update path in `pscommander\Services`,
  4. composition in `MainWindow.xaml.cs`,
  5. collection/default handling in `Configuration` and `ConfigService`.
- Secondary executable launches use `NamedPipeClient` to send commands to the running instance and then exit.
- Persistent registrations and lookup data use LiteDB under `%APPDATA%\PSCommander\database.db`.

## Coding conventions

- Keep namespaces as `pscommander`.
- Match existing C# style: four-space indentation, braces on new lines for types/methods, `var` where the code already uses it, and private service fields prefixed with `_` in newer service code.
- PowerShell functions in `PSCommander.psm1` use approved verb-style names with a `Commander` noun prefix and return model objects that the config loader consumes.
- Public cmdlet additions should include comment-based help consistent with surrounding functions.
- Avoid changing generated or vendored-looking collection helpers in `pscommander.models` unless the task specifically concerns them.

## Windows integration cautions

- Many services touch user-level Windows state: registry run keys, file associations, Explorer context menus, shortcuts, global hot keys, named pipes, WMI events, and desktop windows.
- Prefer dry validation through builds and code inspection unless a task explicitly requires live integration testing.
- If live testing is necessary, use an isolated config path with `--configFilePath` and avoid modifying the default user config or persistent Windows registrations unintentionally.
- Be careful with actions that start or stop `PSCommander.exe`; `Stop-Commander` stops running PSCommander processes.

## Documentation

Update `README.md` when adding or changing public PowerShell cmdlets, user-facing configuration syntax, or visible feature behavior. Keep examples PowerShell-focused because the project is distributed as a module.
