using Business.Entities;

namespace DataAccess;

public interface ICategoryDAO
{
    List<Category> GetCategories();
    Category GetCategoryByProduct(string name);
    Category GetCategoryByProduct(Product product);
    void Insert(Category category);
    void Delete(string categoryName);
    void Save();
}
