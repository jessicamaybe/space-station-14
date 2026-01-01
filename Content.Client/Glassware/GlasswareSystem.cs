using System.Numerics;
using Content.Shared.Glassware;
using Robust.Client.GameObjects;
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

        SubscribeLocalEvent<GlasswareComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }


    private void OnAppearanceChange(Entity<GlasswareComponent> ent, ref AppearanceChangeEvent ev)
    {
        if (!TryComp<GlasswareVisualizerComponent>(ent.Owner, out var glasswareVisualizer))
            return;

        foreach (var tubeSprite in glasswareVisualizer.TubeSprites)
        {
            Del(tubeSprite);
        }

        if (ent.Comp.OutletDevice != null)
        {
            var xformOrigin = Transform(ent.Owner).LocalPosition;
            var xformTarget = Transform(ent.Comp.OutletDevice.Value).LocalPosition;

            var midpoint = xformTarget - xformOrigin;

            var pipeEnt = Spawn(null, _transformSystem.GetMapCoordinates(ent.Owner));
            var spriteComponent = AddComp<SpriteComponent>(pipeEnt);

            var effectLayer = _spriteSystem.AddLayer((pipeEnt, spriteComponent), new SpriteSpecifier.Rsi(new ResPath("glassware.rsi"), "tube"));

            var distance = Vector2.Distance(xformOrigin, xformTarget);


            var scale = new Vector2(2, distance * 25); //I eyeballed 25 and it was the magic number. hooray...?

            _spriteSystem.LayerSetScale((pipeEnt, spriteComponent), effectLayer, scale);
            _spriteSystem.LayerSetOffset((pipeEnt, spriteComponent), effectLayer, midpoint/2);
            _spriteSystem.LayerSetRotation((pipeEnt, spriteComponent), effectLayer, midpoint.ToWorldAngle() );

            glasswareVisualizer.TubeSprites.Add(pipeEnt);
        }
    }

}
