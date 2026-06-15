---
name: generalupdate-ui
description: |
  Generate a complete update UI window for ANY .NET UI framework — no UI coding required.
  Automatically detects WPF (LayUI.Wpf, WPFDevelopers, native), WinForms (AntdUI, native),
  Avalonia (SemiUrsa), MAUI, or console apps. Generates fully wired update windows with
  REAL GeneralUpdate.Core event bindings that cover ALL states: checking, downloading,
  error/retry, paused, completed, upgrade-in-progress, already-latest, forced-update,
  rollback. Generates IDownloadService bridge to replace MockDownloadService.
  Triggers on: "update UI", "progress bar", "update window", "show progress",
  "update UI", "show progress", "update window", "beautiful UI", "UI style",
  "how to show update progress", "need a progress UI", "update form",
  "beautiful update UI", "professional update appearance".
  ALWAYS load this skill when the user asks for auto-update + UI together.
  Pairs with generalupdate-init for complete integration.
  Pairs with generalupdate-troubleshoot if UI states show wrong values.
when_to_use: |
  - User wants a visual update progress interface (any framework)
  - User asks about showing download progress, speed, remaining time
  - User mentions their UI framework (WPF/WinForms/Avalonia/MAUI) in context of updates
  - User wants a "beautiful" or "professional looking" update experience
  - User currently uses MockDownloadService and wants real update binding
  - User wants pre-built ViewModels, Windows, Styles for update flows
  - User already has basic update integration working and wants a UI for it
allowed-tools: "Read, Write, Edit, Glob, Grep"
---

# GeneralUpdate Update UI Generation — Full State Coverage

Automatically detects the developer's UI framework type and generates a complete update window with real GeneralUpdate.Core event bindings.
Covers all UI states, error handling, animations, and MVVM bindings.

---

## UI State Machine (all templates cover the following states)

```
                   ┌─────────────┐
                   │    Idle     │ ← Initial state
                   └──────┬──────┘
                          │ Auto/manual trigger
                          ▼
                   ┌─────────────┐
            ┌─────│  Checking    │ ← "Checking for updates..." indeterminate animation
            │     └──────┬──────┘
            │            │
            │     ┌──────┴──────┐
            │     ▼             ▼
            │  ┌────────┐  ┌──────────┐
            │  │ Latest │  │  Found!  │ ← Shows version/size/release notes
            │  │(Latest)│  └────┬─────┘
            │  └────────┘       │ User clicks "Start Update"
            │                   ▼
            │            ┌──────────────┐
            │      ┌─────│ Downloading  │ ← Progress bar/speed/remaining time/animation
            │      │     └──────┬───────┘
            │      │            │
            │      │     ┌──────┴──────┐
            │      │     ▼             ▼
            │      │  ┌────────┐  ┌──────────┐
            │      │  │ Paused │  │  Error   │ ← Shows error message + "Retry" button
            │      │  └───┬────┘  └────┬─────┘
            │      │      │ Resume     │ Retry
            │      │      ▼             ▼
            │      │  ┌──────────────┐
            │      │  │ Downloading  │ ← Back to download state
            │      │  └──────────────┘
            │      │
            │      │     ┌──────────────┐
            │      └────→│  Applying    │ ← "Installing update..." (Upgrade process)
            │             └──────┬───────┘
            │                    │
            │             ┌──────┴──────┐
            │             ▼             ▼
            │       ┌─────────┐  ┌──────────┐
            │       │ Success │  │  Failed  │ ← Shows failure reason + rollback hint
            │       └────┬────┘  └────┬─────┘
            │            │            │
            │            ▼            ▼
            │       ┌──────────┐  ┌──────────┐
            │       │ Restart  │  │ Rollback │ ← "Rolling back to previous version"
            │       │(Restart app)│  └──────────┘
            │       └──────────┘
            │
            └── Back to Idle (when no update needed)
```

---

## Workflow

```
1. Framework Detection
   ├── Scan .csproj → PackageReference to identify UI library
   │   ├── Semi.Avalonia / Ursa → Avalonia + SemiUrsa
   │   ├── LayUI.Wpf → WPF + LayUI
   │   ├── WPFDevelopers → WPF + WPFDevelopers
   │   ├── AntdUI → WinForms + AntdUI
   │   ├── Microsoft.Maui → MAUI
   │   └── None → Detect .xaml / .Designer.cs → Native WPF/WinForms
   ├── If unrecognized → Ask user which framework they use
   └── If no UI framework → Console progress bar

2. Status Code Generation
   ├── IDownloadService enhanced interface (covers all states)
   ├── RealDownloadService bridge code (GeneralUpdate.Core → IDownloadService)
   ├── ViewModel (MVVM) or Code-Behind
   └── Window/Page XAML (framework-specific)

3. Integration Guide
   ├── How to replace MockDownloadService → RealDownloadService
   ├── DI Registration (or direct instantiation)
   └── Bootstrap configuration (works with generalupdate-init)
```

