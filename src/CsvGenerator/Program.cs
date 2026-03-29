using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using CsvStatsTool;
using CsvStatsTool.Models;
using CsvStatsTool.Services;

Console.WriteLine("=== CSV统计工具 大文件测试 ===\n");

string testFilePath = "large_test_data.csv";
var csvService = new CsvService();
var pivotService = new PivotService();

try
{
    // 测试1: 加载文件预览
    Console.WriteLine("测试1: 加载文件预览...");
    var stopwatch = Stopwatch.StartNew();
    System.Data.DataTable preview;
    long totalRows;
    List<ColumnInfo> columns;
    csvService.LoadFile(testFilePath, 0, 10, out preview, out totalRows, out columns);
    stopwatch.Stop();
    Console.WriteLine($"  - 总行数: {totalRows:N0}");
    Console.WriteLine($"  - 列数: {columns.Count}");
    Console.WriteLine($"  - 预览行数: {preview.Rows.Count}");
    Console.WriteLine($"  - 加载时间: {stopwatch.ElapsedMilliseconds}ms\n");

    // 测试2: 生成透视表（按类别统计销量求和）
    Console.WriteLine("测试2: 生成透视表（按类别统计）...");
    stopwatch.Restart();
    var pivotResult = pivotService.GeneratePivotTable(testFilePath, "类别", null, "销量", AggregationType.Sum);
    stopwatch.Stop();
    Console.WriteLine($"  - 透视表行数: {pivotResult.Rows.Count}");
    Console.WriteLine($"  - 透视表列数: {pivotResult.Columns.Count}");
    Console.WriteLine($"  - 生成时间: {stopwatch.ElapsedMilliseconds}ms\n");

    // 显示部分结果
    Console.WriteLine("透视结果（按类别销量求和）:");
    foreach (System.Data.DataRow row in pivotResult.Rows)
    {
        Console.WriteLine($"  {row["类别"]}: {row[1]}");
    }
    Console.WriteLine();

    // 测试3: 二维透视表（按类别和地区）
    Console.WriteLine("测试3: 二维透视表（按类别和地区）...");
    stopwatch.Restart();
    var pivot2d = pivotService.GeneratePivotTable(testFilePath, "类别", "地区", "销量", AggregationType.Sum);
    stopwatch.Stop();
    Console.WriteLine($"  - 透视表行数: {pivot2d.Rows.Count}");
    Console.WriteLine($"  - 透视表列数: {pivot2d.Columns.Count}");
    Console.WriteLine($"  - 生成时间: {stopwatch.ElapsedMilliseconds}ms\n");

    // 显示部分结果
    Console.WriteLine("透视结果（按类别和地区销量求和）:");
    foreach (System.Data.DataRow row in pivot2d.Rows)
    {
        var values = new List<string>();
        for (int i = 1; i < Math.Min(4, pivot2d.Columns.Count); i++)
        {
            values.Add(row[i]?.ToString() ?? "-");
        }
        Console.WriteLine($"  {row["类别"]}: {string.Join(", ", values)}...");
    }
    Console.WriteLine();

    Console.WriteLine("=== 所有测试通过！ ===");
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
