---
description: "GeneralUpdate advanced — 10+ extension points, 4 IPC, Bowl, AOT"
globs: ["**/*.cs"]
tags: ["dotnet", "generalupdate", "advanced"]
---

# 高级定制 (v10.4.6 注意事项 ⚠️)

## ⚠️ v10.4.6 稳定版无以下扩展点
- ❌ 无 `IUpdateHooks` / `IStrategy` / `IProcessInfoProvider`
- ❌ 无 `SetOption` / `SilentPollOrchestrator` / `ProcessContract`
- ❌ 无可编程 Option 配置系统（仅 Configinfo 属性）

## IPC: 仅 EncryptedFile（默认，AES加密文件），无可替换 IPC 接口
- ❌ 无 NamedPipe / SharedMemory / AutoFallback 接口替换能力

## Bowl: procdump->监控->dump->故障报告->AutoRestore（可用，属于独立模块）
- Bowl 引用规则：只用 `GeneralUpdate.Bowl`，不额外引用 Core

## AOT: v10.4.6 支持 NativeAOT，SignalR 推荐 JSON + JsonSerializerContext

> ⚠️ 以上扩展点仅在 dev 分支（v10.5.0-beta.2）存在。**模板和示例均已针对 v10.4.6 稳定版编写。**
