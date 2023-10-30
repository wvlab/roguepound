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
        public IEnumerable<int> HWallXCoords()
        {
            for (int i = x1 + 2; i < x2 - 1; i++)
                yield return i;
        }

        public IEnumerable<int> VWallYCoords()
        {
            for (int i = y1 + 2; i < y2 - 1; i++)
                yield return i;
        }

        public (float, float) Center() => (
            x1 + (float)(x2 - x1) / 2,
            y1 + (float)(y2 - y1) / 2
        );
    }

    private readonly record struct Edge(Room Room1, Room Room2);

    enum SplitDirection { Horizontal, Vertical }

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

            foreach (int i in room.HWallXCoords())
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

            foreach (int i in room.VWallYCoords())
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
        foreach (Room room in Rooms)
        {
            Tiles[room.x1 + 1, room.y1 + 1] = Corner;
            Tiles[room.x2 - 1, room.y1 + 1] = Corner;
            Tiles[room.x1 + 1, room.y2 - 1] = Corner;
            Tiles[room.x2 - 1, room.y2 - 1] = Corner;

            foreach (int i in room.HWallXCoords())
            {
                Tiles[i, room.y2 - 1] = HWall;
                Tiles[i, room.y1 + 1] = HWall;
            }

            foreach (int i in room.VWallYCoords())
            {
                Tiles[room.x1 + 1, i] = VWall;
                Tiles[room.x2 - 1, i] = VWall;
            }

            foreach (int i in room.HWallXCoords())
            {
                foreach (int j in room.VWallYCoords())
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
        (float x1, float y1) = room1.Center();
        (float x2, float y2) = room1.Center();

        return FUtility.EuclideanDistance(x1, y1, x2, y2);
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

    private SplitDirection ChooseDirection(Room leaf)
    {
        int x = leaf.x2 - leaf.x1;
        int y = leaf.y2 - leaf.y1;

        return (x, y) switch
        {
            (_, _) when x > 2.5 * y => SplitDirection.Vertical,
            (_, _) when y > 2.5 * x => SplitDirection.Horizontal,
            _ => (SplitDirection)Rand.Next(Enum.GetValues(typeof(SplitDirection)).Length),
        };
    }

    private void GenerateTree(Room leaf, short depth)
    {
        if (leaf.x2 - leaf.x1 < 6 || leaf.y2 - leaf.y1 < 6)
        {
            return; // It's too small
        }

        if (depth >= MaxRoomDepth)
        {
            Rooms.Add(leaf);
            return;
        }

        SplitDirection direction = ChooseDirection(leaf);


        SplitRoom(leaf, direction, ++depth);
    }

    private void SplitRoom(Room leaf, SplitDirection direction, short depth)
    {
        Room leafCopy = leaf;

        int factor = 0;
        switch (direction)
        {
            case SplitDirection.Vertical:
                factor = Rand.Next(0, leaf.x2 - leaf.x1 - 6);
                leaf.x2 -= factor;
                leafCopy.x1 = leaf.x2 + 1;
                break;
            case SplitDirection.Horizontal:
                factor = Rand.Next(0, leaf.y2 - leaf.y1 - 6);
                leaf.y2 -= factor;
                leafCopy.y1 = leaf.y2 + 1;
                break;
        }

        GenerateTree(leaf, depth);
        GenerateTree(leafCopy, depth);
    }

    private IEnumerable<(int, int)> TunnelBetween((int, int) start, (int, int) end)
    {
        (int x1, int y1) = start;
        (int x2, int y2) = end;

        (int cornerX, int cornerY) = Rand.Next(2) == 0 ? (x2, y1) : (x1, y2);

        foreach (Tuple<int, int> coord in FUtility.BresenhamLine(x1, y1, cornerX, cornerY))
        {
            yield return (coord.Item1, coord.Item2);
        }

        foreach (Tuple<int, int> coord in FUtility.BresenhamLine(cornerX, cornerY, x2, y2))
        {
            yield return (coord.Item1, coord.Item2);
        }
    }

    private void ConnectRoomWalls(Room room1, Room room2)
    {
        (float x1, float y1) = room1.Center();
        (float x2, float y2) = room2.Center();

        foreach ((int x, int y) in TunnelBetween(((int)x1, (int)y1), ((int)x2, (int)y2)))
        {
            if (x >= 0 && x < Settings.TileWidth && y >= 0 && y < Settings.TileHeight)
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
