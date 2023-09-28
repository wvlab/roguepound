using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public interface ITile
{
    void Draw(Vector2 destination);
}

public readonly record struct VoidTile : ITile
{
    public void Draw(Vector2 _) { }
}


public readonly record struct ColoredTile(Color color) : ITile
{
    public void Draw(Vector2 destination)
    {
        Raylib.DrawRectangleV(
            destination,
            new Vector2(Settings.TileSize, Settings.TileSize),
            color
        );
    }
}
