using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

// TODO: implement procedural generation postprocessing
// TODO: implement movement
// TODO: implement hud
// TODO: implement tile sets
// TODO: implement traps
// TODO: implement enemies && ai
// TODO: implement combat system
// TODO: implement score system
// TODO: implement audio system
// TODO: implement event system
// TODO: implement special theme floors
// TODO: implement statistics
// TODO: implement menus

/// <summary>
/// Class <c>Settings</c> keeps all constants and variables from settings file
/// </summary>
public sealed class Settings
{
    public Resolution Resolution;
    public const int TileSize = 32;
    public const int TileHeight = 48;
    public const int TileWidth = 80;
    public const int MaxRooms = 13;
    private static Settings instance = new Settings();

    private Settings()
    {
        Resolution = Resolution.Fullscreen;
    }

    public static Settings Instance
    {
        get
        {
            return instance;
        }
    }
}


public interface ITile
{
    void Draw(Vector2 destination);
}

public readonly struct VoidTile : ITile
{
    public void Draw(Vector2 _) { }
}


public readonly struct ColoredTile : ITile
{
    readonly Color color;
    public void Draw(Vector2 destination)
    {
        Raylib.DrawRectangleV(
            destination,
            new Vector2(Settings.TileSize, Settings.TileSize),
            color
        );
    }

    public ColoredTile(Color color) => (this.color) = (color);
}


public interface IArtist
{
    void DrawTiles(in ITile[,] tiles);
}


public sealed class Artist : IArtist
{
    public void DrawTiles(in ITile[,] tiles)
    {
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Vector2 vec = new Vector2(
                    x * Settings.TileSize,
                    y * Settings.TileSize
                );
                tiles[x, y].Draw(vec);
            }
        }
    }
}


public interface IState
{
    void Reset();
}


public struct GameState : IState
{
    public ITile[,] Tiles = new ITile[Settings.TileWidth, Settings.TileHeight];
    public Camera2D Camera = new Camera2D();
    public Vector2 Cursor;
    DungeonMaster Dungeon;

    public void Reset()
    {
        Cursor = new Vector2(0.0f, 0.0f);
        Camera.zoom = 1.0f;
        {
            Vector2 MapCenter = new Vector2(
                (float)(Settings.TileWidth * Settings.TileSize) / 2,
                (float)(Settings.TileHeight * Settings.TileSize) / 2
            );
            Vector2 ScreenCenter = new Vector2(
                (float)Raylib.GetScreenWidth() / 2,
                (float)Raylib.GetScreenHeight() / 2
            );
            Camera.offset = ScreenCenter - MapCenter;
        }
        {
            for (int x = 0; x < Settings.TileWidth; x += 1)
            {
                for (int y = 0; y < Settings.TileHeight; y += 1)
                {
                    Tiles[x, y] = new ColoredTile(Raylib.PURPLE);
                }
            }
        }
        Dungeon.Generate();
    }

    public GameState()
    {
        Dungeon = new DungeonMaster(Tiles);
        Reset();
    }
}

public sealed class DungeonMaster
{
    const int MaxDepth = 5; // it will roughly give from 20 to 25 rooms

    Random Rand = new Random();
    // Tileset TileSet;
    List<Rectangle> Rooms = new List<Rectangle>();
    List<Edge> Corridors = new List<Edge>();
    ITile[,] Tiles;

    public void Generate()
    {
        GenerateTree();
        GenerateSpanningTree();
        FreeRandomRooms();
        WriteTiles();
    }

