using System.Data;
using CsvStatsTool;

namespace CsvStatsTool.Tests;

public class CsvServiceTests
{
    [Fact]
    public void LoadFile_WithTestData_LoadsCorrectly()
    {
        // Arrange
        var service = new CsvService();
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "CsvStatsTool", "test_data.csv");
        testFilePath = Path.GetFullPath(testFilePath);

        if (!File.Exists(testFilePath))
        {
            // 如果测试文件不存在，跳过测试
            return;
        }

        // Act
        service.LoadFile(testFilePath, 0, 10, out DataTable preview, out long totalRows, out List<ColumnInfo> columns);

        // Assert
        Assert.NotNull(preview);
        Assert.True(totalRows > 0);
        Assert.True(columns.Count > 0);
        Assert.Equal(10, preview.Rows.Count);
        Assert.Equal("产品", columns[0].Name);
    }

    [Fact]
    public void LoadFile_InvalidPath_ThrowsException()
    {
        // Arrange
        var service = new CsvService();
        var invalidPath = "nonexistent_file.csv";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            service.LoadFile(invalidPath, 0, 10, out _, out _, out _));
    }
}

public class PivotServiceTests
{
    [Fact]
    public void GeneratePivotTable_CountAggregation_ReturnsCorrectResults()
    {
        // Arrange
        var service = new PivotService();
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "CsvStatsTool", "test_data.csv");
        testFilePath = Path.GetFullPath(testFilePath);

        if (!File.Exists(testFilePath))
        {
            // 如果测试文件不存在，跳过测试
            return;
        }

        // Act
        var result = service.GeneratePivotTable(testFilePath, "类别", null, null, AggregationType.Count);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Rows.Count > 0);
        Assert.Contains(result.Columns.Cast<DataColumn>(), c => c.ColumnName == "类别");
        Assert.Contains(result.Columns.Cast<DataColumn>(), c => c.ColumnName == "计数");
    }

    [Fact]
    public void GeneratePivotTable_SumAggregation_ReturnsCorrectResults()
    {
        // Arrange
        var service = new PivotService();
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "CsvStatsTool", "test_data.csv");
        testFilePath = Path.GetFullPath(testFilePath);

        if (!File.Exists(testFilePath))
        {
            // 如果测试文件不存在，跳过测试
            return;
        }

        // Act
        var result = service.GeneratePivotTable(testFilePath, "类别", null, "第一季度", AggregationType.Sum);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Rows.Count > 0);
        Assert.Contains(result.Columns.Cast<DataColumn>(), c => c.ColumnName.Contains("第一季度_求和"));
    }

    [Fact]
    public void GeneratePivotTable_WithRowAndColumnFields_ReturnsCorrectResults()
    {
        // Arrange
        var service = new PivotService();
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "CsvStatsTool", "test_data.csv");
        testFilePath = Path.GetFullPath(testFilePath);

        if (!File.Exists(testFilePath))
        {
            // 如果测试文件不存在，跳过测试
            return;
        }

        // Act
        var result = service.GeneratePivotTable(testFilePath, "类别", "地区", "第一季度", AggregationType.Sum);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Rows.Count > 0);
        // 列名应该包含地区和值字段名
        Assert.Contains(result.Columns.Cast<DataColumn>(), c => c.ColumnName.Contains("北京") && c.ColumnName.Contains("第一季度"));
    }

    [Fact]
    public void GeneratePivotTable_EmptyRowField_ThrowsException()
    {
        // Arrange
        var service = new PivotService();
        var testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "CsvStatsTool", "test_data.csv");
        testFilePath = Path.GetFullPath(testFilePath);

        if (!File.Exists(testFilePath))
        {
            // 如果测试文件不存在，跳过测试
            return;
        }

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.GeneratePivotTable(testFilePath, "", null, null, AggregationType.Count));
    }
}
