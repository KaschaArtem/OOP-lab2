using System.Reflection.Metadata.Ecma335;
using Business.Base;
using Business.Rules;

namespace Business.Entities;

public class Product : BusinessObject
{
    public string Name { get; set; } = String.Empty;
    private double calories100;
    public double Calories100
    {
        get { return calories100; }
        set
        {
            if (value < 0)
                calories100 = 0;
            else
                calories100 = value;
        }
    }
    private double weight;
    public double Weight
    {
        get { return weight; }
        set
        {
            if (value < 0)
                weight = 0;
            else
                weight = value;
        }
    }
    private double protein100;
    public double Protein100
    {
        get { return protein100; }
        set
        {
            if (value < 0)
                protein100 = 0;
            else
                protein100 = value;
        }
    }
    private double fats100;
    public double Fats100
    {
        get { return fats100; }
        set
        {
            if (value < 0)
                fats100 = 0;
            else
                fats100 = value;
        }
    }
    private double carbs100;
    public double Carbs100
    {
        get { return carbs100; }
        set
        {
            if (value < 0)
                carbs100 = 0;
            else
                carbs100 = value;
        }
    }
    public Category Category { get; set; } = new Category();

    public double Calories
    {
        get
        {
            if (Weight != 100.0) 
                return (Weight * Calories100) / 100;
            return Calories100;
        }
    }

    public double Protein
    {
        get
        {
            if (Weight != 100.0) 
                return (Weight * Protein100) / 100;
            return Protein100;
        }
    }

    public double Fats
    {
        get
        {
            if (Weight != 100.0)
                return (Weight * Fats100) / 100;
            return Fats100;
        }
    }

    public double Carbs
    {
        get
        {
            if (Weight != 100.0)
                return (Weight * Carbs100) / 100.0;
            return Carbs100;
        }
    }

    public Product() { }

    public Product(Product product)
    {
        Name = product.Name;
        Weight = product.Weight;
        Protein100 = product.Protein100;
        Fats100 = product.Fats100;
        Carbs100 = product.Carbs100;
        Calories100 = product.Calories100;
        Category = product.Category;

        AddRule(new NameRule(nameof(Name)));
    }

    public override string ToString()
    {
        return $"{Name} {Protein} {Fats} {Carbs} {Calories}";
    }
}
