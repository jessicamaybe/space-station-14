using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Construction;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Wires;
using Robust.Shared.Containers;

namespace Content.Shared._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class BuggableMachineSharedSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BuggableMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BuggableMachineComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BuggableMachineComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BuggableMachineComponent, MachineDeconstructedEvent>(OnMachineDeconstructed);
        SubscribeLocalEvent<BuggableMachineComponent, MachineBugInsertDoAfterEvent>(OnMachineBugInsert);
    }

    private void OnComponentInit(Entity<BuggableMachineComponent> ent, ref ComponentInit args)
    {
        ent.Comp.InstalledBugs = _container.EnsureContainer<ContainerSlot>(ent, BuggableMachineComponent.ContainerID);
    }

    private void OnMachineDeconstructed(Entity<BuggableMachineComponent> ent, ref MachineDeconstructedEvent args)
    {
        _container.EmptyContainer(ent.Comp.InstalledBugs, true, Transform(ent).Coordinates);
    }

    private void OnInteractUsing(Entity<BuggableMachineComponent> ent, ref InteractUsingEvent args)
    {
        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        if (!TryComp<MachineBugComponent>(args.Used, out var bug) || ent.Comp.InstalledBugs.Count != 0)
            return;

        InstallBug(ent, args.User, args.Used, bug.DoAfterDuration);
        args.Handled = true;
    }

    private void OnExamined(Entity<BuggableMachineComponent> ent, ref ExaminedEvent args)
    {
        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        if (ent.Comp.InstalledBugs.Count > 0)
            args.PushText("This things got a bug in it", -10);
    }

    private void InstallBug(Entity<BuggableMachineComponent> ent, EntityUid user, EntityUid bug, float duration)
    {
        var doAfterEventArgs = new DoAfterArgs(EntityManager, user, duration, new MachineBugInsertDoAfterEvent(), ent, bug, user)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnMachineBugInsert(Entity<BuggableMachineComponent> ent, ref MachineBugInsertDoAfterEvent args)
    {
        if (args.Target == null)
            return;

        if (args.Cancelled)
            return;

        _container.Insert(args.Target.Value, ent.Comp.InstalledBugs);
    }
}
