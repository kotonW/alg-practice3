using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AlgorithmsLab3.App.Services;

public static class FileDialogService
{
    public static async Task<(string Text, string Name)?> PickTextFileAsync(Visual owner)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel is null)
        {
            return null;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Открыть input.txt",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Текстовые файлы")
                {
                    Patterns = ["*.txt", "*.dat", "*.csv"],
                },
                FilePickerFileTypes.All,
            ],
        });

        if (files.Count == 0)
        {
            return null;
        }

        var file = files[0];
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        return (text, file.Name);
    }

    public static async Task SaveTextFileAsync(Visual owner, string suggestedName, string content)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить результаты замеров",
            SuggestedFileName = suggestedName,
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV")
                {
                    Patterns = ["*.csv"],
                },
            ],
        });

        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
    }
}
