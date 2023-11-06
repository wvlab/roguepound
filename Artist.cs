using System.Numerics;
using Raylib_CsLo;

namespace RoguePound;

public interface IArtist
{
    void DrawDungeon(in ITile[,] DungeonTiles);
    IDisposable DrawingEnvironment();
    IDisposable World2DEnvironment(Camera2D Camera);
}

public class ArtistPermissionException : Exception
{
    public ArtistPermissionException(string message) : base(message) { }
}


public class Artist : IArtist
{
    Permissions Perms = new Permissions();

    private class Permissions
    {
        public bool isAllowedToDraw = false;
        public bool isIn2DWorld = false;
    }

    private void CheckPermissionToDraw()
    {
        if (!Perms.isAllowedToDraw) throw new ArtistPermissionException("method must be used in DrawingEnvironment");
    }

    private void CheckPermissionToDraw2D()
    {
        if (!Perms.isAllowedToDraw) throw new ArtistPermissionException("method must be used in World2DEnvironment");
    }

    public IDisposable DrawingEnvironment() => new _DrawingEnvironment(Perms);

    private class _DrawingEnvironment : IDisposable
    {
        private Permissions Permissions;
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
                    Permissions.isAllowedToDraw = false;
                    Raylib.EndDrawing();
                }

                _disposedValue = true;
            }
        }

        public _DrawingEnvironment(Permissions permissions)
        {
            Permissions = permissions;
            Permissions.isAllowedToDraw = true;
            Raylib.BeginDrawing();
        }
    }

    public IDisposable World2DEnvironment(Camera2D cam) =>  new _World2DEnvironment(Perms, cam);

    private class _World2DEnvironment : IDisposable
    {
        private Permissions Permissions;
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
                    Permissions.isIn2DWorld = false;
                    Raylib.EndMode2D();
                }

                _disposedValue = true;
            }
        }

        public _World2DEnvironment(Permissions permissions, Camera2D Camera)
        {
            Permissions = permissions;
            Permissions.isIn2DWorld = true;
            Raylib.BeginMode2D(Camera);
        }
    }

    public void DrawDungeon(in ITile[,] dungeonTiles)
    {
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();

        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Vector2 vec = new Vector2(
                    x * Settings.TileSize,
                    y * Settings.TileSize
                );
                dungeonTiles[x, y].Draw(vec);
            }
        }
    }

    public void DrawGrid()
    {
        CheckPermissionToDraw();
        CheckPermissionToDraw2D();

        for (int i = 0; i < Settings.TileWidth + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(Settings.TileSize * i, 0),
                new Vector2(Settings.TileSize * i, Settings.TileHeight * Settings.TileSize),
                Raylib.LIGHTGRAY
            );
        }

        for (int i = 0; i < Settings.TileHeight + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(0, Settings.TileSize * i),
                new Vector2(Settings.TileWidth * Settings.TileSize, Settings.TileSize * i),
                Raylib.LIGHTGRAY
            );
        }
    }
}
