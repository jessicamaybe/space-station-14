using System.Numerics;
using Content.Shared.Destructible;
using Content.Shared.DragDrop;
using Content.Shared.Glassware.Components;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;

namespace Content.Shared.Glassware;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedGlasswareSystem : EntitySystem
{
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

        if (draggedGlassware.NumOutlets == 0)
            return;

        if (TryGetOutlets(ent, out var outlets) && outlets.Contains(args.Dragged))
            return;

        if (TryGetOutlets((args.Dragged, draggedGlassware), out outlets) && (outlets.Count >= draggedGlassware.NumOutlets))
            return;

        var dragged = (args.Dragged, draggedGlassware);
        ConnectGlassware(dragged, ent);
    }

    private void OnPickupAttempt(Entity<GlasswareComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        if (ent.Comp.Network != null)
        {
            args.Cancel();
        }

    }
    private void OnPullAttemptEvent(Entity<GlasswareComponent> ent, ref PullAttemptEvent args)
    {
        if (ent.Comp.Network != null)
        {
            args.Cancelled = true;
        }
    }

    private void OnToolUseAttempt(Entity<GlasswareComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (ent.Comp.Network != null)
        {
            args.Cancel();
        }
    }

    private void OnInteractUsing(Entity<GlasswareComponent> ent, ref InteractUsingEvent args)
    {
        if (_tool.HasQuality(args.Used, ent.Comp.Tool))
            RemoveGlassware(ent);
    }

    private void OnGlasswareMoved(Entity<GlasswareComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.Network != null)
            RemoveGlassware(ent);
    }

    private void OnDestruction(Entity<GlasswareComponent> ent, ref DestructionAttemptEvent args)
    {
        RemoveGlassware(ent);
    }
}
