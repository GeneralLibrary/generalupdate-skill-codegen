---
description: "GeneralUpdate update UI — framework detection, RealDownloadService bridge, 11-state state machine"
globs: ["**/*.xaml", "**/*.axaml", "**/*.cs"]
tags: ["dotnet", "wpf", "avalonia", "winforms", "maui"]
---

# 更新界面

## 框架: SemiUrsa/LayUI/WPFDevelopers/AntdUI/MAUI

## 11状态: Idle->Checking->Found->Downloading->Paused->Error->Applying->Success->Failed->Rollback->Latest

## 桥接: RealDownloadService(GeneralUpdate.Core -> IDownloadService)
