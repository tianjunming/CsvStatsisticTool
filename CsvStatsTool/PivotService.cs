using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvStatsTool;

public class PivotService
{
    public DataTable GeneratePivotTable(string filePath, string rowField, string? columnField, string? valueField, AggregationType aggregation)
    {
        var result = new DataTable();

        // 如果没有指定行字段，抛出异常
        if (string.IsNullOrEmpty(rowField))
        {
            throw new ArgumentException("必须指定行字段");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null
        };

        // 读取所有数据到内存中进行处理
        // 对于超大文件，这可能需要更多内存，但这是透视分析的必要代价
        var allData = new List<Dictionary<string, string>>();

        using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            if (headers == null)
            {
                throw new Exception("CSV文件没有有效的表头");
            }

            int rowFieldIndex = Array.IndexOf(headers, rowField);
            int columnFieldIndex = columnField != null ? Array.IndexOf(headers, columnField) : -1;
            int valueFieldIndex = valueField != null ? Array.IndexOf(headers, valueField) : -1;

            if (rowFieldIndex < 0)
            {
                throw new ArgumentException($"找不到行字段: {rowField}");
            }

            while (csv.Read())
            {
                var row = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    row[headers[i]] = csv.GetField(i) ?? "";
                }
                allData.Add(row);
            }
        }

        // 进行透视分析
        // rowField -> (columnField -> values)
        // 对于去重计数，使用string存储；对于其他聚合，使用double存储
        var pivotDictNumeric = new Dictionary<string, Dictionary<string, List<double>>>();
        var pivotDictString = new Dictionary<string, Dictionary<string, List<string>>>();

        bool isDistinctCount = aggregation == AggregationType.DistinctCount;

        foreach (var row in allData)
        {
            string rowKey = row.GetValueOrDefault(rowField) ?? "(空)";
            string colKey = columnField != null ? (row.GetValueOrDefault(columnField) ?? "(空)") : "值";
            string valStr = valueField != null ? (row.GetValueOrDefault(valueField) ?? "0") : "1";

            if (isDistinctCount)
            {
                // 去重计数模式
                if (!pivotDictString.ContainsKey(rowKey))
                {
                    pivotDictString[rowKey] = new Dictionary<string, List<string>>();
                }
                if (!pivotDictString[rowKey].ContainsKey(colKey))
                {
                    pivotDictString[rowKey][colKey] = new List<string>();
                }
                pivotDictString[rowKey][colKey].Add(valStr);
            }
            else
            {
                // 数值聚合模式
                if (!double.TryParse(valStr, out double val))
                {
                    val = 0;
                }

                if (!pivotDictNumeric.ContainsKey(rowKey))
                {
                    pivotDictNumeric[rowKey] = new Dictionary<string, List<double>>();
                }
                if (!pivotDictNumeric[rowKey].ContainsKey(colKey))
                {
                    pivotDictNumeric[rowKey][colKey] = new List<double>();
                }
                pivotDictNumeric[rowKey][colKey].Add(val);
            }
        }

        // 构建结果表
        // 获取所有列键
        var allColumnKeys = new HashSet<string>();
        if (isDistinctCount)
        {
            foreach (var rowData in pivotDictString.Values)
            {
                foreach (var colKey in rowData.Keys)
                {
                    allColumnKeys.Add(colKey);
                }
            }
        }
        else
        {
            foreach (var rowData in pivotDictNumeric.Values)
            {
                foreach (var colKey in rowData.Keys)
                {
                    allColumnKeys.Add(colKey);
                }
            }
        }
        var sortedColumnKeys = allColumnKeys.OrderBy(k => k).ToList();

        // 添加行字段列
        result.Columns.Add(rowField, typeof(string));

        // 添加值列（根据聚合类型）
        string valueColumnName;
        if (valueField == null)
        {
            valueColumnName = "计数";
        }
        else
        {
            valueColumnName = aggregation switch
            {
                AggregationType.Sum => $"{valueField}_求和",
                AggregationType.Average => $"{valueField}_平均值",
                AggregationType.Max => $"{valueField}_最大值",
                AggregationType.Min => $"{valueField}_最小值",
                AggregationType.DistinctCount => $"{valueField}_去重计数",
                _ => $"{valueField}_计数"
            };
        }

        if (columnField == null)
        {
            result.Columns.Add(valueColumnName, typeof(string));
        }
        else
        {
            foreach (var colKey in sortedColumnKeys)
            {
                result.Columns.Add($"{colKey}_{valueColumnName}", typeof(string));
            }
        }

        // 添加数据行
        IEnumerable<string> rowKeysEnumerable = isDistinctCount
            ? pivotDictString.Keys.OrderBy(k => k)
            : pivotDictNumeric.Keys.OrderBy(k => k);
        var sortedRowKeys = rowKeysEnumerable.ToList();
        foreach (var rowKey in sortedRowKeys)
        {
            var row = result.NewRow();
            row[rowField] = rowKey;

            if (columnField == null)
            {
                // 无列字段，只有一列值
                if (isDistinctCount)
                {
                    var values = pivotDictString[rowKey].Values.SelectMany(v => v).ToList();
                    row[valueColumnName] = AggregateStringValues(values, aggregation).ToString("N0");
                }
                else
                {
                    var values = pivotDictNumeric[rowKey].Values.SelectMany(v => v).ToList();
                    row[valueColumnName] = AggregateValues(values, aggregation).ToString("N2");
                }
            }
            else
            {
                // 有列字段，多列值
                foreach (var colKey in sortedColumnKeys)
                {
                    string colName = $"{colKey}_{valueColumnName}";
                    bool hasKey = isDistinctCount
                        ? pivotDictString[rowKey].ContainsKey(colKey)
                        : pivotDictNumeric[rowKey].ContainsKey(colKey);
                    if (hasKey)
                    {
                        if (isDistinctCount)
                        {
                            var values = pivotDictString[rowKey][colKey];
                            row[colName] = AggregateStringValues(values, aggregation).ToString("N0");
                        }
                        else
                        {
                            var values = pivotDictNumeric[rowKey][colKey];
                            row[colName] = AggregateValues(values, aggregation).ToString("N2");
                        }
                    }
                    else
                    {
                        row[colName] = "-";
                    }
                }
            }

            result.Rows.Add(row);
        }

        return result;
    }

    private double AggregateValues(List<double> values, AggregationType aggregation)
    {
        if (values.Count == 0) return 0;

        return aggregation switch
        {
            AggregationType.Count => values.Count,
            AggregationType.Sum => values.Sum(),
            AggregationType.Average => values.Average(),
            AggregationType.Max => values.Max(),
            AggregationType.Min => values.Min(),
            _ => values.Count
        };
    }

    private int AggregateStringValues(List<string> values, AggregationType aggregation)
    {
        if (values.Count == 0) return 0;

        return aggregation switch
        {
            AggregationType.DistinctCount => values.Distinct().Count(),
            _ => values.Count
        };
    }
}
