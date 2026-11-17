using AlgorithmsLab3.App.Services;
using AlgorithmsLab3.App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AlgorithmsLab3.App.Views;

public partial class StackView : UserControl
{
    public StackView() => InitializeComponent();

    private async void LoadFileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not StackPageViewModel viewModel)
        {
            return;
        }

        var result = await FileDialogService.PickTextFileAsync(this);
        if (result is { } file)
        {
            viewModel.LoadCommandsFromFile(file.Text, file.Name);
        }
    }

    private async void ExportBenchmarksClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not StackPageViewModel viewModel)
        {
            return;
        }

        await FileDialogService.SaveTextFileAsync(this, "stack-benchmarks.csv", viewModel.BuildBenchmarkCsv());
    }
}
