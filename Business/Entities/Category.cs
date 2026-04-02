using Business.Base;
using Business.Rules;

namespace Business.Entities;

public class Category : BusinessObject
{
    public string Name { get; set; } = String.Empty;

    public Category() { }

    public Category(string name)
    {
        Name = name;

        AddRule(new NameRule(nameof(Name)));
    }
}
