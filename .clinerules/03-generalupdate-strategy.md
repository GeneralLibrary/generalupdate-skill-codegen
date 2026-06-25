---
description: "GeneralUpdate strategies — decision tree, 6 strategies, mixed combinations"
globs: ["**/*.cs"]
tags: ["dotnet", "generalupdate"]
---

# Update Strategies

## Decision Tree

- **Has a backend (SignalR, gRPC, WebSocket, etc.)?** -> SignalR / Silent / Differential / Standard
- **No backend?** -> OSS (Object Storage Service)

## The 6 Strategies

| Strategy | When to Use |
|----------|-------------|
| **Standard** | Direct download from an HTTP/S source with basic UI progress. Simplest setup. |
| **OSS** | Files hosted on S3 / MinIO / Azure Blob / OSS-compatible storage. No backend required. |
| **Silent** | Unattended background update. No UI shown to the user. Reboots / restarts on next launch. |
| **Differential** | Binary diff (bsdiff) patches instead of full-file downloads. Save bandwidth — good for large binaries. |
| **CVP** (Cross Version Patch) | Skip intermediate versions when jumping N versions. Client applies a cumulative patch. |
| **Push** | Server-initiated update via SignalR / WebSocket push notification. Client then fetches the package. |

## Mixed Combinations

| Combination | Typical Scenario |
|-------------|------------------|
| **OSS + Silent** | Game launcher / kiosk — pulls from CDN, no UI interaction. |
| **Standard + Differential** | Client downloads full files on first update, patches thereafter. |
| **CVP + Differential** | Cross-version jump applied as a single binary patch — best of both strategies. |
| **Standard + Push** | Backend pushes a "new version available" toast; user clicks to download via HTTP. |

## Platform Considerations

| Platform | Notes |
|----------|-------|
| **Windows** | Uses **Bowl** crash daemon (restarts the app bus on unhandled exception). Full file-system access. |
| **Linux / macOS** | ✅ v10.5.0-rc.1 提供 `UnixPermissionHooks`，通过 `bootstrap.Hooks<UnixPermissionHooks>()` 自动设置执行权限。目标框架建议 .NET 8+。 |

> For the full Bootstrap pipeline and middleware architecture, see `01-generalupdate-init.md`.
