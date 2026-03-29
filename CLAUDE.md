# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

CSVStatsTool - CSV文件统计与透视分析工具，用于解决Excel处理超过100万行大文件困难的问题。

## 项目结构

```
CsvStatsTool/           # 主WPF应用程序
  CsvStatsTool.csproj   # 项目配置（.NET 8.0-windows, WPF）
  MainWindow.xaml       # 主窗口界面
  MainWindow.xaml.cs    # 主窗口逻辑和ViewModel（含ColumnInfo、AggregationType）
  CsvService.cs         # CSV文件读取服务
  PivotService.cs       # 数据透视分析服务
  test_data.csv         # 测试数据文件

CsvStatsTool.Tests/     # 单元测试项目（xUnit）
CsvGenerator/            # 大数据测试文件生成器（用于性能测试）
```

## 构建和运行

```bash
# 构建主项目
cd CsvStatsTool
dotnet build

# 运行应用程序
dotnet run

# 发布Release版本
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish

# 运行测试
cd CsvStatsTool.Tests
dotnet test

# 运行单个测试
dotnet test --filter "FullyQualifiedName~TestName"
```

## 核心功能

1. **CSV预览** - 默认显示前10行，支持自定义预览行数
2. **数据透视** - 支持行字段、列字段、值字段的选择
3. **聚合方式** - 计数、求和、平均值、最大值、最小值
4. **大文件处理** - 100万行数据可在约5秒内完成透视分析
5. **导出结果** - 透视结果可导出为CSV文件

## 技术栈

- .NET 8.0-windows
- WPF（Windows Presentation Foundation）
- CsvHelper 33.1.0（CSV解析）
- xUnit（单元测试）

## 架构说明

- **MainWindow.xaml.cs** - 包含视图代码后置、主视图模型(MainViewModel)、列信息(ColumnInfo)和聚合类型(AggregationType)
- **CsvService** - 负责CSV文件读取和导出，使用CsvHelper库
- **PivotService** - 负责数据透视分析，支持计数、求和、平均值、最大值、最小值聚合
- **测试数据** - 测试需要 `CsvStatsTool/test_data.csv` 文件存在
