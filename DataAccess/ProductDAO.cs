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
        var result = new List<Product>();
        if (string.IsNullOrWhiteSpace(productName))
            return result;

        string query = productName.Trim().ToLower();
        foreach (List<Product> products in db.Products.Values)
        {
            foreach (Product product in products)
            {
                if (product.Name.ToLower().Contains(query))
                    result.Add(product);
            }
        }

        return result;
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

    public void Insert(string categoryName, Product product)
    {
        db.InsertProduct(categoryName, product);
        db.SaveCatalog();
    }

    public void Delete(string categoryName, string productName)
    {
        db.DeleteProduct(categoryName, productName);
        db.SaveCatalog();
    }

    public void Save()
    {
        db.SaveCatalog();
    }
}
