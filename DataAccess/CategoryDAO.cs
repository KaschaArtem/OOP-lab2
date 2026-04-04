using Business.Entities;

namespace DataAccess;

public class CategoryDAO : ICategoryDAO
{
    static DataBase db = DataBase.GetInstance();

    public List<Category> GetCategories()
    {
        return db.Categories;
    }

    public Category GetCategoryByProduct(Product product)
    {
        return product.Category;
    }

    public Category GetCategoryByProduct(string productName)
    {
        ProductDAO productDAO = new ProductDAO();
        return productDAO.GetProduct(productName).Category;
    }
}
