using System.Numerics;
using Content.Shared.Glassware;
using Robust.Client.GameObjects;

namespace Content.Client.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareTubeVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    /// <inheritdoc/>
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

        var endPosition = ent.Comp.TubeLength / 2;

        var layer = new PrototypeLayerData
        {
            RsiPath = ent.Comp.RsiPath,
            State = ent.Comp.RsiState
        };

        var startCapLayer = _spriteSystem.AddLayer((ent, sprite), layer, null);
        _spriteSystem.LayerSetOffset((ent, sprite), startCapLayer, new Vector2(0, endPosition));

        var endCapLayer = _spriteSystem.AddLayer((ent, sprite), layer, null);
        _spriteSystem.LayerSetOffset((ent, sprite), endCapLayer, new Vector2(0, -endPosition));
        _spriteSystem.LayerSetRotation((ent, sprite), endCapLayer, Angle.FromDegrees(180));

    }
}
