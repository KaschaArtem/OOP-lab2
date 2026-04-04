using Business.Entities;
using DataAccess;

namespace Service;

public class Service : IService
{
    static readonly ICategoryDAO categoryDao = new CategoryDAO();
    static readonly IProductDAO productDao = new ProductDAO();
    static readonly IDailyRationDAO rationDao = new DailyRationDAO();
    
    public List<Category> GetCategories()
    {
        return categoryDao.GetCategories();
    }

    public Category GetCategoryByProduct(string productName)
    {
        return categoryDao.GetCategoryByProduct(productName);
    }

    public Category GetCategoryByProduct(Product product)
    {
        return categoryDao.GetCategoryByProduct(product);
    }
    
    public Product GetProduct(string productName)
    {
       return productDao.GetProduct(productName);
    }

    public List<Product> GetProductsByCategory(string categoryName)
    {
        return productDao.GetProductsByCategory(categoryName);
    }

    public List<Product> SearchProducts(string productName)
    {
        return productDao.SearchProducts(productName);
    }
    
    public DailyRation GetRation()
    {
        return rationDao.GetDailyRation();
    }

    public Product GetMealTimeProduct(string mealtimeName, string productName)
    {
        return rationDao.GetMealTimeProduct(mealtimeName, productName);
    }

    public void InsertMealTime(string mealtimeName)
    {
        rationDao.Insert(mealtimeName);
    }

    public void InsertProduct(string mealtimeName, Product product)
    {
        rationDao.Insert(mealtimeName, product);
    }

    public void DeleteMealTime(string mealtimeName)
    {
        rationDao.Delete(mealtimeName);
    }

    public void DeleteMealTimeProduct(string mealtimeName, string productName)
    {
        rationDao.Delete(mealtimeName, productName);
    }

    public void SaveRation(string filename)
    {
        rationDao.SaveDailyRation(filename); 
    }
    
    public void ClearRation()
    {
        rationDao.Clear();
    }
}
