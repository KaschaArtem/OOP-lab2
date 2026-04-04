using Business.Entities;

namespace DataAccess;

public class ProductDAO : IProductDAO
{
    static DataBase db = DataBase.GetInstance();

    public List<Product> GetProductsByCategory(string categoryName)
    {
        return db.Products[categoryName];
    }

    public List<Product> SearchProducts(string productName)
    {
        return new List<Product>();
    }
    
    public Product GetProduct(string productName)
    {
        foreach (string key in db.Products.Keys)
            foreach (Product product in db.Products[key])
                if (product.Name.Equals(productName)) return product;
        #pragma warning disable CS8603 // Possible null reference return.
        return null;
        #pragma warning restore CS8603 // Possible null reference return.
    }
}
