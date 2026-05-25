using Business.Entities;
using System.Globalization;
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

    private static readonly CultureInfo XmlCulture = CultureInfo.GetCultureInfo("ru-RU");

    private static double ParseXmlDouble(string value)
    {
        return Convert.ToDouble(value.Replace(',', '.'), CultureInfo.InvariantCulture);
    }

    private static string FormatXmlDouble(double value)
    {
        return value.ToString("0.00", XmlCulture);
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
                product.Weight = ParseXmlDouble(xproduct.Element("Gramms")!.Value);
                product.Protein100 = ParseXmlDouble(xproduct.Element("Protein")!.Value) / 100.0;
                product.Fats100 = ParseXmlDouble(xproduct.Element("Fats")!.Value) / 100.0;
                product.Carbs100 = ParseXmlDouble(xproduct.Element("Carbs")!.Value) / 100.0;
                product.Calories100 = ParseXmlDouble(xproduct.Element("Calories")!.Value);
                product.Category = category;
                categoryProducts.Add(product);
            }
            Products[category.Name] = categoryProducts;
        }
    }

    public void InsertCategory(Category category)
    {
        if (Categories.Any(c => c.Name == category.Name))
            return;

        Categories.Add(category);
        Products[category.Name] = new List<Product>();
    }

    public void DeleteCategory(string categoryName)
    {
        Category? category = Categories.FirstOrDefault(c => c.Name == categoryName);
        if (category == null)
            return;

        if (Products.TryGetValue(categoryName, out List<Product>? products))
        {
            foreach (Product product in products)
                RemoveProductFromRation(product.Name);
        }

        Categories.Remove(category);
        Products.Remove(categoryName);
    }

    public void InsertProduct(string categoryName, Product product)
    {
        if (!Products.ContainsKey(categoryName))
            return;

        if (Products[categoryName].Any(p => p.Name == product.Name))
            return;

        product.Category = Categories.First(c => c.Name == categoryName);
        if (product.Weight <= 0)
            product.Weight = 100;

        Products[categoryName].Add(product);
    }

    public void DeleteProduct(string categoryName, string productName)
    {
        if (!Products.TryGetValue(categoryName, out List<Product>? products))
            return;

        Product? product = products.FirstOrDefault(p => p.Name == productName);
        if (product == null)
            return;

        products.Remove(product);
        RemoveProductFromRation(productName);
    }

    private void RemoveProductFromRation(string productName)
    {
        foreach (MealTime mealTime in Ration.MealTimes.Values)
        {
            Product? inMeal = mealTime.Meal.FirstOrDefault(p => p.Name == productName);
            if (inMeal != null)
                mealTime.Meal.Remove(inMeal);
        }
    }

    public void SaveCatalog()
    {
        var root = new XElement("Db");

        foreach (Category category in Categories)
        {
            var categoryElement = new XElement("Category",
                new XAttribute("name", category.Name),
                new XAttribute("description", ""));

            if (Products.TryGetValue(category.Name, out List<Product>? products))
            {
                foreach (Product product in products)
                {
                    categoryElement.Add(new XElement("Product",
                        new XElement("Name", product.Name),
                        new XElement("Gramms", FormatXmlDouble(product.Weight > 0 ? product.Weight : 100)),
                        new XElement("Protein", FormatXmlDouble(product.Protein100 * 100)),
                        new XElement("Fats", FormatXmlDouble(product.Fats100 * 100)),
                        new XElement("Carbs", FormatXmlDouble(product.Carbs100 * 100)),
                        new XElement("Calories", FormatXmlDouble(product.Calories100))));
                }
            }

            root.Add(categoryElement);
        }

        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null), root);
        document.Save(connectionString);
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
