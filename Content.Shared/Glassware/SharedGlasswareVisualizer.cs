using System.Numerics;

namespace Content.Shared.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedGlasswareVisualizer : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareVisualizerComponent, OnGlasswareConnectEvent>(OnGlasswareConnect);
        SubscribeLocalEvent<GlasswareVisualizerComponent, OnGlasswareDisconnectEvent>(OnGlasswareDisconnect);
    }

    private void OnGlasswareConnect(Entity<GlasswareVisualizerComponent> ent, ref OnGlasswareConnectEvent args)
    {
        if (args.Handled)
            return;

        CreateTube(ent.Owner, args.Target);
        args.Handled = true;
    }

    private void OnGlasswareDisconnect(Entity<GlasswareVisualizerComponent> ent, ref OnGlasswareDisconnectEvent args)
    {
        if (args.Handled)
            return;

        DeleteTube(ent.Owner, args.Target);
        args.Handled = true;
    }

    public bool DeleteTube(Entity<GlasswareComponent?> origin, Entity<GlasswareComponent?> target)
    {
        if (!Resolve(origin, ref origin.Comp) || !Resolve(target, ref target.Comp))
            return false;

        if (!TryComp<GlasswareVisualizerComponent>(origin, out var originGlasswareVisualizer))
            return false;

        if (!originGlasswareVisualizer.TubeSprites.TryGetValue(target, out var tube))
            return false;

        QueueDel(tube);
        return originGlasswareVisualizer.TubeSprites.Remove(target);
    }

    /// <summary>
    /// Creates a tube between two nodes
    ///
    /// I'm just creating the entity on the origin, moving it to the midpoint, and sending the length to the client where the
    /// sprite scaling stuff is handled
    /// </summary>
    public void CreateTube(Entity<GlasswareComponent?> origin, Entity<GlasswareComponent?> target)
    {
        if (!Resolve(origin, ref origin.Comp) || !Resolve(target, ref target.Comp))
            return;

        if (!TryComp<GlasswareVisualizerComponent>(origin, out var originGlasswareVisualizer))
            return;

        if (!TryComp<GlasswareVisualizerComponent>(target, out var targetVisualizerComponent))
            return;

        var xformOrigin = Transform(origin);
        var xformTarget = Transform(target);

        var originCoords = xformOrigin.Coordinates.Offset(xformOrigin.LocalRotation.RotateVec(originGlasswareVisualizer.OutletOffset));
        var targetCoords = xformTarget.Coordinates.Offset(xformTarget.LocalRotation.RotateVec(targetVisualizerComponent.InletOffset));

        var midpoint = (targetCoords.Position + originCoords.Position) / 2;
        var rotation = (originCoords.Position - targetCoords.Position).ToWorldAngle();

        if (!xformOrigin.Coordinates.IsValid(EntityManager))
            return;

        var tube = PredictedSpawnAtPosition(originGlasswareVisualizer.Prototype, xformOrigin.Coordinates);

        originGlasswareVisualizer.TubeSprites.TryAdd(target, tube);

        _transform.SetLocalPositionRotation(tube, midpoint, rotation);

        var comp = EnsureComp<GlasswareTubeVisualizerComponent>(tube);
        comp.TubeLength = Vector2.Distance(originCoords.Position, targetCoords.Position);

        Dirty(tube, comp);

        if (!TryComp<AppearanceComponent>(tube, out var appearanceComponent))
            return;

        _appearance.QueueUpdate(tube, appearanceComponent);
    }
}
