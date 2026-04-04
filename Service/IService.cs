using Business.Entities;
using DataAccess;

namespace Service;

public interface IService
{
    List<Category> GetCategories();
    Category GetCategoryByProduct(string productName);
    Category GetCategoryByProduct(Product product);
    Product GetProduct(string productName);
    List<Product> GetProductsByCategory(string categoryName);
    DailyRation GetRation();
    Product GetMealTimeProduct(string mealtimeName, string productName);
    void InsertMealTime(string mealtimeName);
    void InsertProduct(string mealtimeName, Product product);
    void DeleteMealTime(string mealtimeName);
    void DeleteMealTimeProduct(string mealtimeName, string productName);
    void SaveRation(string filename);
    void ClearRation();
}
