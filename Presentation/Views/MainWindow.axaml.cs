using Business.Entities;
using Service;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Views;

public partial class MainWindow : Window
{
    private readonly IService service = new Service.Service();

    private User user = new User();
    private DailyRation ration;

    private Product? selectedProduct;

    private Dictionary<string, TreeViewItem> mealTimeTrees = new Dictionary<string, TreeViewItem>();

    private const string CategoryNodeTag = "category";
    private const string MealTimeNodeTag = "mealtime";

    private string? currentMealPlanPath;
    private const string WindowTitleBase = "Daily Ration Maker";

    public MainWindow()
    {
        InitializeComponent();
        ration = service.GetRation();

        UpdateUserInfo();
        LoadCategories();
        LoadMealTimes();
        HookEvents();
        UpdateWindowTitle();
    }

    private void UpdateUserInfo()
    {
        UserWeightBox.Text = user.Weight.ToString();
        UserHeightBox.Text = user.Height.ToString();
        UserAgeBox.Text = user.Age.ToString();

        UpdateBmrBox();

        switch (user.Activity)
        {
            case ActivityType.Low: RadioLow.IsChecked = true; break;
            case ActivityType.Normal: RadioNormal.IsChecked = true; break;
            case ActivityType.Average: RadionAverage.IsChecked = true; break;
            case ActivityType.High: RadioHigh.IsChecked = true; break;
        }

        UpdateArmBox();

        UpdateDailyNorm();
    }

    private void LoadCategories()
    {
        RefreshCategoryTree();
    }

    private void RefreshCategoryTree()
    {
        ProductCategoryTree.Items.Clear();

        foreach (Category category in service.GetCategories())
        {
            var categoryNode = new TreeViewItem
            {
                Header = category.Name,
                Tag = CategoryNodeTag
            };

            foreach (Product product in service.GetProductsByCategory(category.Name))
            {
                categoryNode.Items.Add(new TreeViewItem { Header = product.Name });
            }

            ProductCategoryTree.Items.Add(categoryNode);
        }

        ApplyCategorySearchFilter();
    }

    private void RefreshMealTimeTree()
    {
        ProductMealTimeTree.SelectedItem = null;
        ProductMealTimeTree.Items.Clear();
        mealTimeTrees.Clear();
        LoadMealTimes();
        ClearProductInfo();
        UpdateRationInfo();
    }

    private void LoadMealTimes()
    {
        foreach (var kvp in ration.MealTimes)
        {
            var mealTimeNode = new TreeViewItem
            {
                Header = kvp.Key,
                Tag = MealTimeNodeTag
            };
            mealTimeTrees.Add(kvp.Key, mealTimeNode);

            foreach (var product in kvp.Value.Meal)
            {
                var productNode = new TreeViewItem { Header = product.Name };
                mealTimeNode.Items.Add(productNode);
            }

            ProductMealTimeTree.Items.Add(mealTimeNode);
        }
    }

    private void HookEvents()
    {
        UserWeightBox.TextChanged += OnUserWeightChanged;
        UserHeightBox.TextChanged += OnUserHeightChanged;
        UserAgeBox.TextChanged += OnUserAgeChanged;

        RadioLow.IsCheckedChanged += OnRadioChanged;
        RadioNormal.IsCheckedChanged += OnRadioChanged;
        RadionAverage.IsCheckedChanged += OnRadioChanged;
        RadioHigh.IsCheckedChanged += OnRadioChanged;

        ProductCategorySearch.TextChanged += OnProductCategorySearchChanged;
        ProductMealTimeSearch.TextChanged += OnProductMealTimeSearchChanged;

        AddCategoryButton.Click += OnAddCategoryClick;
        AddMealTimeButton.Click += OnAddMealTimeClick;
        ProductCategoryTree.PointerReleased += OnProductTreeClick;
        ProductMealTimeTree.PointerReleased += OnMealProductTreeClick;

        OpenMealPlanButton.Click += OnOpenMealPlanClick;
        CreateMealPlanButton.Click += OnCreateMealPlanClick;
        SaveMealPlanButton.Click += OnSaveMealPlanClick;
        SaveMealPlanAsButton.Click += OnSaveMealPlanAsClick;
        ClearRation.Click += OnClearRationClick;
        SaveRationAsPDF.Click += OnSaveRationAsPdfClick;

        ProductWeightBox.TextChanged += OnProductWeightChanged;

        UpdateRationInfo();
    }

