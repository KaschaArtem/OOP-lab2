using Business.Base;
using Business.Rules;

namespace Business.Entities;

public class MealTime : BusinessObject
{
    public string Name { get; set; }
    public List<Product> Meal { get; set; }

    public MealTime(string name)
    {
        Name = name;
        Meal = new List<Product>();

        AddRule(new NameRule(nameof(Name)));
    }

    public void AddProduct(Product product)
    {
        Meal.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        Meal.Remove(product);
    }

    public List<Product> GetProducts()
    {
        return Meal;
    }

    public double GetCalories()
    {
        double calories = 0;
        foreach (Product product in Meal)
            calories += product.Calories;
        return calories;
    }

    public bool HasProduct(string productName)
    {
        foreach (var product in Meal)
        {
            if (product.Name.Equals(productName))
                return true;
        }
        return false;
    }
}
