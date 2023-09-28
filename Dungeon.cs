using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

// TODO: implement procedural generation postprocessing
/// <summary>
/// Generates tree of rooms using binary space partition
/// Makes routes from room to room, in a way so every room has a way into other
/// Clears random rooms if there are too much
/// Makes postprocessing, e.g. places monsters, exit and player spawn tile
/// </summary>
public sealed class DungeonMaster // maybe split roles?
{
    const int MaxRoomDepth = 5; // it will roughly give from 20 to 25 rooms
    const int RoomCountThreshold = 13;

    Random Rand = new Random();
    // Tileset TileSet; // TODO: make thematic tilesets
    List<Rectangle> Rooms = new List<Rectangle>();
    List<Edge> Corridors = new List<Edge>();
    ITile[,] Tiles;

    public void Generate()
    {
        GenerateTree();
        GenerateSpanningTree();
        FreeRandomRooms();
        WriteTiles();
        PostProcTiles();
    }

    private void PostProcTiles()
    {
        // TODO: Place monsters, exit and player spawn
    }

    private void WriteTiles()
    {
        int h = -1;
        Color[] colors = new Color[] {
            Raylib.RAYWHITE, Raylib.BLUE, Raylib.BROWN,
        };

        ColoredTile HWall = new ColoredTile(Raylib.BLACK);
        ColoredTile VWall = new ColoredTile(Raylib.RED);
        ColoredTile Corner = new ColoredTile(Raylib.YELLOW);
        foreach (Rectangle room in Rooms)
        {
            h++;
            var color = colors[h % colors.Length];

            Tiles[(int)room.x + 1, (int)room.y + 1] = Corner;
            Tiles[(int)room.width - 1, (int)room.y + 1] = Corner;
            Tiles[(int)room.x + 1, (int)room.height - 1] = Corner;
            Tiles[(int)room.width - 1, (int)room.height - 1] = Corner;

            for (int i = (int)room.x + 2; i < room.width - 1; i++)
            {
                Tiles[i, (int)room.height - 1] = HWall;
                Tiles[i, (int)room.y + 1] = HWall;
            }

            for (int i = (int)room.y + 2; i < room.height - 1; i++)
            {
                Tiles[(int)room.x + 1, i] = VWall;
                Tiles[(int)room.width - 1, i] = VWall;
            }

            for (int i = (int)room.x + 2; i < room.width - 1; i++)
            {
                for (int j = (int)room.y + 2; j < room.height - 1; j++)
                {
                    Tiles[i, j] = new ColoredTile(color);
                }
            }
        }

        foreach (Edge corridor in Corridors)
        {
            // TODO: place doors and wall rifts
            ConnectRoomWalls(corridor.Room1, corridor.Room2);
        }
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

        return FUtility.EuclideanDistance(x1, y1, x2, y2);
    }

    private void GenerateTree()
    {
        Rooms.Clear();
        Rectangle map = new Rectangle(0, 0, Settings.TileWidth - 1, Settings.TileHeight - 1);
        GenerateTree(map, 0);
    }

    private void FreeRandomRooms()
    {
        int unclearRooms = Rooms.Count() - RoomCountThreshold;
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

        if (depth >= MaxRoomDepth)
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


    public DungeonMaster(ITile[,] tiles) // Take reference to a player position?
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
