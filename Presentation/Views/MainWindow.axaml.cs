using Business.Entities;
using Service;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace Presentation.Views;

public partial class MainWindow : Window
{
    private readonly IService service = new Service.Service();

    private User user = new User();
    private DailyRation ration;

    private Product? selectedProduct;

    private Dictionary<string, TreeViewItem> mealTimeTrees = new Dictionary<string, TreeViewItem>();

    public MainWindow()
    {
        InitializeComponent();
        ration = service.GetRation();

        UpdateUserInfo();
        LoadCategories();
        LoadMealTimes();
        HookEvents();
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
        foreach (Category category in service.GetCategories())
        {
            var categoryNode = new TreeViewItem { Header = category.Name };

            var products = service.GetProductsByCategory(category.Name);
            foreach (Product product in products)
            {
                var productNode = new TreeViewItem { Header = product.Name };
                categoryNode.Items.Add(productNode);
            }

            ProductCategoryTree.Items.Add(categoryNode);
        }
    }

    private void LoadMealTimes()
    {
        foreach (var kvp in ration.MealTimes)
        {
            var mealTimeNode = new TreeViewItem { Header = kvp.Key };
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

        ProductCategoryTree.PointerReleased += OnProductTreeClick;
        ProductMealTimeTree.PointerReleased += OnMealProductTreeClick;

        ProductWeightBox.TextChanged += OnProductWeightChanged;
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
        if (sender is TextBox searchBox)
        {
            string searchText = searchBox.Text ?? "";

            foreach (TreeViewItem categoryNode in ProductCategoryTree.Items!)
            {
                categoryNode.IsVisible = true;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    foreach (TreeViewItem productNode in categoryNode.Items!)
                    {
                        productNode.IsVisible = true;
                    }
                    categoryNode.IsExpanded = false;
                    continue;
                }

                searchText = searchText.ToLower();
                bool hasVisibleProducts = false;

                foreach (TreeViewItem productNode in categoryNode.Items!)
                {
                    bool isVisible = false;

                    if (productNode.Header!.ToString()!.ToLower().Contains(searchText))
                        isVisible = true;

                    productNode.IsVisible = isVisible;

                    if (isVisible)
                        hasVisibleProducts = true;
                }

                categoryNode.IsVisible = hasVisibleProducts;

                categoryNode.IsExpanded = hasVisibleProducts;
            }
        }
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

    private void OnProductTreeClick(object? sender, PointerReleasedEventArgs e)
    {
        if (ProductCategoryTree.SelectedItem is not TreeViewItem item)
            return;

        if (item.Items.Count > 0)
            return;

        string productName = item.Header!.ToString()!;

        Product newProduct = new Product(service.GetProduct(productName));

        UpdateSelectedProduct(newProduct, false);
        UpdateProductInfo(newProduct);

        if (e.InitialPressMouseButton != MouseButton.Right)
            return;

        var menu = new ContextMenu();

        foreach (var kvp in ration.MealTimes)
        {
            if (kvp.Value.HasProduct(newProduct.Name))
                continue;

            var menuItem = new MenuItem { Header = $"Добавить в {kvp.Key}" };
            menuItem.Click += (_, __) =>
            {
                kvp.Value.AddProduct(newProduct);
                AddProductToMealTimeInfo(kvp.Key, productName);
                UpdateRationInfo();
            };
            menu.Items.Add(menuItem);
        }

        menu.Open(item);
    }

    private void OnMealProductTreeClick(object? sender, PointerReleasedEventArgs e)
    {
        if (ProductMealTimeTree.SelectedItem is not TreeViewItem item)
            return;

        if (item.Parent is not TreeViewItem mealItem)
            return;

        string productName = item.Header!.ToString()!;
        string currentMeal = mealItem.Header!.ToString()!;

        Product product = ration.GetProduct(currentMeal, productName)!;

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

    private void UpdateProductInfo(Product product)
    {
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