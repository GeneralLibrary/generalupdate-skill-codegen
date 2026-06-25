---
description: "GeneralUpdate advanced — 10+ extension points, 4 IPC, Bowl, AOT"
globs: ["**/*.cs"]
tags: ["dotnet", "generalupdate", "advanced"]
---

# 高级定制 (v10.5.0-rc.1 可用功能 ✅)

## ✅ v10.5.0-rc.1 支持以下扩展点
- ✅ `IUpdateHooks` — 生命周期钩子: `bootstrap.Hooks<MyHooks>()`
- ✅ `IStrategy` — 自定义策略: `bootstrap.Strategy<MyStrategy>()`
- ✅ `IUpdateReporter` — 自定义上报
- ✅ `ISslValidationPolicy` — SSL 验证策略
- ✅ `IHttpAuthProvider` — HTTP 认证提供者

## ✅ 可编程 Option 系统
```csharp
bootstrap.SetOption(Option.Silent, true);
bootstrap.SetOption(Option.MaxConcurrency, 4);
bootstrap.SetOption(Option.RetryCount, 5);
bootstrap.SetOption(Option.SilentPollIntervalMinutes, 120);
```

## 零配置入口
`SetSource(updateUrl, appSecretKey)` — 自动从 manifest.json 发现身份信息

## IPC: EncryptedFile（默认，AES加密文件），暂无可替换 IPC 接口

## Bowl: procdump->监控->dump->故障报告->AutoRestore（可用，属于独立模块）
- Bowl 引用规则：只用 `GeneralUpdate.Bowl`，不额外引用 Core

## AOT: v10.5.0-rc.1 支持 NativeAOT (net8.0+), SignalR 推荐 JSON + JsonSerializerContext

## 命名空间速查
- `GeneralUpdate.Core.Hooks` — IUpdateHooks, HookContext, NoOpUpdateHooks, UnixPermissionHooks
- `GeneralUpdate.Core.Strategy` — IStrategy
- `GeneralUpdate.Core.Pipeline` — PipelineBuilder, DiffPipelineBuilder
- `GeneralUpdate.Core.Configuration` — Option, AppType, UpdateRequest
- `GeneralUpdate.Core.Download.Reporting` — IUpdateReporter
- `GeneralUpdate.Core.Event` — EventManager, IUpdateEventListener
- `GeneralUpdate.Core.Download` — 所有事件参数类型
