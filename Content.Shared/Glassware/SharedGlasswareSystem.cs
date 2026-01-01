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
        var query = EntityQueryEnumerator<GlasswareComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;

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
        // Easily drawn-from thing can be dragged onto easily refillable thing.
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

        if (ent.Comp.InletDevices.Contains(args.Dragged))
            return;

        if (ent.Comp.OutletDevice == args.Dragged) //Prevent connecting one thing to another and then directly back
        {
            Log.Debug("Circular loop check");
            return;
        }

        var ev = new GlasswareChangeEvent(args.Dragged, ent.Owner);
        RaiseLocalEvent(ent, ref ev);


        if (draggedGlassware.OutletDevice != null &&
            TryComp<GlasswareComponent>(draggedGlassware.OutletDevice.Value, out var oldoutletComp))
        {
            if (TryComp<AppearanceComponent>(draggedGlassware.OutletDevice.Value, out var appearanceOldOutlet))
                _appearance.QueueUpdate(draggedGlassware.OutletDevice.Value, appearanceOldOutlet);

            oldoutletComp.InletDevices.Remove(args.Dragged);
        }

        draggedGlassware.OutletDevice = ent;

        ent.Comp.InletDevices.Add(args.Dragged);

        if (TryComp<AppearanceComponent>(args.Dragged, out var appearance))
            _appearance.QueueUpdate(args.Dragged, appearance);

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
    /// Removes a piece of glassware from its network
    /// </summary>
    /// <param name="ent"></param>
    public void RemoveGlassware(Entity<GlasswareComponent> ent)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.QueueUpdate(ent.Owner, appearance);


        foreach (var inlet in ent.Comp.InletDevices)
        {
            if (!TryComp<GlasswareComponent>(inlet, out var inletGlassware))
                continue;
            inletGlassware.OutletDevice = null;

            if (TryComp<AppearanceComponent>(inlet, out var inletAppearance))
                _appearance.QueueUpdate(inlet, inletAppearance);
        }

        if (TryComp<GlasswareComponent>(ent.Comp.OutletDevice, out var outletComp))
            outletComp.InletDevices.Remove(ent.Owner);

        ent.Comp.OutletDevice = null;
        ent.Comp.InletDevices.Clear();
        var ev = new GlasswareChangeEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnGlasswareMoved(Entity<GlasswareComponent> ent, ref MoveEvent args)
    {
        RemoveGlassware(ent);
    }

    private void OnDestruction(Entity<GlasswareComponent> ent, ref DestructionAttemptEvent args)
    {
        RemoveGlassware(ent);
    }
}
