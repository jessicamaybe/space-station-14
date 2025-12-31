using System.Numerics;
using Content.Client.Beam;
using Content.Shared.Beam;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Glassware;
using Robust.Client.Debugging;
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

        SubscribeLocalEvent<GlasswareComponent, GlasswareChangeEvent>(OnGlasswareChange);
    }


    private void OnGlasswareChange(Entity<GlasswareComponent> ent, ref GlasswareChangeEvent ev)
    {
        //if (!TryComp<SpriteComponent>(ev.Origin, out var spriteComponent))
        //    return;

        var xformOrigin = Transform(ev.Origin).LocalPosition;
        var xformTarget = Transform(ev.Target).LocalPosition;

        var midpoint = xformTarget - xformOrigin;

        var pipeEnt = Spawn(null, _transformSystem.GetMapCoordinates(ev.Origin));
        var spriteComponent = AddComp<SpriteComponent>(pipeEnt);

        var effectLayer = _spriteSystem.AddLayer((pipeEnt, spriteComponent), new SpriteSpecifier.Rsi(new ResPath("glassware.rsi"), "tube"));

        var distance = Vector2.Distance(xformOrigin, xformTarget);
        Log.Debug("Distance: ", distance);

        var scale = new Vector2(2, distance * 25);


        _spriteSystem.LayerSetScale((pipeEnt, spriteComponent), effectLayer, scale);
        _spriteSystem.LayerSetOffset((pipeEnt, spriteComponent), effectLayer, midpoint/2);
        _spriteSystem.LayerSetRotation((pipeEnt, spriteComponent), effectLayer, midpoint.ToWorldAngle() );
    }

}
