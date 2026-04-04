using Business.Entities;
using System.Xml.Linq;

namespace DataAccess;

public class DataBase
{
    public static string connectionString { get; set; } = String.Empty;
    static DataBase? instance;

    public Dictionary<string, List<Product>> Products { get; private set; }
    public List<Category> Categories { get; private set; }
    public DailyRation Ration { get; private set; }

    private DataBase(string connectionString)
    {
        if (DataBase.connectionString == String.Empty)
        {
            var basePath = AppContext.BaseDirectory;
            var fullPath = Path.Combine(basePath, "..", "..", "..", "..", "DataAccess", "products.xml");
            DataBase.connectionString = fullPath;
        }
        else
            DataBase.connectionString = connectionString;
        Products = new Dictionary<string, List<Product>>();
        Categories = new List<Category>();
        Ration = new DailyRation();
        Read(DataBase.connectionString);
    }

    private void Read(string connectionString)
    {
        XDocument xdoc = XDocument.Load(connectionString);
        foreach (XElement xcategory in xdoc.Element("Db")!.Elements("Category"))
        {
            Category category = new Category()
            {
                Name = xcategory.Attribute("name")!.Value,
            };
            Categories.Add(category);

            List<Product> categoryProducts = new List<Product>();
            foreach (XElement xproduct in xcategory.Elements("Product"))
            {
                Product product = new Product();
                product.Name = xproduct.Element("Name")!.Value;
                product.Weight = Convert.ToDouble(xproduct.Element("Gramms")!.Value);
                product.Protein100 = Convert.ToDouble(xproduct.Element("Protein")!.Value) / 100.0;
                product.Fats100 = Convert.ToDouble(xproduct.Element("Fats")!.Value) / 100.0;
                product.Carbs100 = Convert.ToDouble(xproduct.Element("Carbs")!.Value) / 100.0;
                product.Calories100 = Convert.ToDouble(xproduct.Element("Calories")!.Value);
                product.Category = category;
                categoryProducts.Add(product);
            }
            Products[category.Name] = categoryProducts;
        }
    }
    
    public static DataBase GetInstance()
    {
        if (instance == null)
            instance = new DataBase(connectionString);
        return instance;
    }

    public void Insert(string mealtimeName)
    {
        if (!Ration.MealTimes.ContainsKey(mealtimeName))
        {
            Ration.MealTimes[mealtimeName] = new MealTime(mealtimeName);
            Ration.MealAmount++;
        }
    }
    
    public void Insert(string mealtimeName, Product product)
    {
        Ration.MealTimes[mealtimeName].Meal.Add(new Product(product));
    }

    public void Delete(string mealtimeName)
    {
        Ration.MealTimes.Remove(mealtimeName);
        Ration.MealAmount--;
    }
    
    public void Delete(string mealtimeName, string productName)
    {
        foreach (Product p in Ration.MealTimes[mealtimeName].Meal)
            if (p.Name == productName)
            {
                Ration.MealTimes[mealtimeName].Meal.Remove(p);
                return;
            }
    }

    public void ClearDailyRation()
    {
        Ration = new DailyRation();
    }

    public void SaveDailyRation(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
            writer.WriteLine(Ration);
    }
}
