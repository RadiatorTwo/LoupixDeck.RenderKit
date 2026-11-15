using LoupixDeck.PluginSdk;
using LoupixDeck.RenderKit.Pixel;

namespace LoupixDeck.RenderKit.Tiles;

/// <summary>
/// The render path for pixel tiles: draw into a fresh <see cref="PixelSurface"/>, send it to the
/// host canvas and report the frame's content hash, so the host pushes to the device only when a
/// pixel actually changed.
/// </summary>
public static class PixelTile
{
    /// <summary>
    /// Redraw rate. Values change about once a second and the critical blink runs at 1 Hz; 4 fps
    /// keeps a page switch after a key press under 250 ms. Unchanged frames cost no device I/O.
    /// </summary>
    public const int TargetFps = 4;

    /// <summary>Draws one frame with <paramref name="draw"/> and presents it on <paramref name="canvas"/>.</summary>
    /// <param name="transparent">Draw on <see cref="PixelPalette.Transparent"/> instead of <see cref="PixelPalette.Default"/>.</param>
    public static AnimationFrameInfo Render(CommandContext ctx, IRenderCanvas canvas, bool transparent, Action<PixelSurface> draw)
    {
        PixelSurface surface = new(canvas.Width, canvas.Height,
            transparent ? PixelPalette.Transparent : PixelPalette.Default);

        draw(surface);
        PixelPresenter.Present(surface, canvas);
        return AnimationFrameInfo.Frame(surface.ContentHash());
    }

    /// <summary>The blink phase for the poll-driven path, which has no frame clock.</summary>
    public static bool WallClockBlink() => TileDrawing.BlinkOn(TimeSpan.FromMilliseconds(Environment.TickCount64));
}
