using Business.Entities;

namespace DataAccess;

public interface IProductDAO
{
    List<Product> GetProductsByCategory(string categoryName);
    List<Product> SearchProducts(string productName);
    Product GetProduct(string productName);
    void Insert(string categoryName, Product product);
    void Delete(string categoryName, string productName);
    void Save();
}
