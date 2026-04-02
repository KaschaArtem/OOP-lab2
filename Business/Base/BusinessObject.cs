namespace Business.Base;

public abstract class BusinessObject 
{
    List<BusinessRule> rules = new List<BusinessRule>();

    public List<string> Errors
    {
        get
        {
            List<string> errors = new List<string>();
            foreach (var rule in rules)
                if (rule.Validate(this))
                    errors.Append(rule.Error);
            return errors;
        }
    }

    protected void AddRule(BusinessRule rule)
    {
        rules.Add(rule);
    }

    public bool IsValid()
    {  
        foreach (var rule in rules)
            if (!rule.Validate(this))
                return false;
        return true;
    }
}
