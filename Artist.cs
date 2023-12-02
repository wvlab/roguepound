using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;

public interface IArtist
{
    void DrawDungeon(in Tile[,] DungeonTiles);
    IDisposable DrawingEnvironment();
    IDisposable World2DEnvironment(Camera2D Camera);
}

public class ArtistPermissionException : Exception
{
    public ArtistPermissionException(string message) : base(message) { }
}


public class Artist : IArtist
{
    private ITileSet TileSet = new ClassicTileSet();
    private class Permission
    {
        public bool isAllowed { get; set; } = false;
    }

    private Permission permissionToDraw = new Permission();
    private Permission permissionToDraw2D = new Permission();

    private void CheckPermissionToDraw()
    {
        if (!permissionToDraw.isAllowed) throw new ArtistPermissionException("method must be used in DrawingEnvironment");
    }

    private void CheckPermissionToDraw2D()
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

    public IDisposable DrawingEnvironment() => new PermissionEnvironment(permissionToDraw, Raylib.BeginDrawing, Raylib.EndDrawing);
    public IDisposable World2DEnvironment(Camera2D cam) => new PermissionEnvironment(permissionToDraw2D, () => Raylib.BeginMode2D(cam), Raylib.EndMode2D);

    public void DrawDungeon(in Tile[,] dungeonTiles)
    {
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();

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

    public void DrawGrid()
    {
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();

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

    private void ClearCell(Position position)
    {
        Raylib.DrawRectangleV(position.ToVector2 * Settings.TileSize, new(Settings.TileSize, Settings.TileSize), Raylib.BLACK);
    }

    public void DrawActor(IActor actor)
    {
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();

        ClearCell(actor.Position);
        FDraw.TextCentered(
            actor.Letter,
            (actor.Position.ToVector2 + new Vector2(0.5f, 0)) * new Vector2(Settings.TileSize),
            Settings.TileSize,
            Raylib.WHITE
        ).Invoke(null);
    }

    public void DrawInteractiveObjects(in IEnumerable<InteractiveObject> interactiveObjects, in Tile[,] tiles)
    {
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
}
