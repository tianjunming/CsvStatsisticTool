# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

CSVStatsTool - CSV文件统计与透视分析工具，用于解决Excel处理超过100万行大文件困难的问题。

## 项目结构

```
CsvStatsisticTool/
├── CsvStatsisticTool.sln      # 解决方案文件
├── CLAUDE.md                 # 本文件
├── src/
│   └── CsvStatsTool/         # 主WPF应用程序
│       ├── CsvStatsTool.csproj
│       ├── App.xaml / App.xaml.cs
│       ├── Models/           # 数据模型
│       │   ├── ColumnInfo.cs
│       │   └── AggregationType.cs
│       ├── ViewModels/       # 视图模型
│       │   ├── MainViewModel.cs
│       │   └── RelayCommand.cs
│       ├── Views/            # 视图
│       │   ├── MainWindow.xaml
│       │   └── MainWindow.xaml.cs
│       ├── Services/         # 业务服务
│       │   ├── CsvService.cs
│       │   └── PivotService.cs
│       ├── Converters/       # 值转换器
│       │   └── InverseBooleanToVisibilityConverter.cs
│       └── Resources/        # 资源文件
│           └── test_data.csv
├── tests/
│   └── CsvStatsTool.Tests/   # 单元测试项目（xUnit）
└── src/
    └── CsvGenerator/         # 大数据测试文件生成器（用于性能测试）
```

## 构建和运行

```bash
# 构建整个解决方案
dotnet build

# 运行应用程序
dotnet run --project src/CsvStatsTool

# 发布Release版本
dotnet publish src/CsvStatsTool -c Release -r win-x64 --self-contained false -o ./publish

# 运行测试
dotnet test

# 运行单个测试
dotnet test --filter "FullyQualifiedName~TestName"
```

## 核心功能

1. **CSV预览** - 默认显示前10行，支持自定义预览行数
2. **数据透视** - 支持行字段、列字段、值字段的选择
3. **聚合方式** - 计数、求和、平均值、最大值、最小值、去重计数
4. **大文件处理** - 100万行数据可在约5秒内完成透视分析
5. **导出结果** - 透视结果可导出为CSV文件

## 技术栈

- .NET 8.0-windows
- WPF（Windows Presentation Foundation）
- CsvHelper 33.1.0（CSV解析）
- xUnit（单元测试）

## 架构说明 (MVVM)

- **Models/** - 数据模型，包含 ColumnInfo 和 AggregationType
- **ViewModels/** - 视图模型，包含 MainViewModel 和 RelayCommand
- **Views/** - 视图，包含 MainWindow.xaml 及代码后置
- **Services/** - 业务服务，CsvService 负责CSV读写，PivotService 负责数据透视
- **Converters/** - 值转换器，用于XAML数据绑定

## 测试数据

测试需要 `src/CsvStatsTool/Resources/test_data.csv` 文件存在
