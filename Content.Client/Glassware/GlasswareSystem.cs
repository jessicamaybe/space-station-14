using System.Numerics;
using Content.Shared.Glassware;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    /// <inheritdoc/>m
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareTubeVisualizerComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<GlasswareTubeVisualizerComponent> ent, ref AppearanceChangeEvent ev)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var scale = new Vector2(1.0f, ent.Comp.TubeLength * 8); //I eyeballed 8 and it was the magic number for a tube sprite 4px long

        _spriteSystem.LayerSetScale((ent, sprite), GlasswareTubeLayers.Tube, scale);

        var endpos = ent.Comp.TubeLength / 2;


        //TODO: rotate the one on the end, or start. whatever it fuckin is
        var startCapLayer = _spriteSystem.AddLayer((ent, sprite), new SpriteSpecifier.Rsi(new ResPath("glassware/tube_end.rsi"), "tube_end"));
        _spriteSystem.LayerSetOffset((ent, sprite), startCapLayer, new Vector2(0, endpos));

        var endCapLayer = _spriteSystem.AddLayer((ent, sprite), new SpriteSpecifier.Rsi(new ResPath("glassware/tube_end.rsi"), "tube_end"));
        _spriteSystem.LayerSetOffset((ent, sprite), endCapLayer, new Vector2(0, -endpos));

    }
}
