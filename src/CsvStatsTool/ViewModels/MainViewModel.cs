using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CsvStatsTool.Models;
using CsvStatsTool.Services;

namespace CsvStatsTool.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly CsvService _csvService;
    private readonly PivotService _pivotService;

    private string _currentFileName = "";
    private string _currentFilePath = "";
    private long _totalRowCount;
    private int _columnCount;
    private int _previewRowCount = 10;
    private int _skipRows = 0;
    private string _statusMessage = "请打开CSV文件";
    private bool _isLoading = false;
    private System.Data.DataTable _previewData = new();
    private System.Data.DataTable _pivotData = new();
    private List<ColumnInfo> _columns = new();
    private ColumnInfo? _selectedRowField;
    private ColumnInfo? _selectedColumnField;
    private ColumnInfo? _selectedValueField;
    private string _selectedAggregation = "计数";

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        _csvService = new CsvService();
        _pivotService = new PivotService();

        ClearRowFieldCommand = new RelayCommand(_ => { SelectedRowField = null; });
        ClearColumnFieldCommand = new RelayCommand(_ => { SelectedColumnField = null; });
        ClearValueFieldCommand = new RelayCommand(_ => { SelectedValueField = null; });
    }

    public List<string> AggregationTypes { get; } = new() { "计数", "求和", "平均值", "最大值", "最小值", "去重计数" };

    public ICommand ClearRowFieldCommand { get; }
    public ICommand ClearColumnFieldCommand { get; }
    public ICommand ClearValueFieldCommand { get; }

    public bool HasFile => !string.IsNullOrEmpty(_currentFilePath);
    public bool HasPivotResult => _pivotData != null && _pivotData.Rows.Count > 0;
    public long PivotRowCount => _pivotData?.Rows.Count ?? 0;

    public string CurrentFileName
    {
        get => _currentFileName;
        set { _currentFileName = value; OnPropertyChanged(); }
    }

    public long TotalRowCount
    {
        get => _totalRowCount;
        set { _totalRowCount = value; OnPropertyChanged(); }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set { _columnCount = value; OnPropertyChanged(); }
    }

    public int PreviewRowCount
    {
        get => _previewRowCount;
        set { _previewRowCount = value; OnPropertyChanged(); }
    }

    public int SkipRows
    {
        get => _skipRows;
        set { _skipRows = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public System.Data.DataTable PreviewData
    {
        get => _previewData;
        set { _previewData = value; OnPropertyChanged(); }
    }

    public System.Data.DataTable PivotData
    {
        get => _pivotData;
        set { _pivotData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPivotResult)); OnPropertyChanged(nameof(PivotRowCount)); }
    }

    public List<ColumnInfo> Columns
    {
        get => _columns;
        set { _columns = value; OnPropertyChanged(); }
    }

    public ColumnInfo? SelectedRowField
    {
        get => _selectedRowField;
        set { _selectedRowField = value; OnPropertyChanged(); }
    }

    public ColumnInfo? SelectedColumnField
    {
        get => _selectedColumnField;
        set { _selectedColumnField = value; OnPropertyChanged(); }
    }

    public ColumnInfo? SelectedValueField
    {
        get => _selectedValueField;
        set { _selectedValueField = value; OnPropertyChanged(); }
    }

    public string SelectedAggregation
    {
        get => _selectedAggregation;
        set { _selectedAggregation = value; OnPropertyChanged(); }
    }

    public void LoadFile(string filePath)
    {
        try
        {
            StatusMessage = "正在加载文件...";
            IsLoading = true;
            Mouse.OverrideCursor = Cursors.Wait;

            _csvService.LoadFile(filePath, _skipRows, _previewRowCount, out var preview, out long totalRows, out List<ColumnInfo> columns);

            PreviewData = preview;
            Columns = columns;
            TotalRowCount = totalRows;
            ColumnCount = columns.Count;
            CurrentFileName = System.IO.Path.GetFileName(filePath);
            _currentFilePath = filePath;

            // 默认选择
            if (columns.Count > 0)
            {
                SelectedRowField = columns[0];
                SelectedColumnField = columns.Count > 1 ? columns[1] : columns[0];
                SelectedValueField = columns.Count > 2 ? columns[2] : columns[0];
            }

            StatusMessage = $"已加载: {CurrentFileName}, 总行数: {totalRows:N0}";
            OnPropertyChanged(nameof(HasFile));
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
            MessageBox.Show($"加载CSV文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            Mouse.OverrideCursor = null;
        }
    }

    public void RefreshPreview()
    {
        if (!HasFile) return;

        try
        {
            StatusMessage = "正在刷新预览...";
            IsLoading = true;
            Mouse.OverrideCursor = Cursors.Wait;

            _csvService.LoadPreview(_currentFilePath, _skipRows, _previewRowCount, out var preview);

            PreviewData = preview;
            StatusMessage = $"预览已刷新，显示 {_previewRowCount} 行";
        }
        catch (Exception ex)
        {
            StatusMessage = $"刷新失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            Mouse.OverrideCursor = null;
        }
    }

    public void GeneratePivot()
    {
        if (!HasFile)
        {
            MessageBox.Show("请先打开CSV文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedRowField == null)
        {
            MessageBox.Show("请选择行字段。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            StatusMessage = "正在生成透视表...";
            IsLoading = true;
            Mouse.OverrideCursor = Cursors.Wait;

            var aggregation = SelectedAggregation switch
            {
                "求和" => AggregationType.Sum,
                "平均值" => AggregationType.Average,
                "最大值" => AggregationType.Max,
                "最小值" => AggregationType.Min,
                "去重计数" => AggregationType.DistinctCount,
                _ => AggregationType.Count
            };

            PivotData = _pivotService.GeneratePivotTable(
                _currentFilePath,
                SelectedRowField?.Name,
                SelectedColumnField?.Name,
                SelectedValueField?.Name,
                aggregation);

            StatusMessage = $"透视表已生成，共 {PivotRowCount:N0} 行";
        }
        catch (Exception ex)
        {
            StatusMessage = $"生成失败: {ex.Message}";
            MessageBox.Show($"生成透视表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            Mouse.OverrideCursor = null;
        }
    }

    public void ExportPivotResult(string filePath)
    {
        if (_pivotData == null) return;

        _csvService.ExportDataTable(_pivotData, filePath);
        StatusMessage = $"已导出到: {filePath}";
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
