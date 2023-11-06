using FunctionalRoguePound;

namespace RoguePound;

/// <summary>
/// Class <c>Settings</c> keeps all constants and variables from settings file
/// </summary>
public sealed class Settings
{
    public Resolution Resolution;
    public const int TileSize = 32;
    public const int TileHeight = 48;
    public const int TileWidth = 80;
    private static Settings instance = new Settings();

    private Settings()
    {
        Resolution = Resolution.NewWindowed(900, 600);
    }

    public static Settings Instance
    {
        get
        {
            return instance;
        }
    }
}


