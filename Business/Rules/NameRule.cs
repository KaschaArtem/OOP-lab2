using Business.Base;

namespace Business.Rules;

public class NameRule : BusinessRule
{
    public NameRule(string property)
        : base(property)
    {
    }

    public NameRule(string property, string error)
        : base(property, error)
    {
    }

    public override bool Validate(BusinessObject businessObject)
    {
        var value = GetPropertyValue(businessObject);
        
        if (value == null)
            return false;

        if (value is string s)
            return !string.IsNullOrWhiteSpace(s);

        return true;
    }
}
