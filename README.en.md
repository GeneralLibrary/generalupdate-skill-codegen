# GeneralUpdate Skill CodeGen

**Claude Code Skill Suite** — helps .NET developers integrate the [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) auto-update system into any .NET application in under 5 minutes.

Covers 50+ known issues discovered from real GitHub/Gitee feedback, providing production-ready code generation and deep troubleshooting.

> Version: 0.0.1-bate.1 (2026-06-16)

---

## Skills Overview

| Skill | Command | One-Liner | Coverage |
|-------|---------|-----------|----------|
| 🚀 `generalupdate-init` | `/generalupdate-init` | Dual-project scaffold + Bootstrap config (4 methods) | 4 scenes + 4 config methods + full API |
| 🎨 `generalupdate-ui` | `/generalupdate-ui` | Auto-detect UI framework, generate full-state update window (11 states) | 6 UI frameworks + full state machine + bridge code |
| ⚙️ `generalupdate-strategy` | `/generalupdate-strategy` | 6-strategy decision tree + mixed combinations + platform diff | 6 strategies + 4 combos + platform matrix |
| 🔧 `generalupdate-advanced` | `/generalupdate-advanced` | 10+ extension points + 4 IPC + Bowl + AOT | 10+ ext points + full architecture diagram |
| 🩺 `generalupdate-troubleshoot` | `/generalupdate-troubleshoot` | 50+ known issues diagnosis + 6-step universal check | 8 critical + 11 high + 20 medium + 12 low |

---

## Quick Start

In Claude Code, simply describe your needs:

```
"Add auto-update to my WPF app"
→ Automatically activates generalupdate-init + generalupdate-ui

"Update succeeded but the app crashes on startup"
→ Automatically activates generalupdate-troubleshoot

"Configure OSS silent update"
→ Automatically activates generalupdate-strategy

"Add Bowl crash daemon + custom Hooks"
→ Automatically activates generalupdate-advanced
```

### Prerequisites

1. **Claude Code**: requires [Claude Code CLI](https://claude.com/claude-code) installed and configured
2. **.NET SDK**: target project must target .NET 8+ (.NET 10 recommended)
3. **GeneralUpdate Server**: for standard strategies, deploy [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) or a compatible backend
4. **Dual-Process Architecture**: basic understanding of the Client + Upgrade dual-process model

---

## Data Sources

All skill content is derived from real-world sources:

- **GitHub Issues**: #308–#517 (refactoring, bugs, features, tests)
- **Gitee Issues**: 30 real user reports (Chinese community pain points)
- **Full Code Audit**: 17 CRITICAL/HIGH + 14 MEDIUM + 10 INFO findings
- **Samples Source**: CompleteUpdateSample, SilentUpdateSample, OssSample, DifferentialSample, PushSample, BowlSample, ExtensionSample, CompressSample, ImDiskQuickInstallSample
- **UI Samples**: SemiUrsa, LayUI, AntdUI, WPFDevelopers, MauiUpdate, AndroidUpdate

---

## Skill File Structure

```
.claude/skills/
├── generalupdate-init/         (7 files)
│   ├── SKILL.md                     ← 4 scenes + 3 config methods + API deep-dive
│   ├── reference.md                 ← NuGet/API/protocol/framework compatibility
│   └── templates/
│       ├── MinimalIntegration.cs    ← 3 lines + annotations
│       ├── FullIntegration.cs       ← Full config + Upgrade process + appsettings
│       ├── generalupdate.manifest.json
│       └── project-scaffold/
│           ├── ClientApp.csproj / ClientProgram.cs
│           └── UpgradeApp.csproj / UpgradeProgram.cs
│
├── generalupdate-ui/           (10 files)
│   ├── SKILL.md                     ← 11-state UI state machine + framework detection
│   └── templates/
│       ├── RealDownloadService.cs   ← ★ Core bridge: Mock→GeneralUpdate
│       ├── DownloadViewModels.cs    ← Full-state MVVM ViewModel
│       ├── SemiUrsaClientView.axaml ← Avalonia full-state window
│       ├── SemiUrsaUpgradeView.axaml
│       ├── LayUIStyle.xaml          ← WPF+LayUI
│       ├── WPFDevelopersStyle.xaml  ← WPF+WPFDevelopers
│       ├── AntdUIStyle.cs           ← WinForms+AntdUI
│       └── MauiUpdatePage.xaml/.cs  ← MAUI
│
├── generalupdate-strategy/     (7 files)
│   ├── SKILL.md                     ← Decision tree + 6 strategies + mixing + platform
│   └── examples/
│       ├── ClientServerStrategy.cs  ← Standard server mode
│       ├── OssStrategy.cs           ← Object storage mode
│       ├── SilentStrategy.cs        ← Silent polling mode
│       ├── DifferentialStrategy.cs  ← Delta update mode
│       ├── CrossVersionStrategy.cs  ← Cross-version CVP mode
│       └── PushStrategy.cs          ← SignalR push mode
│
├── generalupdate-advanced/     (6 files)
│   ├── SKILL.md                     ← 10+ ext points + 4 IPC + Bowl + event system
│   ├── reference.md                 ← Extension API quick ref + Bowl options
│   └── templates/
│       ├── CustomHooks.cs           ← Full IUpdateHooks + Unix permissions
│       ├── CustomStrategy.cs        ← Custom platform strategy
│       ├── BowlIntegration.cs       ← Crash daemon config
│       └── NamedPipeIPC.cs          ← Named pipe IPC replacement
│
└── generalupdate-troubleshoot/ (2 files)
    ├── SKILL.md                     ← Diagnostic workflow
    └── reference.md                 ← ★ 50+ symptom catalog (C/H/M/L levels)
```

---

## Known Limitations

> ⚠️ **NuGet Reference Rules**:
> - Core only: `dotnet add package GeneralUpdate.Core`
> - With Bowl: reference **only** `GeneralUpdate.Bowl` (it transitively includes Core — the two cannot coexist)
> - Differential types are already embedded in Core, **no need** for `GeneralUpdate.Differential`

> ⚠️ **API Surface**: The NuGet stable release (v10.4.6) has a simpler API than the development branch (v10.5.0-beta.2). The following are **not available** in the current stable release:
> - No programmable `Option` config system (only `Configinfo` properties)
> - No `IUpdateHooks` lifecycle hooks
> - No `IStrategy` replaceable strategy interface
> - No `SilentPollOrchestrator`
> - No `ProcessContract`/IPC replacement interface
>
> These features may ship in a future version. All templates target the **stable v10.4.6** API.

See [BUGS.md](BUGS.md) for the full audit trail.

---

## Contributing

1. File an Issue to report bugs or request features
2. Fork this repo, add/modify skills under `.claude/skills/`
3. Ensure template code aligns with the latest GeneralUpdate API
4. Submit a PR

### Development Guide

```bash
# Test skills locally in Claude Code
claude-code --load-skills .claude/skills/

# Verify template code compiles
dotnet build your-test-project/
```

---

## License

Apache 2.0 — consistent with the GeneralUpdate main project.

## Related Projects

- [GeneralUpdate](https://github.com/GeneralLibrary/GeneralUpdate) — .NET auto-update core library
- [GeneralSpacestation](https://github.com/JusterZhu/GeneralSpacestation) — Update server backend
- [GeneralUpdate-Samples](https://github.com/GeneralLibrary/GeneralUpdate-Samples) — Sample projects collection
