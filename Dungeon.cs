using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public record struct Room(int x1, int y1, int x2, int y2)
{
    public const int WallOffset = 2;

    public IEnumerable<int> HWallXCoords()
    {
        for (int i = x1 + WallOffset + 1; i < x2 - WallOffset; i++)
            yield return i;
    }

    public IEnumerable<int> VWallYCoords()
    {
        for (int i = y1 + WallOffset + 1; i < y2 - WallOffset; i++)
            yield return i;
    }

    public (float, float) Center() => (
        x1 + (float)(x2 - x1) / 2,
        y1 + (float)(y2 - y1) / 2
    );
}

public readonly record struct Edge(Room Room1, Room Room2);

public sealed record class DungeonMainFrame(Random Rand, ITile[,] Tiles)
{
    DumbTileSet DumbTileSet = new DumbTileSet();

    public void PostProcTiles(ITileSet TileSet, List<Room> Rooms)
    {
        foreach (Room room in Rooms)
        {
            Tiles[room.x1 + Room.WallOffset, room.y1 + Room.WallOffset] = TileSet.LTCorner;
            Tiles[room.x2 - Room.WallOffset, room.y1 + Room.WallOffset] = TileSet.RTCorner;
            Tiles[room.x1 + Room.WallOffset, room.y2 - Room.WallOffset] = TileSet.LBCorner;
            Tiles[room.x2 - Room.WallOffset, room.y2 - Room.WallOffset] = TileSet.RBCorner;

            foreach (int i in room.HWallXCoords())
            {
                foreach (int j in room.VWallYCoords())
                {
                    Tiles[i, j] = TileSet.Floor;
                }
            }

            foreach (int i in room.HWallXCoords())
            {
                if (Tiles[i, room.y1 + Room.WallOffset + 1].Equals(DumbTileSet.Path))
                {
                    Tiles[i, room.y1 + Room.WallOffset] = TileSet.TDoor;
                }
                else
                {
                    Tiles[i, room.y1 + Room.WallOffset] = TileSet.HWall;
                }

                if (Tiles[i, room.y2 - Room.WallOffset - 1].Equals(DumbTileSet.Path))
                {
                    Tiles[i, room.y2 - Room.WallOffset] = TileSet.BDoor;
                }
                else
                {
                    Tiles[i, room.y2 - Room.WallOffset] = TileSet.HWall;
                }
            }

            foreach (int i in room.VWallYCoords())
            {
                if (Tiles[room.x1 + Room.WallOffset + 1, i].Equals(DumbTileSet.Path))
                {
                    Tiles[room.x1 + Room.WallOffset, i] = TileSet.LDoor;
                }
                else
                {
                    Tiles[room.x1 + Room.WallOffset, i] = TileSet.VWall;
                }
                if (Tiles[room.x2 - Room.WallOffset - 1, i].Equals(DumbTileSet.Path))
                {
                    Tiles[room.x2 - Room.WallOffset, i] = TileSet.RDoor;
                }
                else
                {
                    Tiles[room.x2 - Room.WallOffset, i] = TileSet.VWall;
                }
            }
        }

    }
}

public sealed record class DungeonArchitect(Random Rand, ITile[,] Tiles)
{
    const int MaxRoomDepth = 5; // it will roughly give from 20 to 25 rooms
    const int RoomCountThreshold = 13;
    public List<Edge> Corridors = new List<Edge>();
    public List<Room> Rooms = new List<Room>();
    DumbTileSet DumbTileSet = new DumbTileSet();

    enum SplitDirection { Horizontal, Vertical }

    private void WriteTiles()
    {
        foreach (Room room in Rooms)
        {
            Tiles[room.x1 + Room.WallOffset, room.y1 + Room.WallOffset] = DumbTileSet.LTCorner;
            Tiles[room.x2 - Room.WallOffset, room.y1 + Room.WallOffset] = DumbTileSet.RTCorner;
            Tiles[room.x1 + Room.WallOffset, room.y2 - Room.WallOffset] = DumbTileSet.LBCorner;
            Tiles[room.x2 - Room.WallOffset, room.y2 - Room.WallOffset] = DumbTileSet.RTCorner;

            foreach (int i in room.HWallXCoords())
            {
                Tiles[i, room.y2 - Room.WallOffset] = DumbTileSet.HWall;
                Tiles[i, room.y1 + Room.WallOffset] = DumbTileSet.HWall;
            }

            foreach (int i in room.VWallYCoords())
            {
                Tiles[room.x1 + Room.WallOffset, i] = DumbTileSet.VWall;
                Tiles[room.x2 - Room.WallOffset, i] = DumbTileSet.VWall;
            }

            foreach (int i in room.HWallXCoords())
            {
                foreach (int j in room.VWallYCoords())
                {
                    Tiles[i, j] = DumbTileSet.Floor;
                }
            }
        }

        foreach (Edge corridor in Corridors)
        {
            ConnectRoomWalls(corridor.Room1, corridor.Room2);
        }
    }

    public void Generate()
    {
        GenerateTree();
        GenerateSpanningTree();
        FreeRandomRooms();
        WriteTiles();
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
        if (leaf.x2 - leaf.x1 < Room.WallOffset * 3 + 2
        || leaf.y2 - leaf.y1 < Room.WallOffset * 3 + 2)
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

        switch (direction)
        {
            case SplitDirection.Vertical:
                {
                    int factor = Rand.Next(0, leaf.x2 - leaf.x1 - Room.WallOffset * 2 - 2);
                    leaf.x2 -= factor;
                    leafCopy.x1 = leaf.x2 + 1;
                }
                break;
            case SplitDirection.Horizontal:
                {
                    int factor = Rand.Next(0, leaf.y2 - leaf.y1 - Room.WallOffset * 2 - 2);
                    leaf.y2 -= factor;
                    leafCopy.y1 = leaf.y2 + 1;
                }
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
                Tiles[x, y] = DumbTileSet.Path;
            }
        }
    }
}

public sealed class DungeonMaster
{
    Random Rand = new Random();
    public ITileSet TileSet = new TestingTileSet();
    ITile[,] Tiles;
    DungeonArchitect Architect;
    DungeonMainFrame MainFrame;

    public void Generate()
    {
        Architect.Generate();
        MainFrame.PostProcTiles(TileSet, Architect.Rooms);
    }

    public DungeonMaster(ITile[,] tiles) // Take reference to a player position?
    {
        Tiles = tiles;
        Architect = new DungeonArchitect(Rand, tiles);
        MainFrame = new DungeonMainFrame(Rand, tiles);
    }
}
