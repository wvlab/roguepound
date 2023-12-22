using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound.Dungeon;

internal static class MainFrame
{
    static public void PlaceMonsters()
    {
        int monstersAmount = CalculateMonstersAmount();
        short[] monstersAmountPerRoom = new short[GameStorage.Rooms.Count];
        int monstersPerRoom = (int)Math.Ceiling((double)monstersAmount / GameStorage.Rooms.Count) + 2;
        for (int i = 0; i < monstersAmount; i++)
        {
            int roomIndex;
            Room room;
            while (true)
            {
                roomIndex = GameStorage.Rand.Next(0, GameStorage.Rooms.Count);
                if (monstersAmountPerRoom[roomIndex] < monstersPerRoom)
                {
                    room = GameStorage.Rooms[roomIndex];
                    break;
                }
            }
            Stats stats = Stats.Default;
            IMonster m = CreateRandMonster(stats, 6, 1);
            (m.Position.X, m.Position.Y) = room.RandPos(GameStorage.Rand);
            GameStorage.Monsters.Add(new MonsterData(m));
        }
    }

    static private int CalculateMonstersAmount() => (int)Math.Ceiling(
        GameStorage.Rooms.Count * (GameStorage.DungeonFloor > 5 ? 1 : 0.5) * GameStorage.Rand.Next(1, 4)
        + Math.Min(GameStorage.DungeonFloor, 10)
    );

    static private IMonster CreateRandMonster(Stats stats, short exp, short gold)
    {
        int n = GameStorage.Rand.Next(3);
        if (n == 0)
        {
            return Evil.CreateAquator(stats, exp, gold);
        }
        if (n == 1)
        {
            return Evil.CreateZombie(stats, exp, gold);
        }
        if (n == 2)
        {
            return Evil.CreateSnake(stats, exp, gold);
        }

        throw new Exception("Evil god is dreaming");
    }

    static private Stats CreateStats()
    {
        int delta = GameStorage.DungeonFloor - 10;
        return new Stats(
            health: Math.Min(0, Stats.Default.Health - delta),
            maxHealth: Math.Min(0, Stats.Default.MaxHealth - delta),
            attack: Math.Min(0, Stats.Default.Attack - delta),
            armor: (short)Math.Min(0, Stats.Default.Armor - delta),
            agility: (short)Math.Min(0, Stats.Default.Agility - delta)
        );
    }

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

        List<(int, int)> path = new();
        AStar.FindPath((player.X, player.Y), (posX, posY), path);
        if (path.Count == 0)
        {
            throw new BrokenDungeonException("Does it look like I'm going downward spiral?");
        }
    }
}
