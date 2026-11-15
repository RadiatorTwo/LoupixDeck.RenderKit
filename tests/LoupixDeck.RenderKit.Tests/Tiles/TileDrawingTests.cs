using LoupixDeck.RenderKit.Pixel;
using LoupixDeck.RenderKit.Tiles;

namespace LoupixDeck.RenderKit.Tests.Tiles;

/// <summary>
/// Golden hashes produced once with the original Argus TileDrawing (LoupixDeck.Plugin.Argus,
/// Rendering/Tiles, master as of 2026-10-04), fed with equivalent metric snapshots.
/// </summary>
public sealed class TileDrawingTests
{
    private static readonly PixelPalette D = PixelPalette.Default;

    private static long Hash(Action<PixelSurface> draw)
    {
        PixelSurface surface = new(90, 90, D);
        draw(surface);
        return surface.ContentHash();
    }

    private static double[] Wave() => Enumerable.Range(0, 72).Select(i => (Math.Sin(i / 5.0) * 50) + 50).ToArray();

    [Fact]
    public void GoldenHash_T1_HeaderOkWithIndex() =>
        Assert.Equal(1560302870765860107L, Hash(s => TileDrawing.Header(s, "GPU", "2/5", TileState.Ok, true)));

    [Fact]
    public void GoldenHash_T2_HeaderWarnLongTitle() =>
        Assert.Equal(8120235310939627623L, Hash(s => TileDrawing.Header(s, "TEMPERATURE SENSOR", null, TileState.Warn, true)));

    [Fact]
    public void GoldenHash_T3_HeaderCriticalBlinkOff() =>
        Assert.Equal(3450880075281416429L, Hash(s => TileDrawing.Header(s, "CPU", "1/3", TileState.Critical, false)));

    [Fact]
    public void GoldenHash_T4_HeroValue() =>
        Assert.Equal(7938305246455337145L, Hash(s => TileDrawing.HeroValue(s, "98", 21, D.Text, 3, 20)));

    [Fact]
    public void GoldenHash_T5_BarOk() =>
        Assert.Equal(4217094206664077053L, Hash(s => TileDrawing.Bar(s, 0.42, TileState.Ok, 45, 3)));

    [Fact]
    public void GoldenHash_T6_BarCriticalWithWarnTick() =>
        Assert.Equal(5328284808978900615L, Hash(s => TileDrawing.Bar(s, 0.9, TileState.Critical, 45, 3, warnFraction: 0.8)));

    [Fact]
    public void GoldenHash_T7_NoData() =>
        Assert.Equal(38939855657481303L, Hash(s => TileDrawing.NoData(s, "NET")));

    [Fact]
    public void GoldenHash_T8_Spark() =>
        Assert.Equal(6462051614337994487L, Hash(s => TileDrawing.Spark(s, Wave(), 0, 100, 52, 28)));

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    public void Bar_FractionBelowRangeOrNaN_DrawsLikeZero(double fraction) =>
        Assert.Equal(7408994538591946899L, Hash(s => TileDrawing.Bar(s, fraction, TileState.Ok, 45, 3)));

    [Fact]
    public void Bar_FractionAboveRange_DrawsLikeFull() =>
        Assert.Equal(1086092417109317127L, Hash(s => TileDrawing.Bar(s, 1.7, TileState.Ok, 45, 3)));

    [Fact]
    public void Fit_TooLong_TrimsToWidth() =>
        Assert.True(PixelFont.Measure(TileDrawing.Fit("A VERY LONG DEVICE NAME", 40)) <= 40);

    [Fact]
    public void Header_EmptyTitle_DoesNotThrow()
    {
        PixelSurface surface = new(90, 90, D);
        TileDrawing.Header(surface, string.Empty, null, TileState.Ok, true);
    }

    [Fact]
    public void Header_NullTitle_DoesNotThrow()
    {
        PixelSurface surface = new(90, 90, D);
        TileDrawing.Header(surface, null!, null, TileState.Ok, true);
    }

    [Fact]
    public void Fit_Null_ReturnsEmpty() => Assert.Equal(string.Empty, TileDrawing.Fit(null!, 40));

    [Fact]
    public void Worst_ReturnsMostSevere() =>
        Assert.Equal(TileState.Critical, TileDrawing.Worst([TileState.Ok, TileState.Critical, TileState.Warn]));
}
