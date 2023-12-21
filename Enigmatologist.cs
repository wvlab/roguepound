using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;

static class Enigmatologist
{
    public static void UpdateFogOfWar()
    {
        (int posX, int posY) = (GameStorage.Player.Position.X, GameStorage.Player.Position.Y);
        foreach (Room room in GameStorage.Rooms)
        {
            if (!room.IsInRoom(posX, posY))
            {
                continue;
            }

            if (!GameStorage.Tiles[room.x1 + Room.WallOffset + 1, room.y1 + Room.WallOffset + 1].IsOpen)
            {
                foreach (int i in room.HWallXCoords().Append(room.x1 + Room.WallOffset).Append(room.x2 - Room.WallOffset))
                {
                    foreach (int j in room.VWallYCoords().Append(room.y1 + Room.WallOffset).Append(room.y2 - Room.WallOffset))
                    {
                        GameStorage.Tiles[i, j].IsOpen = true;
                    }
                }

                return;
            }
        }
        if (GameStorage.Player.Position.X != Settings.TileWidth - 1)
        {
            GameStorage.Tiles[GameStorage.Player.Position.X + 1, GameStorage.Player.Position.Y].IsOpen = true;
        }
        if (GameStorage.Player.Position.X != 0)
        {
            GameStorage.Tiles[GameStorage.Player.Position.X - 1, GameStorage.Player.Position.Y].IsOpen = true;
        }
        if (GameStorage.Player.Position.Y != Settings.TileHeight - 1)
        {
            GameStorage.Tiles[GameStorage.Player.Position.X, GameStorage.Player.Position.Y + 1].IsOpen = true;
        }
        if (GameStorage.Player.Position.Y != 0)
        {
            GameStorage.Tiles[GameStorage.Player.Position.X, GameStorage.Player.Position.Y - 1].IsOpen = true;
        }
    }

    const int Feeling = 7;
    public static void UpdateMonsters()
    {
        MonsterData[] mDatas = GameStorage.Monsters.ToArray();
        foreach (MonsterData mData in mDatas)
        {
            IMonster monster = mData.Monster;

            if (monster.Stats.Health <= 0)
            {
                GameStorage.Gold += monster.Gold;
                GameStorage.Player.Experience += monster.Experience;

                GameStorage.Monsters.RemoveAll(md => md.Monster == monster);
                continue;
            }

            (int x, int y) playerPos = GameStorage.Player.Position.ToTuple;
            (int x, int y) monsterPos = monster.Position.ToTuple;

            if (mData.LastPlayerPosition != playerPos)
            {
                if (FMath.EuclideanDistance(monsterPos.x, monsterPos.y, playerPos.x, playerPos.y) < Feeling)
                {
                    AStar.FindPath(monsterPos, playerPos, mData.Path);
                }
            }

            if (playerPos.x - monsterPos.x >= -1 && playerPos.x - monsterPos.x <= 1 && playerPos.y - monsterPos.y >= -1 && playerPos.y - monsterPos.y <= 1)
            {
                ActorScene.Attack(monster, GameStorage.Player);
            }
            else if (mData.Path.Count > 0)
            {
                (int cx, int cy) = mData.Path[0];
                ActorScene.Teleport(monster, cx, cy);
                mData.Path.RemoveAt(0);
            }
        }
    }

    // https://nethackwiki.com/wiki/Rogue_(game)#Experience_Levels
    private static int CalculateNewLevelCap(short level) => level switch
    {
        (< 1) => throw new ArgumentException("How have you reached level less than 1?"),
        2 => 10,
        3 => 20,
        4 => 40,
        5 => 80,
        6 => 160,
        7 => 320,
        8 => 640,
        9 => 1300,
        10 => 2600,
        11 => 5200,
        12 => 13_000,
        13 => 26_000,
        14 => 50_000,
        15 => 100_000,
        16 => 200_000,
        17 => 400_000,
        18 => 800_000,
        19 => 2_000_000,
        20 => 4_000_000,
        21 => 8_000_000,
        _ => throw new ArgumentException("Eh can't find new level"),
    };

    public static void CheckLevelup()
    {
        Player player = GameStorage.Player;

        while (player.Level >= Settings.LevelCap && player.Experience > player.ExperienceCap)
        {
            player.Stats.MaxHealth += 2;
            player.Stats.Health = player.Stats.MaxHealth;
            player.Level += 1;
            player.Experience -= player.ExperienceCap;
            player.ExperienceCap = CalculateNewLevelCap(player.Level);
        }
    }
}
