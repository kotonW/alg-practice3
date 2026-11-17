using AlgorithmsLab3.App.Services;
using AlgorithmsLab3.App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AlgorithmsLab3.App.Views;

public partial class LinkedListView : UserControl
{
    public LinkedListView() => InitializeComponent();

    private async void LoadFileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not LinkedListPageViewModel viewModel)
        {
            return;
        }

        var result = await FileDialogService.PickTextFileAsync(this);
        if (result is { } file)
        {
            viewModel.LoadValuesFromFile(file.Text, file.Name);
        }
    }
}
