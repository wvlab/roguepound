using FunctionalRoguePound.FUtility;

namespace RoguePound;

public interface IActor
{
    string letter { get; }
    Position Position { get; set; }

}

public interface IMonster : IActor
{
    enum Behavior
    {
        Basic,
        // Distant, Ghost
    }

    Behavior GetBehavior { get; }
}


public class Player : IActor
{
    public string letter { get; } = "@";
    public Position Position { get; set; } = new Position(0, 0);

    public Player() { }
}
