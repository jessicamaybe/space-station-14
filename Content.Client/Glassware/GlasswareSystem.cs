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
        base.Initialize();
        SubscribeNetworkEvent<GlasswareConnectEvent>(OnGlasswareConnect);
    }

    private void OnGlasswareConnect(GlasswareConnectEvent ev)
    {
        var origin = GetEntity(ev.Origin);
        var target = GetEntity(ev.Target);

        if (!TryComp<GlasswareVisualizerComponent>(origin, out var glasswareVisualizer))
            return;

        if (!TryComp<GlasswareComponent>(origin, out var originGlasswareComponent))
            return;

        foreach (var tubeSprite in glasswareVisualizer.TubeSprites)
        {
            Del(tubeSprite);
        }

        if (originGlasswareComponent.OutletDevice != null)
        {
            var xformOrigin = Transform(origin).LocalPosition;
            var xformTarget = Transform(target).LocalPosition;

            var midpoint = xformTarget - xformOrigin;

            var pipeEnt = Spawn(null, _transformSystem.GetMapCoordinates(origin));
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
