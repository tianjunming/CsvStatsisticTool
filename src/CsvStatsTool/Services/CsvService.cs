using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvStatsTool.Models;

namespace CsvStatsTool.Services;

public class CsvService
{
    public void LoadFile(string filePath, int skipRows, int previewRows, out DataTable preview, out long totalRows, out List<ColumnInfo> columns)
    {
        preview = new DataTable();
        columns = new List<ColumnInfo>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath, Encoding.UTF8, true);
        using var csv = new CsvReader(reader, config);

        // 跳过指定行数
        for (int i = 0; i < skipRows; i++)
        {
            csv.Read();
        }

        // 读取表头
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;
        if (headers == null || headers.Length == 0)
        {
            throw new Exception("CSV文件没有表头或格式无效");
        }

        for (int i = 0; i < headers.Length; i++)
        {
            columns.Add(new ColumnInfo { Name = headers[i], Index = i });
            preview.Columns.Add(headers[i], typeof(string));
        }

        // 读取预览数据
        int rowIndex = 0;
        while (csv.Read() && rowIndex < previewRows)
        {
            var row = preview.NewRow();
            for (int i = 0; i < headers.Length; i++)
            {
                row[i] = csv.GetField(i) ?? "";
            }
            preview.Rows.Add(row);
            rowIndex++;
        }

        // 计算总行数（不包括表头和跳过的行）
        totalRows = CountLines(filePath) - 1 - skipRows;
        if (totalRows < 0) totalRows = 0;
    }

    public void LoadPreview(string filePath, int skipRows, int previewRows, out DataTable preview)
    {
        preview = new DataTable();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath, Encoding.UTF8, true);
        using var csv = new CsvReader(reader, config);

        // 跳过指定行数
        for (int i = 0; i < skipRows; i++)
        {
            csv.Read();
        }

        // 读取表头
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;
        if (headers == null || headers.Length == 0)
        {
            throw new Exception("CSV文件没有表头或格式无效");
        }

        for (int i = 0; i < headers.Length; i++)
        {
            preview.Columns.Add(headers[i], typeof(string));
        }

        // 读取预览数据
        int rowIndex = 0;
        while (csv.Read() && rowIndex < previewRows)
        {
            var row = preview.NewRow();
            for (int i = 0; i < headers.Length; i++)
            {
                row[i] = csv.GetField(i) ?? "";
            }
            preview.Rows.Add(row);
            rowIndex++;
        }
    }

    public void ExportDataTable(DataTable dt, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        using var csv = new CsvWriter(writer, config);

        // 写入表头
        foreach (DataColumn col in dt.Columns)
        {
            csv.WriteField(col.ColumnName);
        }
        csv.NextRecord();

        // 写入数据
        foreach (DataRow row in dt.Rows)
        {
            foreach (var item in row.ItemArray)
            {
                csv.WriteField(item?.ToString() ?? "");
            }
            csv.NextRecord();
        }
    }

    private long CountLines(string filePath)
    {
        long count = 0;
        using var reader = new StreamReader(filePath, Encoding.UTF8, true);
        while (reader.ReadLine() != null)
        {
            count++;
        }
        return count;
    }
}
