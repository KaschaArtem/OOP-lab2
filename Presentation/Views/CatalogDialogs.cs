using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Business.Entities;

namespace Presentation.Views;

internal static class CatalogDialogs
{
    private static readonly CultureInfo NumberCulture = CultureInfo.InvariantCulture;

    public static Task<string?> ShowCategoryNameDialogAsync(
        Window owner,
        string title,
        IReadOnlyCollection<string> existingNames) =>
        ShowUniqueNameDialogAsync(
            owner,
            title,
            "Название категории",
            "Категория с таким названием уже существует",
            existingNames);

    public static Task<string?> ShowMealTimeNameDialogAsync(
        Window owner,
        string title,
        IReadOnlyCollection<string> existingNames) =>
        ShowUniqueNameDialogAsync(
            owner,
            title,
            "Название (например, Полдник)",
            "Приём пищи с таким названием уже существует",
            existingNames);

    public static Task<Product?> ShowProductDialogAsync(Window owner, string title) =>
        ShowProductEditorDialogAsync(owner, title);

    private static async Task<string?> ShowUniqueNameDialogAsync(
        Window owner,
        string title,
        string watermark,
        string duplicateErrorMessage,
        IReadOnlyCollection<string> existingNames)
    {
        var nameBox = new TextBox { Watermark = watermark, MinWidth = 300 };
        var errorText = CreateErrorTextBlock();

        string? result = null;
        var dialog = CreateDialogWindow(title);

        var confirmButton = CreateDialogButton("ОК", () =>
        {
            if (TryGetValidatedUniqueName(nameBox.Text, existingNames, out string? name))
            {
                result = name;
                dialog.Close();
            }
        });
        confirmButton.IsEnabled = false;

        void RefreshConfirmButton()
        {
            string text = nameBox.Text ?? "";
            confirmButton.IsEnabled = TryGetValidatedUniqueName(text, existingNames, out _);
            UpdateDuplicateNameError(errorText, text, existingNames, duplicateErrorMessage);
        }

        nameBox.TextChanged += (_, _) => RefreshConfirmButton();

        dialog.Content = BuildDialogContent(
            nameBox,
            errorText,
            CreateDialogButton("Отмена", () => dialog.Close()),
            confirmButton);

        await dialog.ShowDialog(owner);
        return result;
    }

    private static async Task<Product?> ShowProductEditorDialogAsync(Window owner, string title)
    {
        var nameBox = new TextBox { Watermark = "Название" };
        var caloriesBox = new TextBox { Watermark = "Ккал на 100 г" };
        var proteinBox = new TextBox { Watermark = "Белки (г на 100 г)" };
        var fatsBox = new TextBox { Watermark = "Жиры (г на 100 г)" };
        var carbsBox = new TextBox { Watermark = "Углеводы (г на 100 г)" };

        Product? result = null;
        var dialog = CreateDialogWindow(title);

        var confirmButton = CreateDialogButton("ОК", () =>
        {
            if (TryCreateProductFromForm(
                    nameBox.Text,
                    caloriesBox.Text,
                    proteinBox.Text,
                    fatsBox.Text,
                    carbsBox.Text,
                    out Product? product))
            {
                result = product;
                dialog.Close();
            }
        });
        confirmButton.IsEnabled = false;

        void RefreshConfirmButton()
        {
            confirmButton.IsEnabled = IsValidProductForm(
                nameBox.Text,
                caloriesBox.Text,
                proteinBox.Text,
                fatsBox.Text,
                carbsBox.Text);
        }

        nameBox.TextChanged += (_, _) => RefreshConfirmButton();
        caloriesBox.TextChanged += (_, _) => RefreshConfirmButton();
        proteinBox.TextChanged += (_, _) => RefreshConfirmButton();
        fatsBox.TextChanged += (_, _) => RefreshConfirmButton();
        carbsBox.TextChanged += (_, _) => RefreshConfirmButton();

        dialog.Content = BuildDialogContent(
            new StackPanel
            {
                Spacing = 8,
                Children = { nameBox, caloriesBox, proteinBox, fatsBox, carbsBox }
            },
            confirmButton: confirmButton,
            cancelButton: CreateDialogButton("Отмена", () => dialog.Close()));

        await dialog.ShowDialog(owner);
        return result;
    }

    private static bool TryGetValidatedUniqueName(
        string? text,
        IReadOnlyCollection<string> existingNames,
        out string? name)
    {
        name = text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (IsNameAlreadyUsed(name, existingNames))
            return false;

        return true;
    }

    private static bool IsNameAlreadyUsed(string name, IReadOnlyCollection<string> existingNames) =>
        existingNames.Any(existing =>
            string.Equals(existing.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    private static void UpdateDuplicateNameError(
        TextBlock errorText,
        string text,
        IReadOnlyCollection<string> existingNames,
        string duplicateErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            errorText.IsVisible = false;
            return;
        }

        if (IsNameAlreadyUsed(text, existingNames))
        {
            errorText.Text = duplicateErrorMessage;
            errorText.IsVisible = true;
            return;
        }

        errorText.IsVisible = false;
    }

    private static bool IsValidProductForm(
        string? name,
        string? calories,
        string? protein,
        string? fats,
        string? carbs)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return TryParseNonNegativeDouble(calories, out _)
            && TryParseNonNegativeDouble(protein, out _)
            && TryParseNonNegativeDouble(fats, out _)
            && TryParseNonNegativeDouble(carbs, out _);
    }

    private static bool TryCreateProductFromForm(
        string? name,
        string? caloriesText,
        string? proteinText,
        string? fatsText,
        string? carbsText,
        out Product? product)
    {
        product = null;
        if (!IsValidProductForm(name, caloriesText, proteinText, fatsText, carbsText))
            return false;

        TryParseNonNegativeDouble(caloriesText, out double calories);
        TryParseNonNegativeDouble(proteinText, out double protein);
        TryParseNonNegativeDouble(fatsText, out double fats);
        TryParseNonNegativeDouble(carbsText, out double carbs);

        product = new Product
        {
            Name = name!.Trim(),
            Weight = 100,
            Calories100 = calories,
            Protein100 = protein / 100.0,
            Fats100 = fats / 100.0,
            Carbs100 = carbs / 100.0
        };
        return true;
    }

    private static bool TryParseNonNegativeDouble(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string normalized = text.Trim().Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, NumberCulture, out value))
            return false;

        return value >= 0;
    }

    private static Window CreateDialogWindow(string title) =>
        new()
        {
            Title = title,
            Width = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

    private static TextBlock CreateErrorTextBlock() =>
        new()
        {
            Foreground = Brushes.DarkRed,
            IsVisible = false,
            TextWrapping = TextWrapping.Wrap
        };

    private static Control BuildDialogContent(
        Control mainContent,
        TextBlock errorText,
        Button cancelButton,
        Button confirmButton) =>
        new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 10,
            Children =
            {
                mainContent,
                errorText,
                CreateButtonRow(cancelButton, confirmButton)
            }
        };

    private static Control BuildDialogContent(
        Control mainContent,
        Button cancelButton,
        Button confirmButton) =>
        new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 10,
            Children =
            {
                mainContent,
                CreateButtonRow(cancelButton, confirmButton)
            }
        };

    private static StackPanel CreateButtonRow(Button cancelButton, Button confirmButton) =>
        new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { cancelButton, confirmButton }
        };

    private static Button CreateDialogButton(string caption, Action onClick)
    {
        var button = new Button { Content = caption, MinWidth = 80 };
        button.Click += (_, _) => onClick();
        return button;
    }
}
