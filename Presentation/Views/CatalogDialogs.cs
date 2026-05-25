using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Business.Entities;
using System.Globalization;
using System.Threading.Tasks;

namespace Presentation.Views;

internal static class CatalogDialogs
{
    private static readonly CultureInfo InputCulture = CultureInfo.InvariantCulture;

    public static async Task<string?> PromptCategoryAsync(
        Window owner,
        string title,
        IReadOnlyCollection<string> existingCategoryNames)
    {
        var nameBox = new TextBox { Watermark = "Название категории", MinWidth = 300 };
        var errorText = new TextBlock
        {
            Foreground = Brushes.DarkRed,
            IsVisible = false,
            TextWrapping = TextWrapping.Wrap
        };

        string? result = null;
        Window dialog = new Window
        {
            Title = title,
            Width = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var okButton = CreateButton("ОК", () =>
        {
            if (TryGetCategoryName(nameBox.Text, existingCategoryNames, out string? name))
            {
                result = name;
                dialog.Close();
            }
        });
        okButton.IsEnabled = false;

        void UpdateOkButtonState()
        {
            string text = nameBox.Text ?? "";
            bool isValid = TryGetCategoryName(text, existingCategoryNames, out _);
            okButton.IsEnabled = isValid;

            if (string.IsNullOrWhiteSpace(text))
            {
                errorText.IsVisible = false;
                return;
            }

            if (IsDuplicateCategory(text, existingCategoryNames))
            {
                errorText.Text = "Категория с таким названием уже существует";
                errorText.IsVisible = true;
                return;
            }

            errorText.IsVisible = false;
        }

        nameBox.TextChanged += (_, _) => UpdateOkButtonState();

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 10,
            Children =
            {
                nameBox,
                errorText,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children =
                    {
                        CreateButton("Отмена", () => dialog.Close()),
                        okButton
                    }
                }
            }
        };

        await dialog.ShowDialog(owner);
        return result;
    }

    private static bool TryGetCategoryName(
        string? text,
        IReadOnlyCollection<string> existingCategoryNames,
        out string? name)
    {
        name = text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (IsDuplicateCategory(name, existingCategoryNames))
            return false;

        return true;
    }

    private static bool IsDuplicateCategory(string name, IReadOnlyCollection<string> existingCategoryNames) =>
        existingCategoryNames.Any(existing =>
            string.Equals(existing.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    public static async Task<Product?> PromptProductAsync(Window owner, string title)
    {
        var nameBox = new TextBox { Watermark = "Название" };
        var caloriesBox = new TextBox { Watermark = "Ккал на 100 г" };
        var proteinBox = new TextBox { Watermark = "Белки (г на 100 г)" };
        var fatsBox = new TextBox { Watermark = "Жиры (г на 100 г)" };
        var carbsBox = new TextBox { Watermark = "Углеводы (г на 100 г)" };

        Product? result = null;
        Window dialog = new Window
        {
            Title = title,
            Width = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var okButton = CreateButton("ОК", () =>
        {
            if (TryBuildProduct(nameBox.Text, caloriesBox.Text, proteinBox.Text, fatsBox.Text, carbsBox.Text, out Product? product))
            {
                result = product;
                dialog.Close();
            }
        });
        okButton.IsEnabled = false;

        void UpdateOkButtonState()
        {
            okButton.IsEnabled = IsProductInputValid(
                nameBox.Text, caloriesBox.Text, proteinBox.Text, fatsBox.Text, carbsBox.Text);
        }

        nameBox.TextChanged += (_, _) => UpdateOkButtonState();
        caloriesBox.TextChanged += (_, _) => UpdateOkButtonState();
        proteinBox.TextChanged += (_, _) => UpdateOkButtonState();
        fatsBox.TextChanged += (_, _) => UpdateOkButtonState();
        carbsBox.TextChanged += (_, _) => UpdateOkButtonState();

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(12),
            Spacing = 10,
            Children =
            {
                new StackPanel
                {
                    Spacing = 8,
                    Children = { nameBox, caloriesBox, proteinBox, fatsBox, carbsBox }
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children =
                    {
                        CreateButton("Отмена", () => dialog.Close()),
                        okButton
                    }
                }
            }
        };

        await dialog.ShowDialog(owner);
        return result;
    }

    private static bool IsProductInputValid(
        string? name,
        string? calories,
        string? protein,
        string? fats,
        string? carbs)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return TryParsePositive(calories, out _)
            && TryParsePositive(protein, out _)
            && TryParsePositive(fats, out _)
            && TryParsePositive(carbs, out _);
    }

    private static bool TryBuildProduct(
        string? name,
        string? caloriesText,
        string? proteinText,
        string? fatsText,
        string? carbsText,
        out Product? product)
    {
        product = null;
        if (!IsProductInputValid(name, caloriesText, proteinText, fatsText, carbsText))
            return false;

        TryParsePositive(caloriesText, out double calories);
        TryParsePositive(proteinText, out double protein);
        TryParsePositive(fatsText, out double fats);
        TryParsePositive(carbsText, out double carbs);

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

    private static bool TryParsePositive(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string normalized = text.Trim().Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, InputCulture, out value))
            return false;

        return value >= 0;
    }

    private static Button CreateButton(string caption, Action onClick)
    {
        var button = new Button { Content = caption, MinWidth = 80 };
        button.Click += (_, _) => onClick();
        return button;
    }
}
