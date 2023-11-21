using System.Numerics;
using Raylib_CsLo;

namespace RoguePound;

public interface ITile
{
    void Draw(Vector2 destination);
}

public interface ITileSet
{
    ITile LTCorner { get; }
    ITile RTCorner { get; }
    ITile LBCorner { get; }
    ITile RBCorner { get; }
    ITile HWall { get; }
    ITile VWall { get; }
    ITile Floor { get; }
    ITile Path { get; }
    ITile TDoor { get; }
    ITile BDoor { get; }
    ITile LDoor { get; }
    ITile RDoor { get; }
}


public readonly record struct VoidTile : ITile
{
    public void Draw(Vector2 _) { }
}


public readonly record struct ColoredTile(Color color) : ITile
{
    public void Draw(Vector2 destination) =>
        Raylib.DrawRectangleV(
            destination,
            new Vector2(Settings.TileSize, Settings.TileSize),
            color
        );
}

public readonly record struct TextTile(string text) : ITile
{
    public void Draw(Vector2 destination) =>
        Util.Draw.TextCentered(
            destination + new Vector2(Settings.TileSize / 2, 0),
            text
        );
}

public readonly record struct DumbTileSet() : ITileSet
{
    public readonly struct DumbLTCorner : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbRTCorner : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbLBCorner : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbRBCorner : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbHWall : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbVWall : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbFloor : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbPath : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbTDoor : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbBDoor : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbLDoor : ITile { public void Draw(Vector2 _) { } }
    public readonly struct DumbRDoor : ITile { public void Draw(Vector2 _) { } }

    public ITile LTCorner { get; } = new DumbLTCorner();
    public ITile RTCorner { get; } = new DumbRTCorner();
    public ITile LBCorner { get; } = new DumbLBCorner();
    public ITile RBCorner { get; } = new DumbRBCorner();
    public ITile HWall { get; } = new DumbHWall();
    public ITile VWall { get; } = new DumbVWall();
    public ITile Floor { get; } = new DumbFloor();
    public ITile Path { get; } = new DumbPath();
    public ITile TDoor { get; } = new DumbTDoor();
    public ITile BDoor { get; } = new DumbBDoor();
    public ITile LDoor { get; } = new DumbLDoor();
    public ITile RDoor { get; } = new DumbRDoor();
}

public readonly record struct TestingTileSet() : ITileSet
{
    public ITile LTCorner { get; } = new ColoredTile(Raylib.LIGHTGRAY);
    public ITile RTCorner { get; } = new ColoredTile(Raylib.DARKGRAY);
    public ITile LBCorner { get; } = new ColoredTile(Raylib.YELLOW);
    public ITile RBCorner { get; } = new ColoredTile(Raylib.ORANGE);
    public ITile HWall { get; } = new ColoredTile(Raylib.RED);
    public ITile VWall { get; } = new ColoredTile(Raylib.MAROON);
    public ITile Floor { get; } = new ColoredTile(Raylib.RAYWHITE);
    public ITile Path { get; } = new ColoredTile(Raylib.WHITE);
    public ITile TDoor { get; } = new ColoredTile(Raylib.BROWN);
    public ITile BDoor { get; } = new ColoredTile(Raylib.DARKBROWN);
    public ITile LDoor { get; } = new ColoredTile(Raylib.BROWN);
    public ITile RDoor { get; } = new ColoredTile(Raylib.DARKBROWN);
}

public readonly record struct ClassicTileSet() : ITileSet
{
    public ITile LTCorner { get; } = new TextTile("+");
    public ITile RTCorner { get; } = new TextTile("+");
    public ITile LBCorner { get; } = new TextTile("+");
    public ITile RBCorner { get; } = new TextTile("+");
    public ITile HWall { get; } = new TextTile("--");
    public ITile VWall { get; } = new TextTile("|");
    public ITile Floor { get; } = new TextTile(".");
    public ITile Path { get; } = new TextTile("#");
    public ITile TDoor { get; } = new TextTile("/");
    public ITile BDoor { get; } = new TextTile("\\");
    public ITile LDoor { get; } = new TextTile("\\");
    public ITile RDoor { get; } = new TextTile("/");
}
