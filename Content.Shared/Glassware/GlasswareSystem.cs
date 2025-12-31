using Content.Shared.DragDrop;

namespace Content.Shared.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedGlasswareSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareComponent, CanDragEvent>(OnGlasswareCanDrag);
        SubscribeLocalEvent<GlasswareComponent, CanDropDraggedEvent>(OnGlasswareCanDragDropped);
        SubscribeLocalEvent<GlasswareComponent, DragDropTargetEvent>(OnDragDropTarget);
        SubscribeLocalEvent<GlasswareComponent, DragDropDraggedEvent>(OnDragDropDragged);
        SubscribeLocalEvent<GlasswareComponent, MoveEvent>(OnGlasswareMoved);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        // Set all of our eye rotations to the relevant values.
        var query = EntityQueryEnumerator<GlasswareComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var ev = new GlasswareUpdateEvent();
            RaiseLocalEvent(uid, ref ev);
        }

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

        Log.Debug("DragDropTargetEvent: ", args.Dragged.Id);

        var ev = new GlasswareChangeEvent();
        RaiseLocalEvent(ent, ref ev);
        draggedGlassware.OutletDevice = ent;
        ent.Comp.InletDevices.Add(args.Dragged);
    }

    private void OnGlasswareMoved(Entity<GlasswareComponent> ent, ref MoveEvent args)
    {
        ent.Comp.OutletDevice = null;
        ent.Comp.InletDevices.Clear();
        var ev = new GlasswareChangeEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnDragDropDragged(Entity<GlasswareComponent> ent, ref DragDropDraggedEvent args)
    {
        if (!HasComp<GlasswareComponent>(args.Target))
            return;

        //ent.Comp.OutletDevice = args.Target;
    }
}
