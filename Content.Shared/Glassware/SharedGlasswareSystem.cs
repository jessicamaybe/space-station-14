using Content.Shared.Destructible;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
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
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GlasswareComponent, CanDragEvent>(OnGlasswareCanDrag);
        SubscribeLocalEvent<GlasswareComponent, CanDropDraggedEvent>(OnGlasswareCanDragDropped);
        SubscribeLocalEvent<GlasswareComponent, DragDropTargetEvent>(OnDragDropTarget);

        SubscribeLocalEvent<GlasswareComponent, GettingPickedUpAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<GlasswareComponent, PullAttemptEvent>(OnPullAttemptEvent);

        SubscribeLocalEvent<GlasswareComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<GlasswareComponent, ToolUseAttemptEvent>(OnToolUseAttempt);

        SubscribeLocalEvent<GlasswareComponent, MoveEvent>(OnGlasswareMoved);
        SubscribeLocalEvent<GlasswareComponent, DestructionAttemptEvent>(OnDestruction);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        // Set all of our eye rotations to the relevant values.
        var query = EntityQueryEnumerator<GlasswareComponent, DropperFunnelComponent>();

        while (query.MoveNext(out var uid, out var glasswareComponent, out var dropperFunnelComponent))
        {
            if (_timing.CurTime < glasswareComponent.NextUpdate)
                continue;

            glasswareComponent.NextUpdate = _timing.CurTime + glasswareComponent.UpdateInterval;

            var ev = new GlasswareUpdateEvent();
            RaiseLocalEvent(uid, ref ev);
        }
    }

    private void OnMapInit(Entity<GlasswareComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;
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

    private void OnDragDropTarget(Entity<GlasswareComponent> ent, ref DragDropTargetEvent args)
    {
        if (!TryComp<GlasswareComponent>(args.Dragged, out var draggedGlassware))
            return;

        if (ent.Comp.OutletDevice == args.Dragged) //Prevent connecting one thing to another and then directly back
        {
            Log.Debug("Circular loop check");
            return;
        }

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

        _physics.SetBodyType(ent, BodyType.Static);
        _physics.SetBodyType(target, BodyType.Static);

        Dirty(ent);
        Dirty(target);

        var ev = new GlasswareConnectEvent(GetNetEntity(ent), GetNetEntity(target));
        RaiseNetworkEvent(ev);
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

        Dirty(ent.Comp.OutletDevice.Value, outletDeviceComp);

        var ev = new GlasswareConnectEvent(GetNetEntity(ent), GetNetEntity(ent.Comp.OutletDevice.Value));
        RaiseNetworkEvent(ev);
        ent.Comp.OutletDevice = null;
        Dirty(ent);
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
            DirtyEntity(inlet);

            var ev = new GlasswareConnectEvent(GetNetEntity(inlet), GetNetEntity(ent));
            RaiseNetworkEvent(ev);
        }

        RemoveGlasswareOutlet(ent);
        ent.Comp.InletDevices.Clear();

        _physics.SetBodyType(ent, BodyType.Dynamic);

        DirtyEntity(ent);

        var ev2 = new GlasswareConnectEvent(GetNetEntity(ent), GetNetEntity(ent));
        RaiseNetworkEvent(ev2);
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
