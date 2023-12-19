using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound.Dungeon;

internal static class MainFrame
{
    static public void PostProcTiles()
    {
        foreach (Room room in GameStorage.Rooms)
        {
            ChangeRoomTiles(room);
        }
    }

    static private void ChangeRoomTiles(Room room)
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
                GameStorage.Tiles[i, j].Type = TileType.Floor;
            }
        }
    }

    static private void VPlaceDoor(TileType door, int x, int y) =>
        PlaceDoor(door, x, y, x + 1, y, x - 1, y);

    static private void HPlaceDoor(TileType door, int x, int y) =>
        PlaceDoor(door, x, y, x, y + 1, x, y - 1);

    static private void PlaceDoor(TileType door, int x, int y, int x1, int y1, int x2, int y2)
    {
        if (GameStorage.Tiles[x1, y1].Type.IsPath && GameStorage.Tiles[x2, y2].Type.IsPath)
        {
            GameStorage.Tiles[x, y].Type = door;
        }
    }

    static public void PlaceInteractivePieces()
    {
        PlacePlayerAndStairs();
        foreach (Room room in GameStorage.Rooms)
        {
            (int posX, int posY) = room.RandPos(GameStorage.Rand);
            GameStorage.InteractiveObjects.Add(new InteractiveObject(
                InteractiveObjectType.NewCoins(GameStorage.Rand.Next(10, 150)),
                new(posX, posY)
            ));
        }
    }

    static private void PlacePlayerAndStairs()
    {
        Position player = GameStorage.Player.Position;
        Room spawn = GameStorage.Rooms[GameStorage.Rand.Next(GameStorage.Rooms.Count)];
        (player.X, player.Y) = spawn.RandPos(GameStorage.Rand);
        Room stairRoom = spawn;

        while (stairRoom == spawn)
        {
            stairRoom = GameStorage.Rooms[GameStorage.Rand.Next(GameStorage.Rooms.Count)];
        }

        (int posX, int posY) = stairRoom.RandPos(GameStorage.Rand);
        GameStorage.InteractiveObjects.Add(new InteractiveObject(
            InteractiveObjectType.Stairs,
            new(posX, posY)
        ));
    }
}
