using System.Numerics;
using Content.Shared.Glassware.Components;

namespace Content.Shared.Glassware;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedGlasswareSystem
{

    public bool DeleteEdgeVisuals(Entity<GlasswareNetworkComponent> network, Entity<GlasswareComponent?> origin, Entity<GlasswareComponent?> target)
    {
        if (!Resolve(origin, ref origin.Comp) || !Resolve(target, ref target.Comp))
            return false;

        if (!network.Comp.TubeVisuals.TryGetValue((origin, target), out var tubeEnt))
            return false;

        network.Comp.TubeVisuals.Remove((origin, target));

        Del(tubeEnt);
        return true;
    }

    public EntityUid? CreateEdgeVisuals(Entity<GlasswareNetworkComponent> network, Entity<GlasswareComponent> origin, Entity<GlasswareComponent> target)
    {
        if (!TryComp<GlasswareVisualizerComponent>(origin, out var originGlasswareVisualizerComponent))
            return null;

        if (!TryComp<GlasswareVisualizerComponent>(target, out var targetGlasswareVisualizerComponent))
            return null;

        var xformOrigin = Transform(origin);
        var xformTarget = Transform(target);

        var originCoords = xformOrigin.Coordinates.Offset(xformOrigin.LocalRotation.RotateVec(originGlasswareVisualizerComponent.OutletOffset));
        var targetCoords = xformTarget.Coordinates.Offset(xformTarget.LocalRotation.RotateVec(targetGlasswareVisualizerComponent.InletOffset));

        var midpoint = (targetCoords.Position + originCoords.Position) / 2;
        var rotation = (originCoords.Position - targetCoords.Position).ToWorldAngle();

        if (!xformOrigin.Coordinates.IsValid(EntityManager))
            return null;

        var tube = PredictedSpawnAtPosition(originGlasswareVisualizerComponent.Prototype, xformOrigin.Coordinates);

        _transform.SetLocalPositionRotation(tube, midpoint, rotation);

        var comp = EnsureComp<GlasswareTubeVisualizerComponent>(tube);
        comp.TubeLength = Vector2.Distance(originCoords.Position, targetCoords.Position);

        if (!TryComp<AppearanceComponent>(tube, out var appearanceComponent))
            return null;

        _appearance.QueueUpdate(tube, appearanceComponent);

        Dirty(tube, comp);

        return tube;
    }
}
