using LoupixDeck.RenderKit.Pixel;

namespace LoupixDeck.RenderKit.Tiles;

/// <summary>
/// Building blocks shared by tile layouts, ported from the hardware-display design prototype
/// (hw-screen.js) via the Argus plugin: state colors, the header band, the hero value, the gauge
/// bar and the history chart. All coordinates are design coordinates (content grid
/// <see cref="PixelSurface.Left"/>..<see cref="PixelSurface.Right"/>).
/// </summary>
public static class TileDrawing
{
    /// <summary>Left edge of the content grid.</summary>
    public const int L = PixelSurface.Left;

    /// <summary>Right edge of the content grid.</summary>
    public const int R = PixelSurface.Right;

    /// <summary>Width of the content grid.</summary>
    public const int W = PixelSurface.ContentSize;

    /// <summary>Header band: rows 8–16 across the 74-px window.</summary>
    public const int HeaderX = 8;

    /// <summary>Top row of the header band.</summary>
    public const int HeaderY = 8;

    /// <summary>Width of the header band.</summary>
    public const int HeaderWidth = 74;

    /// <summary>Height of the header band.</summary>
    public const int HeaderHeight = 9;

    /// <summary>The blink phase of a critical alert (1 Hz, on for 500 ms).</summary>
    public static bool BlinkOn(TimeSpan elapsed) => ((long)(elapsed.TotalMilliseconds / 500) % 2) == 0;

    /// <summary>The text color for a value in <paramref name="state"/>.</summary>
    public static uint StateColor(PixelPalette palette, TileState state) => state switch
    {
        TileState.Critical => palette.Critical,
        TileState.Warn => palette.Warn,
        _ => palette.Text
    };

    /// <summary>The most severe of <paramref name="states"/>; <see cref="TileState.Ok"/> when empty.</summary>
    public static TileState Worst(IEnumerable<TileState> states)
    {
        TileState worst = TileState.Ok;
        foreach (TileState state in states)
        {
            if (state > worst)
                worst = state;
        }

        return worst;
    }

    /// <summary>
    /// The header band with <paramref name="title"/> left and <paramref name="index"/> right. Warn
    /// fills it amber; critical fills it red on the blink's on phase and frames it in red on the off
    /// phase. Colour is never the only cue: a critical header shows "!" instead of the index.
    /// </summary>
    public static void Header(PixelSurface surface, string title, string? index, TileState state, bool blinkOn)
    {
        PixelPalette p = surface.Palette;
        bool alert = state != TileState.Ok;
        bool filled = state == TileState.Warn || (state == TileState.Critical && blinkOn);

        uint background = alert ? (filled ? StateColor(p, state) : p.Background) : p.Track;
        uint text = alert ? (filled ? p.Ink : p.Critical) : p.Text;

        surface.Fill(HeaderX, HeaderY, HeaderWidth, HeaderHeight, background);
        if (alert && !filled)
        {
            surface.Fill(HeaderX, HeaderY, HeaderWidth, 1, p.Critical);
            surface.Fill(HeaderX, HeaderY + HeaderHeight - 1, HeaderWidth, 1, p.Critical);
        }

        string right = alert ? "!" : index ?? string.Empty;
        int titleRoom = W - 2 - (right.Length > 0 ? PixelFont.Measure(right) + 3 : 0);
        surface.Text(Fit(title, titleRoom), L + 1, 9, text, shadow: !filled);
        if (right.Length > 0)
            surface.TextRight(right, R - 1, 9, text, shadow: !filled);
    }

    /// <summary>
    /// Draws a value at the largest scale (at most <paramref name="maxScale"/>) that leaves room for
    /// a <paramref name="sideWidth"/>-px column to its right, and returns where that column starts.
    /// </summary>
    public static (int SideX, int Scale) HeroValue(PixelSurface surface, string value, int y, uint color,
        int maxScale, int sideWidth)
    {
        int scale = maxScale;
        while (scale > 1 && PixelFont.Measure(value, scale) + 3 + sideWidth > W)
            scale--;

        surface.Text(value, L, y, color, scale);
        return (L + PixelFont.Measure(value, scale) + 3, scale);
    }

