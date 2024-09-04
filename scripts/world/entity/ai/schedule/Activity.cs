namespace project1.scripts.world.entity.ai.schedule;

public class Activity
{
    public static readonly Activity Core = new Activity("core");
    public static readonly Activity Idle = new Activity("idle");
    
    public readonly string Name;
    
    public Activity(string name)
    {
        Name = name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Activity activity)
        {
            return Name.Equals(activity.Name);
        }
        return false;
    }
}