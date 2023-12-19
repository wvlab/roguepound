using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;

static class Enigmatologist
{
    public static void UpdateFogOfWar(Player player, IEnumerable<Room> rooms, Tile[,] tiles)
    {
        (int posX, int posY) = (player.Position.X, player.Position.Y);
        foreach (Room room in rooms)
        {
            if (!room.IsInRoom(posX, posY))
            {
                continue;
            }

            if (!tiles[room.x1 + Room.WallOffset + 1, room.y1 + Room.WallOffset + 1].IsOpen)
            {
                foreach (int i in room.HWallXCoords().Append(room.x1 + Room.WallOffset).Append(room.x2 - Room.WallOffset))
                {
                    foreach (int j in room.VWallYCoords().Append(room.y1 + Room.WallOffset).Append(room.y2 - Room.WallOffset))
                    {
                        tiles[i, j].IsOpen = true;
                    }
                }

                return;
            }
        }
        if (player.Position.X != Settings.TileWidth - 1)
        {
            tiles[player.Position.X + 1, player.Position.Y].IsOpen = true;
        }
        if (player.Position.X != 0)
        {
            tiles[player.Position.X - 1, player.Position.Y].IsOpen = true;
        }
        if (player.Position.Y != Settings.TileHeight - 1)
        {
            tiles[player.Position.X, player.Position.Y + 1].IsOpen = true;
        }
        if (player.Position.Y != 0)
        {
            tiles[player.Position.X, player.Position.Y - 1].IsOpen = true;
        }
    }
}


public static class Game
{
    static GameState State = new();
    static public void Update()
    {
        State.HandleInput();

        Enigmatologist.UpdateFogOfWar(GameStorage.Player, GameStorage.Rooms, GameStorage.Tiles);

        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.BLACK);

            using (Artist.World2DEnvironment(GameStorage.Camera))
            {
                Artist.DrawDungeon(GameStorage.Tiles);
                Artist.DrawInteractiveObjects(GameStorage.InteractiveObjects, GameStorage.Tiles);
                Artist.DrawActor(GameStorage.Player);
                foreach (IMonster monster in GameStorage.Monsters)
                {
                    Artist.DrawActor(monster);
                }
            }
            Artist.DrawStatusBar();
        }
    }

    static public void Begin()
    {
        Settings.Resolution.InitWindow("RoguePound");
        GameStorage.Reset();
    }
}
