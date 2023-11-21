using System.Numerics;
using Raylib_CsLo;

namespace RoguePound.Util;

static class Draw
{
    static public void TextCentered(Vector2 destination, string text)
    {
        destination.X -= Raylib.MeasureText(text, Settings.TileSize) / 2;

        Raylib.DrawText(
            text,
            destination.X,
            destination.Y,
            Settings.TileSize,
            Raylib.WHITE
        );
    }
}
