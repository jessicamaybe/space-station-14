using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Destructible;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedGlasswareSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareComponent, CanDragEvent>(OnGlasswareCanDrag);
        SubscribeLocalEvent<GlasswareComponent, CanDropDraggedEvent>(OnGlasswareCanDragDropped);
        SubscribeLocalEvent<GlasswareComponent, DragDropTargetEvent>(OnDragDropTarget);
        SubscribeLocalEvent<GlasswareComponent, CanDropTargetEvent>(OnCanDropTargetEvent);

        SubscribeLocalEvent<GlasswareComponent, GettingPickedUpAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<GlasswareComponent, PullAttemptEvent>(OnPullAttemptEvent);

        SubscribeLocalEvent<GlasswareComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<GlasswareComponent, ToolUseAttemptEvent>(OnToolUseAttempt);

        SubscribeLocalEvent<GlasswareComponent, MoveEvent>(OnGlasswareMoved);
        SubscribeLocalEvent<GlasswareComponent, DestructionAttemptEvent>(OnDestruction);
    }

    private void OnGlasswareCanDragDropped(Entity<GlasswareComponent> ent, ref CanDropDraggedEvent args)
    {
        if (!HasComp<GlasswareComponent>(args.Target))
            return;

        args.CanDrop = true;
        args.Handled = true;
    }

    private void OnGlasswareCanDrag(Entity<GlasswareComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }

    private void OnCanDropTargetEvent(Entity<GlasswareComponent> ent, ref CanDropTargetEvent args)
    {
        var xform = Transform(ent);
        var xformTarget = Transform(args.Dragged);

        if (xformTarget.GridUid == null || xform.GridUid == null)
        {
            args.CanDrop = false;
            args.Handled = true;
            return;
        }

        if (xform.GridUid != xformTarget.GridUid)
        {
            args.CanDrop = false;
            args.Handled = true;
            return;
        }

        var distance = Vector2.Distance(xform.LocalPosition, xformTarget.LocalPosition);
        if (distance > 2)
        {
            args.CanDrop = false;
            args.Handled = true;
            return;
        }

        args.CanDrop = true;
        args.Handled = true;
    }

    private void OnDragDropTarget(Entity<GlasswareComponent> ent, ref DragDropTargetEvent args)
    {
        if (!TryComp<GlasswareComponent>(args.Dragged, out var draggedGlassware))
            return;

        if (draggedGlassware.NoOutlet)
            return;

        if (ent.Comp.OutletDevice == args.Dragged) //Prevent connecting one thing to another and then directly back
            return;

        var dragged = (args.Dragged, draggedGlassware);
        ConnectGlassware(dragged, ent);
    }

    private void OnPickupAttempt(Entity<GlasswareComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        if (ent.Comp.InletDevices.Count > 0 || ent.Comp.OutletDevice != null)
        {
            args.Cancel();
        }

    }
    private void OnPullAttemptEvent(Entity<GlasswareComponent> ent, ref PullAttemptEvent args)
    {
        if (ent.Comp.InletDevices.Count > 0 || ent.Comp.OutletDevice != null)
        {
            args.Cancelled = true;
        }
    }

    private void OnToolUseAttempt(Entity<GlasswareComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (ent.Comp.InletDevices.Count == 0 || ent.Comp.OutletDevice == null)
        {
            args.Cancel();
        }
    }

    private void OnInteractUsing(Entity<GlasswareComponent> ent, ref InteractUsingEvent args)
    {
        if (_tool.HasQuality(args.Used, ent.Comp.Tool))
            RemoveGlassware(ent);
    }

    /// <summary>
    /// Connect the output of ent to an input on target
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="target"></param>
    public void ConnectGlassware(Entity<GlasswareComponent> ent, Entity<GlasswareComponent> target)
    {
        if (target.Comp.InletDevices.Contains(ent.Owner) || ent.Comp.OutletDevice == target.Owner)
            return;

        if (ent.Comp.OutletDevice != null)
            RemoveGlasswareOutlet(ent);

        ent.Comp.OutletDevice = target.Owner;
        target.Comp.InletDevices.Add(ent.Owner);

        Dirty(ent);
        Dirty(target);

        var ev = new OnGlasswareConnectEvent();
        RaiseLocalEvent(ent, ref ev);
        RaiseLocalEvent(target, ref ev);

        UpdateGlasswareTube(ent);
    }

    /// <summary>
    /// Disconnects a glassware from its outlet
    /// </summary>
    /// <param name="ent"></param>
    public void RemoveGlasswareOutlet(Entity<GlasswareComponent> ent)
    {
        if (ent.Comp.OutletDevice == null)
            return;

        var outletEnt = ent.Comp.OutletDevice.Value;

        if (!TryComp<GlasswareComponent>(outletEnt, out var outletDeviceComp))
            return;

        outletDeviceComp.InletDevices.Remove(ent);

        ent.Comp.OutletDevice = null;
        UpdateGlasswareTube(ent);
        Dirty(ent);
        Dirty(outletEnt, outletDeviceComp);
    }

    /// <summary>
    /// Removes a piece of glassware from the network
    /// </summary>
    /// <param name="ent"></param>
    public void RemoveGlassware(Entity<GlasswareComponent> ent)
    {
        foreach (var inlet in ent.Comp.InletDevices)
        {
            if (!TryComp<GlasswareComponent>(inlet, out var inletGlassware))
                continue;

            inletGlassware.OutletDevice = null;
            UpdateGlasswareTube((inlet, inletGlassware));
        }

        if (TryComp<GlasswareVisualizerComponent>(ent, out var glasswareVisualizer))
        {
            foreach (var tubeSprite in glasswareVisualizer.TubeSprites)
            {
                Del(tubeSprite);
            }
        }

        ent.Comp.InletDevices.Clear();
        RemoveGlasswareOutlet(ent);
    }

    /// <summary>
    /// If true, returns the entity connected to ent
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="outlet"></param>
    /// <returns></returns>
    public bool TryGetOutlet(Entity<GlasswareComponent?> ent, [NotNullWhen(true)] out Entity<GlasswareComponent>? outlet)
    {
        outlet = null;

        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.OutletDevice == null)
            return false;

        if (!TryComp<GlasswareComponent>(ent.Comp.OutletDevice.Value, out var outletGlassware))
            return false;

        outlet = (ent.Comp.OutletDevice.Value, outletGlassware);
        return true;
    }

    /// <summary>
    /// If true, returns other entities with the same outlet as this one
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="neighbors"></param>
    /// <returns></returns>
    public bool TryGetNeighbors(Entity<GlasswareComponent?> ent,
        [NotNullWhen(true)] out List<Entity<GlasswareComponent>>? neighbors)
    {
        neighbors = null;

        if (!TryGetOutlet(ent.Owner, out var outlet) || outlet.Value.Comp.InletDevices.Count == 0)
            return false;

        neighbors = new List<Entity<GlasswareComponent>>();
        foreach (var inlet in outlet.Value.Comp.InletDevices)
        {
            if (TryComp<GlasswareComponent>(inlet, out var inletGlassware))
            {
                neighbors.Add((inlet, inletGlassware));
            }
        }
        return true;
    }


    /// <summary>
    /// Updates the visualization for a piece of glassware.
    /// Will spawn the tube between the specified entity, and it's outlet entity.
    /// Probably jank as fuck
    /// </summary>
    /// <param name="ent"></param>
    public void UpdateGlasswareTube(Entity<GlasswareComponent> ent)
    {
        //TODO: This throws errors and shit on server shutdown. figure this out
        if (!TryComp<GlasswareVisualizerComponent>(ent, out var glasswareVisualizerComponent))
            return;

        foreach (var tubes in glasswareVisualizerComponent.TubeSprites)
        {
            Del(tubes);
        }

        if (!TryGetOutlet((ent.Owner, ent.Comp), out var outlet))
            return;

        if (!TryComp<GlasswareVisualizerComponent>(outlet, out var outletVisualizerComponent))
            return;

        glasswareVisualizerComponent.TubeSprites.Clear();

        var xformOrigin = Transform(ent);
        var xformTarget = Transform(outlet.Value);

        var originCoords = xformOrigin.Coordinates.Offset(xformOrigin.LocalRotation.RotateVec(glasswareVisualizerComponent.OutletOffset));
        var targetCoords = xformTarget.Coordinates.Offset(xformTarget.LocalRotation.RotateVec(outletVisualizerComponent.InletOffset));

        var midpoint = (targetCoords.Position + originCoords.Position) / 2;
        var rotation = (originCoords.Position - targetCoords.Position).ToWorldAngle();

        if (!xformOrigin.Coordinates.IsValid(EntityManager))
            return;

        var tube = PredictedSpawnAtPosition(glasswareVisualizerComponent.Prototype, xformOrigin.Coordinates);

        glasswareVisualizerComponent.TubeSprites.Add(tube);
        _transform.SetLocalPositionRotation(tube, midpoint, rotation);

        var comp = EnsureComp<GlasswareTubeVisualizerComponent>(tube);
        comp.TubeLength = Vector2.Distance(originCoords.Position, targetCoords.Position);

        Dirty(outlet.Value, outletVisualizerComponent);
        Dirty(tube, comp);

        if (!TryComp<AppearanceComponent>(tube, out var appearanceComponent))
            return;

        _appearance.QueueUpdate(tube, appearanceComponent);
    }

    private void OnGlasswareMoved(Entity<GlasswareComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.InletDevices.Count == 0 && ent.Comp.OutletDevice == null)
            return;

        RemoveGlassware(ent);
    }

    private void OnDestruction(Entity<GlasswareComponent> ent, ref DestructionAttemptEvent args)
    {
        RemoveGlassware(ent);
    }
}
