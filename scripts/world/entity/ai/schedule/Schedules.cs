namespace project1.scripts.world.entity.ai.schedule;

public class Schedules
{
    public static readonly int WorkStartTime = 2000;
    public static readonly int TotalWorkTime = 7000;
    public static readonly Schedule Empty = new();
    public static readonly Schedule Simple = new();
    public static readonly Schedule VillagerBaby = new();
    public static readonly Schedule VillagerDefault = new();

    private Schedules()
    {
    }
}