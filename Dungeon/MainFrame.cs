using FunctionalRoguePound;

namespace RoguePound.Dungeon;

internal sealed record class MainFrame(Random Rand, ITile[,] Tiles, List<Room> Rooms)
{
    DumbTileSet DumbTileSet = new DumbTileSet();

    public void PostProcTiles(ITileSet TileSet)
    {
        foreach (Room room in Rooms)
        {
            ChangeRoomTiles(TileSet, room);
        }

        ChangePathes(TileSet);
    }

    private void ChangePathes(ITileSet TileSet)
    {
        for (int x = 0; x < Settings.TileWidth; x++)
        {
            for (int y = 0; y < Settings.TileHeight; y++)
            {
                Tiles[x, y] = Tiles[x, y] switch
                {
                    DumbTileSet.DumbPath t => TileSet.Path,
                    _ => Tiles[x, y]
                };
            }
        }
    }

    private void ChangeRoomTiles(ITileSet TileSet, Room room)
    {
        Tiles[room.x1 + Room.WallOffset, room.y1 + Room.WallOffset] = TileSet.LTCorner;
        Tiles[room.x2 - Room.WallOffset, room.y1 + Room.WallOffset] = TileSet.RTCorner;
        Tiles[room.x1 + Room.WallOffset, room.y2 - Room.WallOffset] = TileSet.LBCorner;
        Tiles[room.x2 - Room.WallOffset, room.y2 - Room.WallOffset] = TileSet.RBCorner;

        foreach (int i in room.HWallXCoords())
        {
            HPlaceDoor(TileSet.TDoor, TileSet.HWall, i, room.y1 + Room.WallOffset);
            HPlaceDoor(TileSet.BDoor, TileSet.HWall, i, room.y2 - Room.WallOffset);
        }

        foreach (int i in room.VWallYCoords())
        {
            VPlaceDoor(TileSet.LDoor, TileSet.VWall, room.x1 + Room.WallOffset, i);
            VPlaceDoor(TileSet.RDoor, TileSet.VWall, room.x2 - Room.WallOffset, i);
        }

        foreach (int i in room.HWallXCoords())
        {
            foreach (int j in room.VWallYCoords())
            {
                Tiles[i, j] = TileSet.Floor;
            }
        }
    }

    private void VPlaceDoor(ITile door, ITile wall, int x, int y) =>
        PlaceDoor(door, wall, x, y, x + 1, y, x - 1, y);

    private void HPlaceDoor(ITile door, ITile wall, int x, int y) =>
        PlaceDoor(door, wall, x, y, x, y + 1, x, y - 1);

    private void PlaceDoor(ITile door, ITile wall, int x, int y, int x1, int y1, int x2, int y2)
    {
        if (Tiles[x1, y1].Equals(DumbTileSet.Path)
        && (Tiles[x2, y2].Equals(DumbTileSet.Path)))
        {
            Tiles[x, y] = door;
        }
        else
        {
            Tiles[x, y] = wall;
        }
    }
}
