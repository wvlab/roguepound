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
    private record struct Room(int x1, int y1, int x2, int y2)
    {
        // public static implicit operator Rectangle(Room room) {
        //     return new Rectangle {};
        // }
    }
    private readonly record struct Edge(Room Room1, Room Room2);

    const int MaxRoomDepth = 5; // it will roughly give from 20 to 25 rooms
    const int RoomCountThreshold = 13;

    Random Rand = new Random();
    // Tileset TileSet; // TODO: make thematic tilesets
    ColoredTile HWall = new ColoredTile(Raylib.RED);
    ColoredTile VWall = new ColoredTile(Raylib.BLACK);
    ColoredTile Corner = new ColoredTile(Raylib.YELLOW);
    ColoredTile Floor = new ColoredTile(Raylib.RAYWHITE);
    ColoredTile Door = new ColoredTile(Raylib.BROWN);
    ColoredTile Tunnel = new ColoredTile(Raylib.VIOLET);
    List<Room> Rooms = new List<Room>();
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
        foreach (Room room in Rooms)
        {
            Tiles[room.x1 + 1, room.y1 + 1] = Corner;
            Tiles[room.x2 - 1, room.y1 + 1] = Corner;
            Tiles[room.x1 + 1, room.y2 - 1] = Corner;
            Tiles[room.x2 - 1, room.y2 - 1] = Corner;

            for (int i = room.x1 + 2; i < room.x2 - 1; i++)
            {
                if (Tiles[i, room.y2].Equals(Tunnel) && Tiles[i, room.y2 - 2].Equals(Tunnel))
                {
                    Tiles[i, room.y2 - 1] = Door;
                }
                if (Tiles[i, room.y1].Equals(Tunnel) && Tiles[i, room.y1 + 2].Equals(Tunnel))
                {
                    Tiles[i, room.y1 + 1] = Door;
                }
            }

            for (int i = room.y1 + 2; i < room.y2 - 1; i++)
            {
                if (Tiles[room.x1, i].Equals(Tunnel) && Tiles[room.x1 + 2, i].Equals(Tunnel))
                {
                    Tiles[room.x1 + 1, i] = Door;
                }
                if (Tiles[room.x2, i].Equals(Tunnel) && Tiles[room.x2 - 2, i].Equals(Tunnel))
                {
                    Tiles[room.x2 - 1, i] = Door;
                }
            }
        }
    }

    private void WriteTiles()
    {
        int h = -1;
        Color[] colors = new Color[] {
            Raylib.RAYWHITE, Raylib.BLUE, Raylib.BROWN,
        };

        foreach (Room room in Rooms)
        {
            h++;
            var color = colors[h % colors.Length];

            Tiles[room.x1 + 1, room.y1 + 1] = Corner;
            Tiles[room.x2 - 1, room.y1 + 1] = Corner;
            Tiles[room.x1 + 1, room.y2 - 1] = Corner;
            Tiles[room.x2 - 1, room.y2 - 1] = Corner;

            for (int i = room.x1 + 2; i < room.x2 - 1; i++)
            {
                Tiles[i, room.y2 - 1] = HWall;
                Tiles[i, room.y1 + 1] = HWall;
            }

            for (int i = room.y1 + 2; i < room.y2 - 1; i++)
            {
                Tiles[room.x1 + 1, i] = VWall;
                Tiles[room.x2 - 1, i] = VWall;
            }

            for (int i = room.x1 + 2; i < room.x2 - 1; i++)
            {
                for (int j = room.y1 + 2; j < room.y2 - 1; j++)
                {
                    Tiles[i, j] = Floor;
                }
            }
        }

        foreach (Edge corridor in Corridors)
        {
            ConnectRoomWalls(corridor.Room1, corridor.Room2);
        }
    }

    private void GenerateSpanningTree()
    {
        List<Room> unconnectedRooms = new List<Room>(Rooms);

        Room currentRoom = unconnectedRooms[Rand.Next(unconnectedRooms.Count)];
        unconnectedRooms.Remove(currentRoom);

        while (unconnectedRooms.Count > 0)
        {
            double minDistance = double.MaxValue;
            Room? nearestRoom = null;

            foreach (Room room in unconnectedRooms)
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

                Corridors.Add(new Edge(currentRoom, (Room)nearestRoom));
                unconnectedRooms.Remove((Room)nearestRoom);
                currentRoom = (Room)nearestRoom;
            }
        }
    }

    private double CalculateDistance(Room room1, Room room2)
    {
        float x11 = room1.x1 + (room1.x2 - room1.x1) / 2;
        float y11 = room1.y1 + (room1.y2 - room1.y1) / 2;

        float x12 = room2.x1 + (room2.x2 - room2.x1) / 2;
        float y12 = room2.y1 + (room2.y2 - room2.y1) / 2;

        return FUtility.EuclideanDistance(x11, y11, x12, y12);
    }

    private void GenerateTree()
    {
        Corridors.Clear();
        Rooms.Clear();
        Room map = new Room(0, 0, Settings.TileWidth - 1, Settings.TileHeight - 1);
        GenerateTree(map, 0);
    }

    private void FreeRandomRooms()
    {
        for (int unclearRooms = Rooms.Count() - RoomCountThreshold; unclearRooms > 0; unclearRooms--)
        {
            Rooms.RemoveAt(Rand.Next(Rooms.Count));
        }
    }

    private void GenerateTree(Room leaf, short depth)
    {
        if ((leaf.x2 - leaf.x1) < 6 || (leaf.y2 - leaf.y1) < 6)
        {
            return; // It's too small
        }

        if (depth >= MaxRoomDepth)
        {
            Rooms.Add(leaf);
            return;
        }

        int direction;
        if ((leaf.x2 - leaf.x1) > 2.5 * (leaf.y2 - leaf.y1))
        {
            direction = 0;
        }
        else if ((leaf.y2 - leaf.y1) > 2.5 * (leaf.x2 - leaf.x1))
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
            factor = Rand.Next(0, (int)(leaf.x2 - leaf.x1 - 6));
            SplitVertically(leaf, factor, depth);
        }
        else
        {
            factor = Rand.Next(0, (int)(leaf.y2 - leaf.y1 - 6));
            SplitHorizontally(leaf, factor, depth);
        }
    }

    private void SplitVertically(Room leaf, int factor, short depth)
    {
        Room leafCopy = leaf;
        leaf.x2 -= factor;
        leafCopy.x1 = leaf.x2 + 1;
        GenerateTree(leaf, ++depth);
        GenerateTree(leafCopy, depth);
    }

    private void SplitHorizontally(Room leaf, int factor, short depth)
    {
        Room leafCopy = leaf;
        leaf.y2 -= factor;
        leafCopy.y1 = leaf.y2 + 1;
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

    private void ConnectRoomWalls(Room room1, Room room2)
    {
        int x1 = room1.x1 + (room1.x2 - room1.x1) / 2;
        int y1 = room1.y1 + (room1.y2 - room1.y1) / 2;
        int x2 = room2.x1 + (room2.x2 - room2.x1) / 2;
        int y2 = room2.y1 + (room2.y2 - room2.y1) / 2;

        foreach (Tuple<int, int> coord in TunnelBetween(Tuple.Create(x1, y1), Tuple.Create(x2, y2)))
        {
            (int x, int y) = coord;

            if (x >= 0 && x < Settings.TileWidth && y >= 0 && y < Settings.TileWidth)
            {
                Tiles[x, y] = Tunnel;
            }
        }
    }

    public DungeonMaster(ITile[,] tiles) // Take reference to a player position?
    {
        Tiles = tiles;
    }
}
