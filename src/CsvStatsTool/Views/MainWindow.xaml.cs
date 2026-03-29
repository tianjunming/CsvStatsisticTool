using System.Windows;
using Microsoft.Win32;
using CsvStatsTool.ViewModels;

namespace CsvStatsTool.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
            Title = "选择CSV文件"
        };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.LoadFile(dialog.FileName);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshPreview();
    }

    private void ApplyPreviewRows_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PreviewRowsTextBox.Text, out int rows))
        {
            _viewModel.PreviewRowCount = rows;
            _viewModel.RefreshPreview();
        }
    }

    private void GeneratePivot_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.GeneratePivot();
    }

    private void ExportPivot_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.PivotData == null || _viewModel.PivotData.Rows.Count == 0)
        {
            MessageBox.Show("没有可导出的透视结果。", "导出", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV文件 (*.csv)|*.csv",
            Title = "导出透视结果",
            FileName = "pivot_result.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _viewModel.ExportPivotResult(dialog.FileName);
                MessageBox.Show("导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
