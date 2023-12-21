using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;

public class ArtistPermissionException : Exception
{
    public ArtistPermissionException(string message) : base(message) { }
}


static public class Artist
{
    const int StatusBarHeight = 36;

    static private ITileSet TileSet = new ClassicTileSet();
    private class Permission
    {
        public bool isAllowed { get; set; } = false;
    }

    static private Permission permissionToDraw = new Permission();
    static private Permission permissionToDraw2D = new Permission();

    static private void CheckPermissionToDraw()
    {
        if (!permissionToDraw.isAllowed) throw new ArtistPermissionException("method must be used in DrawingEnvironment");
    }

    static private void CheckPermissionToDraw2D()
    {
        if (!permissionToDraw2D.isAllowed) throw new ArtistPermissionException("method must be used in World2DEnvironment");
    }

    private class PermissionEnvironment : IDisposable
    {
        private Permission Permission;
        private Action DisposeAction;
        private bool _disposedValue;

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Permission.isAllowed = false;
                    DisposeAction();
                }

                _disposedValue = true;
            }
        }

        public PermissionEnvironment(Permission permission, Action startAction, Action disposeAction)
        {
            Permission = permission;
            Permission.isAllowed = true;
            DisposeAction = disposeAction;
            startAction();
        }
    }

    static public IDisposable DrawingEnvironment() => new PermissionEnvironment(permissionToDraw, Raylib.BeginDrawing, Raylib.EndDrawing);
    static public IDisposable World2DEnvironment(Camera2D cam) => new PermissionEnvironment(permissionToDraw2D, () => Raylib.BeginMode2D(cam), Raylib.EndMode2D);

    static public void DrawDungeon(in Tile[,] dungeonTiles)
    {
#if DEBUG
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();
#endif
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Tile tile = dungeonTiles[x, y];
                if (tile.IsOpen)
                {
                    Vector2 vec = new Vector2(
                        x * Settings.TileSize,
                        y * Settings.TileSize
                    );

                    TileSet.DrawTile(dungeonTiles[x, y], vec + new Vector2(Settings.TileSize / 2, 0));
                }
            }
        }
    }

    static public void DrawGrid()
    {
#if DEBUG
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();
#endif
        Color col = Raylib.PURPLE;
        for (int i = 0; i < Settings.TileWidth + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(Settings.TileSize * i, 0),
                new Vector2(Settings.TileSize * i, Settings.TileHeight * Settings.TileSize),
                col
            );
        }

        for (int i = 0; i < Settings.TileHeight + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(0, Settings.TileSize * i),
                new Vector2(Settings.TileWidth * Settings.TileSize, Settings.TileSize * i),
                col
            );
        }
    }

    static private void ClearCell(Position position)
    {
#if DEBUG
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();
#endif
        Raylib.DrawRectangleV(position.ToVector2 * Settings.TileSize, new(Settings.TileSize, Settings.TileSize), Raylib.BLACK);
    }

    static public void DrawActor(IActor actor)
    {
#if DEBUG
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();
#endif
        if (GameStorage.Tiles[actor.Position.X, actor.Position.Y].IsOpen)
        {
            ClearCell(actor.Position);
            FDraw.TextCentered(
                actor.Letter,
                (actor.Position.ToVector2 + new Vector2(0.5f, 0)) * new Vector2(Settings.TileSize),
                Settings.TileSize,
                Raylib.WHITE
            ).Invoke(null);
        }
    }

    static public void DrawStatusBar()
    {
#if DEBUG
        CheckPermissionToDraw();
#endif

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        Raylib.DrawRectangle(
            0,
            screenHeight - StatusBarHeight,
            screenWidth,
            StatusBarHeight,
            Raylib.BLACK
        );

        FDraw.TextCentered(
            $"[LVL:{GameStorage.Player.Level}] " +
            $"[Coins:{GameStorage.Gold}] " +
            $"[HP:{GameStorage.Player.Stats.Health}] " +
            $"[ATK:{GameStorage.Player.Stats.Attack}] " +
            $"[ARM:{GameStorage.Player.Stats.Armor}] " +
            $"[EXP:{GameStorage.Player.Experience}/{GameStorage.Player.ExperienceCap}]",
            new Vector2(screenWidth / 2, screenHeight - StatusBarHeight),
            StatusBarHeight - 8,
            Raylib.WHITE
        ).Invoke(null);
    }

    static public void DrawInteractiveObjects(in IEnumerable<InteractiveObject> interactiveObjects, in Tile[,] tiles)
    {
#if DEBUG
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();
#endif
        foreach (var obj in interactiveObjects)
        {
            (int posX, int posY) = (obj.Position.X, obj.Position.Y);
            if (tiles[posX, posY].IsOpen)
            {
                ClearCell(obj.Position);
                FDraw.TextCentered(
                    obj.Letter,
                    (obj.Position.ToVector2 + new Vector2(0.5f, 0)) * new Vector2(Settings.TileSize),
                    Settings.TileSize,
                    Raylib.WHITE
                ).Invoke(null);
            }
        }
    }

    static public void DrawGame()
    {
        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.BLACK);

            using (Artist.World2DEnvironment(GameStorage.Camera))
            {
                Artist.DrawDungeon(GameStorage.Tiles);
                Artist.DrawInteractiveObjects(GameStorage.InteractiveObjects, GameStorage.Tiles);
                Artist.DrawActor(GameStorage.Player);
                foreach (MonsterData MData in GameStorage.Monsters)
                {
                    Artist.DrawActor(MData.Monster);
                }
            }
            Artist.DrawStatusBar();
        }
    }

    static public void DrawGameOver()
    {
        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.RAYWHITE);

            FDraw.TextCentered(
                $"Game is over... You got to downward spiral\nYou collected {GameStorage.Gold} gold",
                new(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2 - Settings.TileSize),
                Settings.TileSize,
                Raylib.BLACK
            ).Invoke(null);
        }
    }
}
