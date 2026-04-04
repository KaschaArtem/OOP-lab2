using Business.Entities;

namespace DataAccess;

public interface ICategoryDAO
{
    List<Category> GetCategories();
    Category GetCategoryByProduct(string name);
    Category GetCategoryByProduct(Product product);
}
