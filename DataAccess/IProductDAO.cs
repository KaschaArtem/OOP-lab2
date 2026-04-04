using Business.Entities;

namespace DataAccess;

public interface IProductDAO
{
    List<Product> GetProductsByCategory(string categoryName);
    List<Product> SearchProducts(string productName);
    Product GetProduct(string productName);
}