    private void ReloadRationFromService()
    {
        ration = service.GetRation();
        RefreshMealTimeTree();
    }

    private void UpdateWindowTitle()
    {
        string currentFile = string.IsNullOrWhiteSpace(currentMealPlanPath)
            ? "Новый файл"
            : Path.GetFileName(currentMealPlanPath);
        Title = $"{WindowTitleBase} — {currentFile}";
    }

    private void OnCreateMealPlanClick(object? sender, RoutedEventArgs e)
    {
        service.ClearRation();
        currentMealPlanPath = null;
        ReloadRationFromService();
        ClearProductInfo();
        UpdateWindowTitle();
    }

    private async void OnOpenMealPlanClick(object? sender, RoutedEventArgs e)
    {
        string? path = await PickMealPlanFileAsync(open: true);
        if (path == null)
            return;

        try
        {
            service.LoadRation(path);
            currentMealPlanPath = path;
            ReloadRationFromService();
            ClearProductInfo();
            UpdateWindowTitle();
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Не удалось открыть рацион", ex.Message);
        }
    }

    private async void OnSaveMealPlanClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(currentMealPlanPath))
        {
            await SaveMealPlanAsAsync();
            return;
        }

        await SaveMealPlanToFileAsync(currentMealPlanPath, showSuccessMessage: false);
    }

    private async void OnSaveMealPlanAsClick(object? sender, RoutedEventArgs e)
    {
        await SaveMealPlanAsAsync();
    }

    private async Task SaveMealPlanAsAsync()
    {
        string? path = await PickMealPlanFileAsync(open: false);
        if (path == null)
            return;

        await SaveMealPlanToFileAsync(path, showSuccessMessage: true);
    }

    private async Task SaveMealPlanToFileAsync(string path, bool showSuccessMessage)
    {
        try
        {
            service.SaveRation(path);
            currentMealPlanPath = path;
            UpdateWindowTitle();
            if (showSuccessMessage)
                await ShowMessageAsync("Рацион сохранён", path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Не удалось сохранить рацион", ex.Message);
        }
    }

    private void OnClearRationClick(object? sender, RoutedEventArgs e)
    {
        service.ClearRation();
        currentMealPlanPath = null;
        ReloadRationFromService();
        ClearProductInfo();
        UpdateWindowTitle();
    }

    private async void OnSaveRationAsPdfClick(object? sender, RoutedEventArgs e)
    {
        string? path = await PickMealPlanFileAsync(open: false, pdf: true);
        if (path == null)
            return;

        try
        {
            service.SaveRationPlainText(path);
            await ShowMessageAsync("Рацион сохранён", path);
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Не удалось сохранить рацион", ex.Message);
        }
    }

    private static readonly FilePickerFileType MealPlanFileType = new("Рацион (XML)")
    {
        Patterns = new[] { "*.xml" },
        MimeTypes = new[] { "application/xml" }
    };

    private async Task<string?> PickMealPlanFileAsync(bool open, bool pdf = false)
    {
        if (StorageProvider == null)
            return null;

        if (open)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть рацион",
                AllowMultiple = false,
                FileTypeFilter = new[] { MealPlanFileType }
            });

            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = pdf ? "Сохранить рацион (PDF)" : "Сохранить рацион как",
            DefaultExtension = pdf ? "pdf" : "xml",
            SuggestedFileName = pdf ? "ration.pdf" : "ration.xml",
            FileTypeChoices = pdf
                ? new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
                : new[] { MealPlanFileType }
        });

        return file?.TryGetLocalPath();
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(12),
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button
                    {
                        Content = "ОК",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        MinWidth = 80
                    }
                }
            }
        };

        if (dialog.Content is StackPanel panel && panel.Children[^1] is Button okButton)
            okButton.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    private void OnUserWeightChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            string text = tb.Text ?? "";

            if (double.TryParse(text, out double weight))
                user.Weight = weight;
            else
                user.Weight = 0;
            UpdateBmrBox();
        }
    }

    private void OnUserHeightChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            string text = tb.Text ?? "";

            if (double.TryParse(text, out double height))
                user.Height = height;
            else
                user.Height = 0;
            UpdateBmrBox();
        }
    }

    private void OnUserAgeChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            string text = tb.Text ?? "";

            if (int.TryParse(text, out int age))
                user.Age = age;
            else
                user.Age = 0;
            UpdateBmrBox();
        }
    }

    private void UpdateBmrBox()
    {
        double? bmr = user.GetBMR();
        if (bmr == null)
            BmrBox.Text = "";
        else
            BmrBox.Text = ((int)bmr).ToString();
        UpdateDailyNorm();
    }

    private void OnRadioChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.IsChecked == true)
        {
            user.Activity = rb.Content!.ToString() switch
            {
                "Сидячий образ жизни" => ActivityType.Low,
                "Умеренная активность" => ActivityType.Normal,
                "Средняя активность" => ActivityType.Average,
                "Высокая активность" => ActivityType.High,
                _ => user.Activity
            };
            UpdateArmBox();
        }
    }

    private void UpdateArmBox()
    {
        double? arm = user.GetARM();
        if (arm == null)
            ArmBox.Text = "";
        else
            ArmBox.Text = Math.Round(arm.Value, 3).ToString();
        UpdateDailyNorm();
    }

    private void UpdateDailyNorm()
    {
        int? totalCalories = user.GetDailyCalories();
        if (totalCalories == null)
        {
            NormalCaloriesBox.Text = "";
            NormalProteinBox.Text = "";
            NormalFatsBox.Text = "";
            NormalCarbsBox.Text = "";

            DailyCaloriesProgress.Maximum = 0;
            DailyProteinProgress.Maximum = 0;
            DailyFatsProgress.Maximum = 0;
            DailyCarbsProgress.Maximum = 0;
        }
        else
        {
            double totalProtein = user.GetDailyProtein(totalCalories);
            double totalFats = user.GetDailyFats(totalCalories);
            double totalCarbs = user.GetDailyCarbs(totalCalories);

            NormalCaloriesBox.Text = totalCalories.ToString();
            NormalProteinBox.Text = totalProtein.ToString();
            NormalFatsBox.Text = totalFats.ToString();
            NormalCarbsBox.Text = totalCarbs.ToString();

            DailyCaloriesProgress.Maximum = (int)totalCalories;
            DailyProteinProgress.Maximum = (int)totalFats;
            DailyFatsProgress.Maximum = (int)totalFats;
            DailyCarbsProgress.Maximum = (int)totalCarbs;
        }
    }

    private void OnProductCategorySearchChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyCategorySearchFilter();
    }

    private void ApplyCategorySearchFilter()
    {
        string searchText = ProductCategorySearch.Text ?? "";

        if (string.IsNullOrWhiteSpace(searchText))
        {
            foreach (TreeViewItem categoryNode in ProductCategoryTree.Items!)
            {
                categoryNode.IsVisible = true;
                foreach (TreeViewItem productNode in categoryNode.Items!)
                    productNode.IsVisible = true;
                categoryNode.IsExpanded = false;
            }
            return;
        }

        searchText = searchText.ToLower();
        var matchedNames = service.SearchProducts(searchText)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (TreeViewItem categoryNode in ProductCategoryTree.Items!)
        {
            bool hasVisibleProducts = false;

            foreach (TreeViewItem productNode in categoryNode.Items!)
            {
                string productName = productNode.Header!.ToString()!;
                bool isVisible = matchedNames.Contains(productName);
                productNode.IsVisible = isVisible;
                if (isVisible)
                    hasVisibleProducts = true;
            }

            categoryNode.IsVisible = hasVisibleProducts;
            categoryNode.IsExpanded = hasVisibleProducts;
        }
    }

    private async void OnAddCategoryClick(object? sender, RoutedEventArgs e)
    {
        var existingNames = service.GetCategories().Select(c => c.Name).ToList();
        string? name = await CatalogDialogs.ShowCategoryNameDialogAsync(this, "Новая категория", existingNames);
        if (string.IsNullOrWhiteSpace(name))
            return;

        service.AddCategory(name);
        RefreshCategoryTree();
    }

    private async void OnAddMealTimeClick(object? sender, RoutedEventArgs e)
    {
        var existingNames = ration.MealTimes.Keys.ToList();
        string? name = await CatalogDialogs.ShowMealTimeNameDialogAsync(this, "Новый приём пищи", existingNames);
        if (string.IsNullOrWhiteSpace(name))
            return;

        service.InsertMealTime(name);
        ReloadRationFromService();
    }

    private void OnProductMealTimeSearchChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox searchBox)
        {
            string searchText = searchBox.Text ?? "";

            foreach (TreeViewItem mealTimeNode in ProductMealTimeTree.Items!)
            {
                mealTimeNode.IsVisible = true;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    foreach (TreeViewItem productNode in mealTimeNode.Items!)
                    {
                        productNode.IsVisible = true;
                    }
                    continue;
                }

                searchText = searchText.ToLower();
                bool hasVisibleProducts = false;

                foreach (TreeViewItem productNode in mealTimeNode.Items!)
                {
                    bool isVisible = false;

                    if (productNode.Header!.ToString()!.ToLower().Contains(searchText))
                        isVisible = true;

                    productNode.IsVisible = isVisible;

                    if (isVisible)
                        hasVisibleProducts = true;
                }

                mealTimeNode.IsVisible = hasVisibleProducts;

                mealTimeNode.IsExpanded = hasVisibleProducts;
            }
        }
    }

    private async void OnProductTreeClick(object? sender, PointerReleasedEventArgs e)
    {
        if (ProductCategoryTree.SelectedItem is not TreeViewItem item)
            return;

        if (IsCategoryNode(item))
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
                ShowCategoryContextMenu(item);
            return;
        }

        if (!IsCatalogProductNode(item))
            return;

        string productName = item.Header!.ToString()!;
        Product? catalogProduct = service.GetProduct(productName);
        if (catalogProduct == null)
            return;

        Product newProduct = new Product(catalogProduct);

        UpdateSelectedProduct(newProduct, false);
        UpdateProductInfo(newProduct);

        if (e.InitialPressMouseButton != MouseButton.Right)
            return;

        await ShowCatalogProductContextMenuAsync(item, newProduct);
    }

    private static bool IsCategoryNode(TreeViewItem item) =>
        item.Tag as string == CategoryNodeTag;

    private static bool IsCatalogProductNode(TreeViewItem item) =>
        item.Parent is TreeViewItem parent && parent.Tag as string == CategoryNodeTag;

    private static string GetCategoryName(TreeViewItem productNode) =>
        ((TreeViewItem)productNode.Parent!).Header!.ToString()!;

    private void ShowCategoryContextMenu(TreeViewItem categoryNode)
    {
        string categoryName = categoryNode.Header!.ToString()!;
        var menu = new ContextMenu();

        var addProduct = new MenuItem { Header = "Добавить продукт" };
        addProduct.Click += async (_, _) =>
        {
            Product? product = await CatalogDialogs.ShowProductDialogAsync(this, "Новый продукт");
            if (product == null)
                return;

            service.AddProduct(categoryName, product);
            RefreshCategoryTree();
        };

        var deleteCategory = new MenuItem { Header = "Удалить категорию" };
        deleteCategory.Click += (_, _) =>
        {
            service.RemoveCategory(categoryName);
            RefreshCategoryTree();
            RefreshMealTimeTree();
        };

        menu.Items.Add(addProduct);
        menu.Items.Add(deleteCategory);
        menu.Open(categoryNode);
    }

    private Task ShowCatalogProductContextMenuAsync(TreeViewItem item, Product newProduct)
    {
        string productName = newProduct.Name;
        string categoryName = GetCategoryName(item);
        var menu = new ContextMenu();

        foreach (var kvp in ration.MealTimes)
        {
            if (kvp.Value.HasProduct(newProduct.Name))
                continue;

            var menuItem = new MenuItem { Header = $"Добавить в {kvp.Key}" };
            menuItem.Click += (_, _) =>
            {
                kvp.Value.AddProduct(new Product(newProduct));
                AddProductToMealTimeInfo(kvp.Key, productName);
                UpdateRationInfo();
            };
            menu.Items.Add(menuItem);
        }

        if (menu.Items.Count > 0)
            menu.Items.Add(new Separator());

        var deleteProduct = new MenuItem { Header = "Удалить из каталога" };
        deleteProduct.Click += (_, _) =>
        {
            service.RemoveProduct(categoryName, productName);
            RefreshCategoryTree();
            RefreshMealTimeTree();
            ClearProductInfo();
        };
        menu.Items.Add(deleteProduct);

        menu.Open(item);
        return Task.CompletedTask;
    }

    private void ClearProductInfo()
    {
        selectedProduct = null;
        ProductNameBox.Text = "";
        ProductCaloriesBox.Text = "";
        ProductProteinBox.Text = "";
        ProductFatsBox.Text = "";
        ProductCarbsBox.Text = "";
        ProductWeightBox.Text = "";
        ProductWeightBox.IsReadOnly = true;
    }

    private void OnMealProductTreeClick(object? sender, PointerReleasedEventArgs e)
    {
        if (ProductMealTimeTree.SelectedItem is not TreeViewItem item)
            return;

        if (IsMealTimeNode(item))
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
                ShowMealTimeContextMenu(item);
            return;
        }

        if (item.Parent is not TreeViewItem mealItem)
            return;

        string productName = item.Header!.ToString()!;
        string currentMeal = mealItem.Header!.ToString()!;

        if (!ration.MealTimes.ContainsKey(currentMeal))
            return;

        Product? product = ration.GetProduct(currentMeal, productName);
        if (product == null)
        {
            ClearProductInfo();
            return;
        }

        UpdateSelectedProduct(product, true);
        UpdateProductInfo(product);

        if (e.InitialPressMouseButton != MouseButton.Right)
            return;

        var menu = new ContextMenu();

        var moveMenu = new MenuItem { Header = "Переместить в..." };

        foreach (var kvp in ration.MealTimes)
        {
            if (kvp.Key == currentMeal || kvp.Value.HasProduct(product.Name))
                continue;

            var menuItem = new MenuItem { Header = kvp.Key };
            menuItem.Click += (_, __) =>
            {
                ration.MealTimes[currentMeal].RemoveProduct(product);
                kvp.Value.AddProduct(product);
                RemoveProductFromMealTimeInfo(currentMeal, productName);
                AddProductToMealTimeInfo(kvp.Key, productName);
                UpdateRationInfo();

            };
            moveMenu.Items.Add(menuItem);
        }

        var delete = new MenuItem { Header = "Удалить" };
        delete.Click += (_, __) =>
        {
            ration.MealTimes[currentMeal].RemoveProduct(product);
            RemoveProductFromMealTimeInfo(currentMeal, productName);
            UpdateRationInfo();
        };

        menu.Items.Add(moveMenu);
        menu.Items.Add(delete);

        menu.Open(item);
    }

    private static bool IsMealTimeNode(TreeViewItem item) =>
        item.Tag as string == MealTimeNodeTag;

    private void ShowMealTimeContextMenu(TreeViewItem mealTimeNode)
    {
        string mealTimeName = mealTimeNode.Header!.ToString()!;
        var menu = new ContextMenu();

        var deleteMealTime = new MenuItem { Header = "Удалить приём пищи" };
        deleteMealTime.Click += (_, _) =>
        {
            service.DeleteMealTime(mealTimeName);
            ReloadRationFromService();
            ClearProductInfo();
        };
        menu.Items.Add(deleteMealTime);

        menu.Open(mealTimeNode);
    }

    private void UpdateSelectedProduct(Product product, bool isFromMealTime)
    {
        if (isFromMealTime)
        {
            ProductWeightBox.IsReadOnly = false;
            selectedProduct = product;
        }
        else
        {
            ProductWeightBox.IsReadOnly = true;
            selectedProduct = null;
        }
    }

    private void UpdateProductInfo(Product? product)
    {
        if (product == null)
        {
            ClearProductInfo();
            return;
        }

        ProductNameBox.Text = product.Name;
        ProductCaloriesBox.Text = Math.Round(product.Calories, 1).ToString();
        ProductProteinBox.Text = Math.Round(product.Protein, 1).ToString();
        ProductFatsBox.Text = Math.Round(product.Fats, 1).ToString();
        ProductCarbsBox.Text = Math.Round(product.Carbs, 1).ToString();
        if (product.Weight != 0)
            ProductWeightBox.Text = Math.Round(product.Weight, 1).ToString();
        else
            ProductWeightBox.Text = "";
    }

    private void AddProductToMealTimeInfo(string key, string productName)
    {
        if (mealTimeTrees.TryGetValue(key, out var mealTimeNode))
        {
            var productNode = new TreeViewItem { Header = productName };
            mealTimeNode.Items.Add(productNode);
        }
    }

    private void RemoveProductFromMealTimeInfo(string key, string productName)
    {
        if (mealTimeTrees.TryGetValue(key, out var mealTimeNode))
        {
            TreeViewItem? nodeToRemove = null;
            foreach (TreeViewItem item in mealTimeNode.Items!)
            {
                if (item.Header?.ToString() == productName)
                {
                    nodeToRemove = item;
                    break;
                }
            }

            if (nodeToRemove != null)
            {
                mealTimeNode.Items.Remove(nodeToRemove);
            }
        }
    }
    
    private void OnProductWeightChanged(object? sender, TextChangedEventArgs e)
    {
        if (selectedProduct == null)
            return;

        if (sender is not TextBox tb)
            return;

        string text = tb.Text ?? "";

        if (double.TryParse(text, out double weight))
            selectedProduct!.Weight = weight;
        else
            selectedProduct!.Weight = 0;

        UpdateProductInfo(selectedProduct);
        UpdateRationInfo();
    }

    private void UpdateRationInfo()
    {   
        double totalCalories = Math.Round(ration.GetTotalCalories(), 1);
        double totalProtein = Math.Round(ration.GetTotalProtein(), 1);
        double totalFats = Math.Round(ration.GetTotalFats(), 1);
        double totalCarbs = Math.Round(ration.GetTotalCarbs(), 1);

        DailyCaloriesInfo.Text = totalCalories.ToString();
        DailyProteinInfo.Text = totalProtein.ToString();
        DailyFatsInfo.Text = totalFats.ToString();
        DailyCarbsInfo.Text = totalCarbs.ToString();

        DailyCaloriesProgress.Value = totalCalories;
        DailyProteinProgress.Value = totalProtein;
        DailyFatsProgress.Value = totalFats;
        DailyCarbsProgress.Value = totalCarbs;
    }
}