using LoupixDeck.RenderKit.Pixel;

namespace LoupixDeck.RenderKit.Tests.Pixel;

/// <summary>
/// Golden hashes produced once with the original Argus renderer (LoupixDeck.Plugin.Argus,
/// Rendering/Pixel, master as of 2026-10-04). A mismatch means RenderKit no longer draws the
/// same pixels as the plugins it replaces.
/// </summary>
public sealed class PixelSurfaceTests
{
    private static readonly PixelPalette D = PixelPalette.Default;
    private static readonly PixelPalette T = PixelPalette.Transparent;

    private static long Hash(int width, int height, PixelPalette palette, Action<PixelSurface> draw)
    {
        PixelSurface surface = new(width, height, palette);
        draw(surface);
        return surface.ContentHash();
    }

    [Fact]
    public void GoldenHash_S1_PlainText() =>
        Assert.Equal(1571064102502811103L, Hash(90, 90, D, s => s.Text("CPU 42%", 9, 9, D.Text)));

    [Fact]
    public void GoldenHash_S2_ScaledDigits() =>
        Assert.Equal(5557809652007858241L, Hash(90, 90, D, s => s.Text("0123456789", 9, 21, D.Accent, scale: 3)));

    [Fact]
    public void GoldenHash_S3_SmallCanvasTransparentUmlauts() =>
        Assert.Equal(282494464677848807L, Hash(74, 74, T, s =>
        {
            s.Fill(9, 45, 72, 3, T.Track);
            s.TextRight("ÄÖÜ?", 80, 62, T.Dim);
        }));

    [Fact]
    public void GoldenHash_S4_LowercaseWithoutShadow() =>
        Assert.Equal(6158962940684787299L, Hash(90, 90, D, s => s.Text("abc xyz", 9, 30, D.Warn, scale: 2, shadow: false)));

    [Fact]
    public void Surface_74x74_ClipsWithoutThrowing()
    {
        PixelSurface surface = new(74, 74, D);
        surface.Fill(-5, -5, 200, 200, D.Text);
        surface.Text("OUTSIDE THE GRID", 70, 70, D.Text, scale: 3);
        Assert.Equal(74 * 74, surface.Pixels.Length);
    }
}
