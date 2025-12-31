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

        var ev = new GlasswareChangeEvent();
        RaiseLocalEvent(ent, ref ev);
        draggedGlassware.OutletDevice = ent;
        ent.Comp.InletDevices.Add(args.Dragged);
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
        foreach (var inlet in ent.Comp.InletDevices)
        {
            if (!TryComp<GlasswareComponent>(inlet, out var inletGlassware))
                continue;
            inletGlassware.OutletDevice = null;
        }

        ent.Comp.OutletDevice = null;
        ent.Comp.InletDevices.Clear();
        var ev = new GlasswareChangeEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnGlasswareMoved(Entity<GlasswareComponent> ent, ref MoveEvent args)
    {
        //RemoveGlassware(ent);
    }
}