    private void GenerateSpanningTree()
    {
        List<Rectangle> unconnectedRooms = new List<Rectangle>(Rooms);

        Rectangle currentRoom = unconnectedRooms[Rand.Next(unconnectedRooms.Count)];
        unconnectedRooms.Remove(currentRoom);

        while (unconnectedRooms.Count > 0)
        {
            double minDistance = double.MaxValue;
            Rectangle? nearestRoom = null;

            foreach (Rectangle room in unconnectedRooms)
            {
                double distance = CalculateDistance(currentRoom, room);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestRoom = room;
                }
            }

            if (nearestRoom is not null)
            {
                Corridors.Add(new Edge(currentRoom, (Rectangle)nearestRoom));
                unconnectedRooms.Remove((Rectangle)nearestRoom);
                currentRoom = (Rectangle)nearestRoom;
            }
        }
    }

    private double CalculateDistance(Rectangle room1, Rectangle room2)
    {
        float x1 = room1.x + (room1.width - room1.x) / 2;
        float y1 = room1.y + (room1.height - room1.y) / 2;

        float x2 = room2.x + (room2.width - room2.x) / 2;
        float y2 = room2.y + (room2.height - room2.y) / 2;

        double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

        return distance;
    }

    private void GenerateTree()
    {
        Rooms.Clear();
        Rectangle map = new Rectangle(0, 0, Settings.TileWidth - 1, Settings.TileHeight - 1);
        GenerateTree(map, 0);
    }

    private void FreeRandomRooms()
    {
        int unclearRooms = Rooms.Count() - Settings.MaxRooms;
        for (; unclearRooms > 0; unclearRooms--)
        {
            Rooms.RemoveAt(Rand.Next(Rooms.Count));
        }
    }

    private void GenerateTree(Rectangle leaf, short depth)
    {
        if ((leaf.width - leaf.x) < 6 || (leaf.height - leaf.y) < 6)
        {
            return; // It's too small
        }

        if (depth >= MaxDepth)
        {
            Rooms.Add(leaf);
            return;
        }

        int direction;
        if ((leaf.width - leaf.x) > 2.5 * (leaf.height - leaf.y))
        {
            direction = 0;
        }
        else if ((leaf.height - leaf.y) > 2.5 * (leaf.width - leaf.x))
        {
            direction = 1;
        }
        else
        {
            direction = Rand.Next(2);
        }

        int factor;
        if (direction == 0)
        {
            factor = Rand.Next(0, (int)(leaf.width - leaf.x - 6));
            SplitVertically(leaf, factor, depth);
        }
        else
        {
            factor = Rand.Next(0, (int)(leaf.height - leaf.y - 6));
            SplitHorizontally(leaf, factor, depth);
        }
    }

    private void SplitVertically(Rectangle leaf, int factor, short depth)
    {
        Rectangle leafCopy = leaf;
        leaf.width -= factor;
        leafCopy.x = leaf.width + 1;
        GenerateTree(leaf, ++depth);
        GenerateTree(leafCopy, depth);
    }

    private void SplitHorizontally(Rectangle leaf, int factor, short depth)
    {
        Rectangle leafCopy = leaf;
        leaf.height -= factor;
        leafCopy.y = leaf.height + 1;
        GenerateTree(leaf, ++depth);
        GenerateTree(leafCopy, depth);
    }

    private void WriteTiles()
    {
        int h = -1;
        Color[] colors = new Color[] {
            Raylib.RAYWHITE, Raylib.BLUE, Raylib.RED,
            Raylib.BROWN,    Raylib.BLACK,
        };
        foreach (Rectangle room in Rooms)
        {
            h++;
            var color = colors[h % colors.Length];

            for (int i = (int)room.x + 1; i < room.width; i++)
            {
                for (int j = (int)room.y + 1; j < room.height; j++)
                {
                    Tiles[i, j] = new ColoredTile(color);
                }
            }
        }
        foreach (Edge corridor in Corridors)
        {
            ConnectRoomWalls(corridor.Room1, corridor.Room2);
        }
    }

    private IEnumerable<Tuple<int, int>> TunnelBetween(Tuple<int, int> start, Tuple<int, int> end)
    {
        int x1 = start.Item1;
        int y1 = start.Item2;
        int x2 = end.Item1;
        int y2 = end.Item2;

        int cornerX, cornerY;
        if (Rand.Next(2) == 0)
        {
            cornerX = x2;
            cornerY = y1;
        }
        else
        {
            cornerX = x1;
            cornerY = y2;
        }

        foreach (Tuple<int, int> coord in FUtility.BresenhamLine(x1, y1, cornerX, cornerY))
        {
            yield return coord;
        }

        foreach (Tuple<int, int> coord in FUtility.BresenhamLine(cornerX, cornerY, x2, y2))
        {
            yield return coord;
        }
    }

    private void ConnectRoomWalls(Rectangle room1, Rectangle room2)
    {
        int x1 = (int)(room1.x + (room1.width - room1.x) / 2);
        int y1 = (int)(room1.y + (room1.height - room1.y) / 2);
        int x2 = (int)(room2.x + (room2.width - room2.x) / 2);
        int y2 = (int)(room2.y + (room2.height - room2.y) / 2);

        foreach (Tuple<int, int> coord in TunnelBetween(Tuple.Create(x1, y1), Tuple.Create(x2, y2)))
        {
            (int x, int y) = coord;

            if (x >= 0 && x < Settings.TileWidth && y >= 0 && y < Settings.TileHeight)
            {
                Tiles[x, y] = new ColoredTile(Raylib.VIOLET);
            }
        }
    }


    public DungeonMaster(ITile[,] tiles)
    {
        Tiles = tiles;
    }

    private class Edge
    {
        public Rectangle Room1 { get; }
        public Rectangle Room2 { get; }

        public Edge(Rectangle room1, Rectangle room2)
        {
            Room1 = room1;
            Room2 = room2;
        }
    }
}


interface IGameLogic
{
}


public sealed class GameLogic : IGameLogic
{

}


public sealed class Game
{
    Settings Settings = Settings.Instance;
    GameState State;
    Artist Artist;
    GameLogic Enigmatologist;

    public void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
        {
            State.Cursor.Y -= Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
        {
            State.Cursor.Y += Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
        {
            State.Cursor.X -= Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
        {
            State.Cursor.X += Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            State.Cursor *= 0;
            State.Camera.zoom = 1.0f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
        {
            State.Camera.offset -= new Vector2(0.01f, 0.01f);
            State.Camera.zoom -= 0.02f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
        {
            State.Camera.zoom += 0.01f;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = delta * (-1.0f / State.Camera.zoom);

            State.Cursor += delta;
        }

        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), State.Camera);

            State.Camera.offset = Raylib.GetMousePosition();

            State.Cursor = mouseWorldPos;

            State.Camera.zoom += wheel * 0.125f;
            if (State.Camera.zoom < 0.125f)
                State.Camera.zoom = 0.125f;
        }

        State.Camera.target = State.Cursor;
        Raylib.BeginDrawing();

        Raylib.ClearBackground(Raylib.BLACK);
        Raylib.BeginMode2D(State.Camera);

        Artist.DrawTiles(State.Tiles);

        Raylib.EndMode2D();

        Raylib.EndDrawing();
    }

    public Game(Artist artist, GameLogic logic)
    {
        Resolution res = Settings.Instance.Resolution;
        res.InitWindow("RoguePound");
        State = new GameState();
        Artist = artist;
        Enigmatologist = logic;
    }
}


public static class Program
{
    public static void Main(string[] args)
    {
        Settings settings = Settings.Instance;

        Artist drawer = new Artist();
        GameLogic logic = new GameLogic();
        Game game = new Game(drawer, logic);
        while (!Raylib.WindowShouldClose())
        {
            game.Update();
        }

        Raylib.CloseWindow();
    }
}