---

## Core Bridge: RealDownloadService

All UI templates share this bridge class, mapping all GeneralUpdate.Core events to the `IDownloadService` interface.

### Enhanced IDownloadService Interface (Full State Coverage)

```csharp
public enum DownloadStatus
{
    Idle,               // Initial state, no operation
    Checking,           // Checking server for updates
    FoundUpdate,        // New version found (waiting for user confirmation)
    AlreadyLatest,      // Already on the latest version
    Downloading,        // Downloading update package
    Paused,             // Download paused
    DownloadError,      // Download error, can retry
    Applying,           // Applying update (extracting/patching)
    UpgradeProgress,    // Upgrade process is executing
    Success,            // Update successful, waiting for restart
    Failed,             // Update failed, may need rollback
    RollingBack         // Rolling back to previous version
}

public interface IDownloadService
{
    // === Events ===
    event Action<DownloadStatistics>? StatisticsChanged;  // Any state/statistics change
    event Action<DownloadStatus>? StatusChanged;          // Status change
    event Action<string>? ErrorOccurred;                  // Error message
    event Action? UpdateCompleted;                        // Update completed

    // === Properties ===
    DownloadStatistics CurrentStatistics { get; }
    DownloadStatus Status { get; }
    bool CanStart { get; }
    bool CanPause { get; }
    bool CanRetry { get; }

    // === Methods ===
    void CheckForUpdates();            // Check for updates
    void StartDownload();              // Start download
    void Pause();                      // Pause
    void Retry();                      // Retry (resume from current state)
    void Cancel();                     // Cancel
    void Restart();                    // Restart completely
}
```

### RealDownloadService Bridge Logic

```csharp
// Maps GeneralUpdate.Core events to DownloadStatus state machine:

Bootstrap Event                 → State Transition
──────────────────────────────────────────────────
LaunchAsync starts              → Checking
UpdateInfo received             → FoundUpdate / AlreadyLatest
MultiDownloadStatistics received→ Downloading
MultiDownloadError received     → DownloadError (after N auto-retries)
MultiDownloadCompleted received → Applying
MultiAllDownloadCompleted recv. → UpgradeProgress → Success
Exception received              → Failed
```

---

## UI Framework Template List

| Template File | Framework | Features Included |
|---------|---------|---------|
| `SemiUrsaClientView.axaml` + `.cs` | Avalonia + SemiUrsa | Full state machine, dark mode toggle, notifications, progress bar animation |
| `SemiUrsaUpgradeView.axaml` + `.cs` | Avalonia + SemiUrsa (Upgrade) | Waiting UI, indeterminate progress, transition animations |
| `LayUIStyle.xaml` + `.cs` | WPF + LayUI.Wpf | Glass effect, modal dialogs, progress bar |
| `WPFDevelopersStyle.xaml` + `.cs` | WPF + WPFDevelopers | Circular progress, breath lamp animation, notification icon |
| `AntdUIStyle.cs` | WinForms + AntdUI | Dark theme, localization, wave progress button, cancel |
| `NativeWpfWindow.xaml` + `.cs` | Native WPF (no skin) | Clean window, progress bar, status text |
| `NativeWinForms.cs` | Native WinForms | Simple form, progress bar, cancel |
| `MauiUpdatePage.xaml` + `.cs` | MAUI | Cross-platform, dark mode, AppThemeBinding |
| `ConsoleProgress.cs` | Console App | ANSI progress bar, status text |
| `DownloadViewModels.cs` | Shared (all frameworks) | Complete ViewModel + DownloadStatistics |
| `RealDownloadService.cs` | Shared (all frameworks) | **Core Bridge**: GeneralUpdate → IDownloadService |

---

## Output

Based on the user's framework and requirements, output the following (in priority order):
- ✅ `RealDownloadService.cs` — Core bridge code (replaces MockDownloadService)
- ✅ `DownloadViewModels.cs` — Complete MVVM ViewModel (full state coverage)
- ✅ Target framework window/page XAML + Code-Behind
- ✅ Integration step instructions (DI registration / file replacement / Navigation)

## Related Skills

- `/generalupdate-init` — If Bootstrap is not yet configured
- `/generalupdate-troubleshoot` — If UI displays abnormal values
