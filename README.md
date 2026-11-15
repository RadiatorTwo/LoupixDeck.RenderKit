# LoupixDeck.RenderKit

Pixel-exact tile rendering for [LoupixDeck](https://github.com/RadiatorTwo/LoupixDeck) plugins.

Small numbers on a 90×90 key stay readable when every stroke lands on the device grid. RenderKit
draws with a 5×7 bitmap font at whole scales only, into a plugin-owned ARGB framebuffer laid out on
a centred 72×72 content grid, and never anti-aliases. It also carries the tile building blocks the
hardware plugins share: the header band, the hero value, the gauge bar and the history chart.

## Use

```xml
<PackageReference Include="LoupixDeck.RenderKit" Version="1.0.0" />
```

Ship the RenderKit DLL with your plugin (the default; `loupix pack` includes it). Every plugin
loads its own copy, so plugins can use different RenderKit versions side by side.

```csharp
using LoupixDeck.RenderKit.Pixel;
using LoupixDeck.RenderKit.Tiles;

public AnimationFrameInfo RenderAnimatedFrame(CommandContext ctx, IRenderCanvas canvas, AnimationFrameContext frame) =>
    PixelTile.Render(ctx, canvas, transparent: false, surface =>
    {
        TileDrawing.Header(surface, "MOUSE", "1/2", TileState.Ok, TileDrawing.BlinkOn(frame.Elapsed));
        surface.Text("98", TileDrawing.L, 21, surface.Palette.Text, scale: 3);
    });
```

## License

MIT

## Releasing

1. Bump `<Version>` in `src/LoupixDeck.RenderKit/LoupixDeck.RenderKit.csproj`.
2. Publish a GitHub release tagged `v<version>`. `nuget-release.yml` checks the tag against the
   version, runs the tests and pushes the package to nuget.org.

nuget.org needs a Trusted Publishing policy for this repository before the first release (see the
header of `.github/workflows/nuget-release.yml`). A manual run of the workflow packs without
pushing.

For local plugin development, `dotnet pack src/LoupixDeck.RenderKit -c Release` writes the package
to `nupkg/`; point a plugin's `nuget.config` at that folder.
