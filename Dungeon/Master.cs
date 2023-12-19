using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound.Dungeon;

internal readonly record struct Edge(Room Room1, Room Room2);

public sealed record class Master(Action ResetTiles)
{
    public void Generate()
    {
        while (true)
        {
            try
            {
                Architect.Generate();
                break;
            }
            catch (BrokenDungeonException e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
                ResetTiles();
            }
        }
        MainFrame.PostProcTiles();
        MainFrame.PlaceInteractivePieces();
        MainFrame.PlaceMonsters();
    }
}
