using Business.Entities;

namespace DataAccess;

public class DailyRationDAO : IDailyRationDAO
{
    static DataBase db = DataBase.GetInstance();
    
    public List<Product> GetDailyProducts()
    {
        List<Product> dailyProducts = new List<Product>();
        foreach (string s in db.Ration.MealTimes.Keys)
            dailyProducts.AddRange(db.Ration.MealTimes[s].Meal);
        return dailyProducts;
    }
    
    public List<Product> GetMealTimeProducts(string mealtime)
    {
        return db.Ration.MealTimes[mealtime].Meal;
    }
    
    public Product GetMealTimeProduct(string mealtimeName, string productName)
    {
        if (db.Ration.MealTimes.ContainsKey(mealtimeName))
            foreach (Product p in db.Ration.MealTimes[mealtimeName].Meal)
                if (p.Name == productName) 
                    return p;
        #pragma warning disable CS8603 // Possible null reference return.
        return null;
        #pragma warning restore CS8603 // Possible null reference return.
    }
    
    public DailyRation GetDailyRation()
    {
        return db.Ration;
    }

    public void Insert(string mealtimeName)
    {
        db.Insert(mealtimeName);
    }

    public void Insert(string mealtimeName, Product product)
    {
        db.Insert(mealtimeName,product);
    }

    public void Delete(string mealtimeName)
    {
        db.Delete(mealtimeName);
    }

    public void Delete(string mealtimeName, string productName)
    {
        db.Delete(mealtimeName, productName);
    }

    public void Clear()
    {
        db.ClearDailyRation();
    }

    public void SaveDailyRation(string filename)
    {
        db.SaveDailyRation(filename);
    }
}