    /// <summary>
    /// Gauge bar across the content width filled to <paramref name="fraction"/> (clamped to 0..1,
    /// NaN counts as 0; at least one pixel is always filled), with a 1-px amber tick at
    /// <paramref name="warnFraction"/> when given.
    /// </summary>
    public static void Bar(PixelSurface surface, double fraction, TileState state, int y, int height,
        double? warnFraction = null)
    {
        double clamped = double.IsNaN(fraction) ? 0.0 : Math.Clamp(fraction, 0.0, 1.0);

        PixelPalette p = surface.Palette;
        surface.Fill(L, y, W, height, p.Track);
        uint fill = state == TileState.Ok ? p.Accent : StateColor(p, state);
        surface.Fill(L, y, Math.Max(1, (int)Math.Round(W * clamped)), height, fill);

        if (warnFraction is { } warn && !double.IsNaN(warn))
        {
            int x = L + (int)Math.Round(W * warn);
            if (x >= L && x <= R)
                surface.Fill(x, y - 1, 1, height + 2, p.Warn);
        }
    }

    /// <summary>
    /// History chart, one column per sample, newest at the right edge. Auto-scaled, but never to
    /// less than 12 % of the range <paramref name="min"/>..<paramref name="max"/> (or of the peak
    /// when there is no range), so noise does not look like a spike. NaN samples leave a gap.
    /// </summary>
    public static void Spark(PixelSurface surface, ReadOnlySpan<double> history, double? min, double? max,
        int y, int height)
    {
        if (height < 2)
            return;

        bool hasRange = min is not null && max is not null && max.Value > min.Value;
        double range = hasRange ? max!.Value - min!.Value : 0.0;

        int count = Math.Min(history.Length, W);
        int first = history.Length - count;

        double lo = double.MaxValue;
        double hi = double.MinValue;
        for (int i = first; i < history.Length; i++)
        {
            if (double.IsNaN(history[i]))
                continue;
            lo = Math.Min(lo, history[i]);
            hi = Math.Max(hi, history[i]);
        }

        if (lo > hi)
            return;

        double minSpan = hasRange ? range * 0.12 : Math.Abs(hi) * 0.12;
        minSpan = Math.Max(minSpan, 1e-9);
        if (hi - lo < minSpan)
        {
            if (hasRange)
            {
                double mid = (hi + lo) / 2;
                lo = mid - (minSpan / 2);
            }

            hi = lo + minSpan;
        }

        PixelPalette p = surface.Palette;
        int x0 = L + (W - count);
        for (int i = 0; i < count; i++)
        {
            double value = history[first + i];
            if (double.IsNaN(value))
                continue;

            int barHeight = Math.Max(1, (int)Math.Round((value - lo) / (hi - lo) * (height - 1)) + 1);
            barHeight = Math.Min(barHeight, height);
            surface.Fill(x0 + i, y + height - barHeight, 1, barHeight, p.Spark);
            surface.Fill(x0 + i, y + height - barHeight, 1, 1, p.Accent);
        }
    }

    /// <summary>Cuts <paramref name="text"/> from the end until it fits <paramref name="maxWidth"/>
    /// at <paramref name="scale"/>.</summary>
    public static string Fit(string text, int maxWidth, int scale = 1, int spacing = -1)
    {
        string fitted = text.Trim();
        while (fitted.Length > 0 && PixelFont.Measure(fitted, scale, spacing) > maxWidth)
            fitted = fitted[..^1].TrimEnd();

        return fitted;
    }

    /// <summary>A tile with nothing to show: header, a dim "N/A" and two explanatory lines.</summary>
    public static void Placeholder(PixelSurface surface, string title, string line1, string line2)
    {
        PixelPalette p = surface.Palette;
        Header(surface, title, null, TileState.Ok, blinkOn: true);
        surface.Text("N/A", L, 21, p.Dim, 3);
        surface.Text(line1, L, 52, p.Dim);
        surface.Text(line2, L, 62, p.Dim);
    }

    /// <summary>The source runs but reports nothing for this tile.</summary>
    public static void NoData(PixelSurface surface, string title) =>
        Placeholder(surface, title, "NO DATA", string.Empty);
}
