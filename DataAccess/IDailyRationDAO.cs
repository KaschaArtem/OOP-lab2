using Business.Entities;

namespace DataAccess;

public interface IDailyRationDAO
{
    List<Product> GetDailyProducts();
    List<Product> GetMealTimeProducts(string mealtime);
    Product GetMealTimeProduct(string mealtimeName, string productName);
    DailyRation GetDailyRation();
    void Insert(string mealtimeName);
    void Insert(string mealtimeName, Product product);
    void Delete(string mealtimeName);
    void Delete(string mealtimeName, string productName);
    void SaveDailyRation(string filename);
    void Clear();
}
