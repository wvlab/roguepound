using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound.Dungeon;

internal readonly record struct Edge(Room Room1, Room Room2);

public sealed class Master
{
    Random Rand = new Random();
    Tile[,] Tiles;
    Architect Architect;
    MainFrame MainFrame;
    Action ResetTiles;

    public void Generate()
    {
        while (true)
        {
            try
            {
                Architect.Generate();
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ResetTiles();
            }
        }
        MainFrame.PostProcTiles();
        MainFrame.PlaceInteractivePieces();
    }

    public Master(Action resetTiles, Tile[,] tiles, Position player) // Take reference to a player position?
    {
        Tiles = tiles;
        Architect = new Architect(Rand, tiles);
        MainFrame = new MainFrame(Rand, tiles, Architect.Rooms, player);
        ResetTiles = resetTiles;
    }
}
