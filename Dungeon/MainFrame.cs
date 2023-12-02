using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound.Dungeon;

internal sealed record class MainFrame(
    Random Rand,
    Tile[,] Tiles,
    List<Room> Rooms,
    List<InteractiveObject> InteractiveObjects,
    Position player
)
{
    public void PostProcTiles()
    {
        foreach (Room room in Rooms)
        {
            ChangeRoomTiles(room);
        }
    }

    private void ChangeRoomTiles(Room room)
    {
        foreach (int i in room.HWallXCoords())
        {
            HPlaceDoor(TileType.TDoor, i, room.y1 + Room.WallOffset);
            HPlaceDoor(TileType.BDoor, i, room.y2 - Room.WallOffset);
        }

        foreach (int i in room.VWallYCoords())
        {
            VPlaceDoor(TileType.LDoor, room.x1 + Room.WallOffset, i);
            VPlaceDoor(TileType.RDoor, room.x2 - Room.WallOffset, i);
        }

        foreach (int i in room.HWallXCoords())
        {
            foreach (int j in room.VWallYCoords())
            {
                Tiles[i, j].Type = TileType.Floor;
            }
        }
    }

    private void VPlaceDoor(TileType door, int x, int y) =>
        PlaceDoor(door, x, y, x + 1, y, x - 1, y);

    private void HPlaceDoor(TileType door, int x, int y) =>
        PlaceDoor(door, x, y, x, y + 1, x, y - 1);

    private void PlaceDoor(TileType door, int x, int y, int x1, int y1, int x2, int y2)
    {
        if (Tiles[x1, y1].Type.IsPath && Tiles[x2, y2].Type.IsPath)
        {
            Tiles[x, y].Type = door;
        }
    }

    public void PlaceInteractivePieces()
    {
        PlacePlayerAndStairs();
        foreach (Room room in Rooms)
        {
            (int posX, int posY) = room.RandPos(Rand);
            InteractiveObjects.Add(new InteractiveObject(
                InteractiveObjectType.NewCoins(Rand.Next(10, 150)),
                new(posX, posY)
            ));
        }
    }

    private void PlacePlayerAndStairs()
    {
        Room spawn = Rooms[Rand.Next(Rooms.Count)];
        (player.X, player.Y) = spawn.RandPos(Rand);
        Room stairRoom = spawn;

        while (stairRoom == spawn)
        {
            stairRoom = Rooms[Rand.Next(Rooms.Count)];
        }

        (int posX, int posY) = stairRoom.RandPos(Rand);
        InteractiveObjects.Add(new InteractiveObject(
            InteractiveObjectType.Stairs,
            new(posX, posY)
        ));
    }
}
