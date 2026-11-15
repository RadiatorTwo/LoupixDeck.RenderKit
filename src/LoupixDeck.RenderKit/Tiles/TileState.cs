namespace LoupixDeck.RenderKit.Tiles;

/// <summary>Alert level of a tile value. Ordered, so the worst of several is their maximum.</summary>
public enum TileState
{
    /// <summary>Normal.</summary>
    Ok = 0,

    /// <summary>Above the warn limit: amber.</summary>
    Warn = 1,

    /// <summary>Above the critical limit: red, blinking.</summary>
    Critical = 2
}
