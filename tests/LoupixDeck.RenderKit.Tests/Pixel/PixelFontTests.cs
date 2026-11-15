using LoupixDeck.RenderKit.Pixel;

namespace LoupixDeck.RenderKit.Tests.Pixel;

public sealed class PixelFontTests
{
    [Fact]
    public void Measure_UnknownGlyph_UsesFallbackWidth() =>
        Assert.Equal(PixelFont.Measure("?"), PixelFont.Measure("€"));

    [Fact]
    public void Measure_Lowercase_EqualsUppercase() =>
        Assert.Equal(PixelFont.Measure("A"), PixelFont.Measure("a"));

    [Fact]
    public void Glyph_DoesNotExposeMutableRows() =>
        Assert.DoesNotContain(typeof(PixelFont.Glyph).GetProperties(), p => p.PropertyType == typeof(int[]));
}
