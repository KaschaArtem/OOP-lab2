using Business.Base;

namespace Business.Entities;

public enum ActivityType
{
    Unknown,
    Low,
    Normal,
    Average,
    High
}

public class User : BusinessObject
{
    private double weight = 0;
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
    private double height = 0;
    public double Height
    {
        get { return height; }
        set
        {
            if (value < 0)
                height = 0;
            else
                height = value;
        }
    }
    private int age = 0;
    public int Age
    {
        get { return age; }
        set
        {
            if (value < 0)
                age = 0;
            else
                age = value;
        }
    }
    public ActivityType Activity { get; set; }

    public User(double weight = 75, double height = 170, int age = 30, ActivityType activity = ActivityType.Normal)
    {
        Weight = weight;
        Height = height;
        Age = age;
        Activity = activity;
    }

    public double? GetBMR()
    {
        double eps = 0.00001;
        if (Weight <= eps || Height <= eps || Age <= 0)
            return null;
        return 447.593 + 9.247 * Weight + 3.098 * Height - 4.330 * Age;
    }
    
    public double? GetARM()
    {
        if (Activity == ActivityType.Unknown)
            return null;
        switch (Activity)
        {
            case ActivityType.Low: return 1.2; 
            case ActivityType.Normal: return 1.375;
            case ActivityType.Average: return 1.55;
            case ActivityType.High: return 1.725;
            default: return 1.375;
        }
    }

    public int? GetDailyCalories()
    {
        double? bmr = GetBMR();
        double? arm = GetARM();
        if (bmr == null || arm == null)
            return null;
        return (int)(bmr * arm)!;
    }

    public int GetDailyProtein(int? calories)
    {
        return (int)(calories * 0.05)!;
    }

    public int GetDailyFats(int? calories)
    {
        return (int)(calories * 0.0335)!;
    }

    public int GetDailyCarbs(int? calories)
    {
        return (int)(calories * 0.125)!;
    }
}
