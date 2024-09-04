namespace project1.scripts.world.entity.ai.schedule;

public class Activity
{
    public readonly string Name;

    public Activity(string name)
    {
        Name = name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is Activity activity) return Name.Equals(activity.Name);
        return false;
    }
}