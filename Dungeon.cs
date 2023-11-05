using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

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

    private IEnumerable<(int, int)> TunnelBetween(Room room1, Room room2)
    {
        (float rx1, float ry1) = room1.Center();
        (float rx2, float ry2) = room2.Center();
        int x1 = (int)rx1;
        int y1 = (int)ry1;
        int x2 = (int)rx2;
        int y2 = (int)ry2;

        (int cornerX, int cornerY) = Rand.Next(2) == 0 ? (x2, y1) : (x1, y2);

        IEnumerable<(int, int)> DirectWay = FUtility.BresenhamLine(x1, y1, cornerX, cornerY)
            .Concat(FUtility.BresenhamLine(cornerX, cornerY, x2, y2));

        (int, int) temp = (0, 0);
        int xOffset = 0, yOffset = 0;
        int offsetTime = 0;
        Room prRoom = new Room(0, 0, 0, 0);
        foreach ((int x, int y) in DirectWay)
        {
            if (offsetTime <= 0)
            {
                foreach (Room room in Rooms.Where(r => !r.Equals(room1) && !r.Equals(room2)))
                {
                    if (room.Equals(prRoom))
                    {
                        continue;
                    }
                    if ((room.x1 <= x && x <= room.x2) && (y == room.y1 || y == room.y2))
                    {
                        xOffset = (Rand.Next(2) == 0 ? room.x1 : room.x2) - x;
                        int yDelta = room.y2 - room.y1;
                        offsetTime = Math.Max(yDelta, Math.Abs(y - cornerY));
                        prRoom = room;
                    }
                    else
                    if ((room.y1 <= y && y <= room.y2) && (x == room.x1 || x == room.x2))
                    {
                        yOffset = (Rand.Next(2) == 0 ? room.y1 : room.y2) - y;
                        offsetTime = room.x2 - room.x1;
                        prRoom = room;
                    }
                    if (room.Equals(prRoom)) foreach ((int mx, int my) in FUtility.BresenhamLine(x + xOffset, y + yOffset, x, y))
                        {
                            yield return (mx, my);
                        }

                }
            }

            temp = (x + FUtility.BoundInt(0, 1, offsetTime) * xOffset, y + FUtility.BoundInt(0, 1, offsetTime) * yOffset);

            if (offsetTime-- == 0)
            {
                foreach ((int mx, int my) in FUtility.BresenhamLine(x, y, x + xOffset, y + yOffset))
                {
                    yield return (mx, my);
                }
                xOffset = 0;
                yOffset = 0;
            }

            yield return temp;
        }

        if (offsetTime >= 0)
        {
            foreach ((int x, int y) in FUtility.BresenhamLine(temp.Item1, temp.Item2, x2, y2))
            {
                yield return (x, y);
            }
        }
    }

    private void ConnectRooms(Room room1, Room room2)
    {
        foreach ((int x, int y) in TunnelBetween(room1, room2))
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
