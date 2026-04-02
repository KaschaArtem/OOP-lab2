using System.Text;
using Business.Base;

namespace Business.Entities;

public class DailyRation : BusinessObject
{
    private int mealAmount;
    public int MealAmount
    {
        get { return mealAmount; }
        set
        {
            if (value < 0)
                mealAmount = 0;
            else
                mealAmount = value;
        }
    }
    public Dictionary<string, MealTime> MealTimes { get; set; }

    public DailyRation(int n = 3)
    {
        MealAmount = n;
        MealTimes = new Dictionary<string, MealTime>();
        MealTimes["Завтрак"] = new MealTime("Завтрак");
        MealTimes["Обед"] = new MealTime("Обед");
        MealTimes["Ужин"] = new MealTime("Ужин");
    }

    public Product? GetProduct(string mealTimeName, string productName)
    {
        foreach (Product product in MealTimes[mealTimeName].GetProducts())
        {
            if (product.Name == productName)
                return product;
        }
        return null;
    }

    public double GetTotalCalories()
    {
        double calories = 0;
        foreach (string s in MealTimes.Keys)
            foreach (Product p in MealTimes[s].Meal)
                calories += p.Calories;
        return calories;
    }

    public double GetTotalProtein()
    {
        double protein = 0;
        foreach (string s in MealTimes.Keys)
            foreach (Product p in MealTimes[s].Meal)
                protein += p.Protein;
        return protein;
    }

    public double GetTotalFats()
    {
        double fats = 0;
        foreach (string s in MealTimes.Keys)
            foreach (Product p in MealTimes[s].Meal)
                fats += p.Fats;
        return fats;
    }

    public double GetTotalCarbs()
    {
        double carbs = 0;
        foreach (string s in MealTimes.Keys)
            foreach (Product p in MealTimes[s].Meal)
                carbs += p.Carbs;
        return carbs;
    }

    public override string ToString()
    {
        var output = new StringBuilder();
        foreach (string s in MealTimes.Keys)
        {
            output.Append(s);
            output.Append(": \n");
            foreach (Product p in MealTimes[s].Meal)
            {
                output.Append(p.ToString());
                output.Append("\n");
            }
        }
        return output.ToString();
    }
}
