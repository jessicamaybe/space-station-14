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
        SubscribeNetworkEvent<GlasswareConnectEvent>(OnGlasswareConnect);
    }

    private void OnGlasswareConnect(GlasswareConnectEvent ev)
    {
        var origin = GetEntity(ev.Origin);
        var target = GetEntity(ev.Target);

        if (!TryComp<GlasswareVisualizerComponent>(origin, out var originGlasswareVisualizer))
            return;

        if (!TryComp<GlasswareVisualizerComponent>(origin, out var targetGlasswareVisualizer))
            return;

        if (!TryComp<GlasswareComponent>(origin, out var originGlasswareComponent))
            return;

        foreach (var tubeSprite in originGlasswareVisualizer.TubeSprites)
        {
            Del(tubeSprite);
        }

        if (originGlasswareComponent.OutletDevice != null)
        {
            var xformOrigin = Transform(origin).LocalPosition + originGlasswareVisualizer.OutletLocation;
            var xformTarget = Transform(target).LocalPosition + targetGlasswareVisualizer.InletLocation;

            var midpoint = xformTarget - xformOrigin;

            var pipeEnt = Spawn(null, _transformSystem.GetMapCoordinates(origin));
            var spriteComponent = AddComp<SpriteComponent>(pipeEnt);

            var effectLayer = _spriteSystem.AddLayer((pipeEnt, spriteComponent), new SpriteSpecifier.Rsi(new ResPath("glassware.rsi"), "tube"));

            var distance = Vector2.Distance(xformOrigin, xformTarget);
            Log.Debug("Distance: " + distance);

            var scale = new Vector2(1.25f, distance * 8); //I eyeballed 25 and it was the magic number. hooray...?

            _spriteSystem.SetScale((pipeEnt, spriteComponent), scale);
            _spriteSystem.SetOffset((pipeEnt, spriteComponent), midpoint/2);

            _spriteSystem.SetRotation((pipeEnt, spriteComponent), midpoint.ToWorldAngle());

            originGlasswareVisualizer.TubeSprites.Add(pipeEnt);
        }
    }

}
