using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Construction;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Content.Shared.Whitelist;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class BuggableMachineSharedSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BuggableMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BuggableMachineComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BuggableMachineComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BuggableMachineComponent, MachineDeconstructedEvent>(OnMachineDeconstructed);
        SubscribeLocalEvent<BuggableMachineComponent, MachineBugInsertDoAfterEvent>(OnMachineBugInsert);
        SubscribeLocalEvent<BuggableMachineComponent, MachineBugRemoveDoAfterEvent>(OnMachineBugRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<BuggableMachineComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            foreach (var bug in comp.InstalledBugs.ContainedEntities)
            {
                if (!TryComp<MachineBugComponent>(bug, out var bugComp))
                    continue;

                if (bugComp.NextMalfunction > curTime)
                    continue;

                var malfunctionEvent = new MachineMalfunctionEvent();
                RaiseLocalEvent(uid, malfunctionEvent);

                var mod = _random.Next(-bugComp.MalfunctionIntervalModifier, bugComp.MalfunctionIntervalModifier);

                bugComp.NextMalfunction += bugComp.MalfunctionInterval + mod;
            }
        }
    }

    private void OnComponentInit(Entity<BuggableMachineComponent> ent, ref ComponentInit args)
    {
        ent.Comp.InstalledBugs = _container.EnsureContainer<ContainerSlot>(ent, BuggableMachineComponent.ContainerId);
    }

    private void OnMachineDeconstructed(Entity<BuggableMachineComponent> ent, ref MachineDeconstructedEvent args)
    {
        _container.CleanContainer(ent.Comp.InstalledBugs);
        _audio.PlayPvs(ent.Comp.BrokenSound, Transform(ent).Coordinates);
    }

    private void OnInteractUsing(Entity<BuggableMachineComponent> ent, ref InteractUsingEvent args)
    {
        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        if (TryComp<WiresPanelSecurityComponent>(ent, out var wiresPanelSecurity) && !wiresPanelSecurity.WiresAccessible)
            return;

        var cutQuality = "Cutting";

        if (_toolSystem.HasQuality(args.Used, cutQuality) && ent.Comp.InstalledBugs.Count > 0)
        {
            RemoveBug(ent, args.User, args.Used, 5f);
            args.Handled = true;
            return;
        }

        if (!TryComp<MachineBugComponent>(args.Used, out var bug))
            return;

        if (_entityWhitelist.IsWhitelistFail(bug.Whitelist, ent))
            return;

        InstallBug(ent, args.User, args.Used, bug.DoAfterDuration);
        args.Handled = true;
    }

    private void OnExamined(Entity<BuggableMachineComponent> ent, ref ExaminedEvent args)
    {
        if (TryComp<WiresPanelComponent>(ent, out var panel) && !panel.Open)
            return;

        if (ent.Comp.InstalledBugs.Count > 0)
            args.PushText("This things got a bug in it! TODO Make this not suck", -10);
    }

    private void RemoveBug(Entity<BuggableMachineComponent> ent, EntityUid user, EntityUid bug, float duration)
    {
        var doAfterEventArgs = new DoAfterArgs(EntityManager, user, duration, new MachineBugRemoveDoAfterEvent(), ent, bug, user)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
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

        if (!TryComp<MachineBugComponent>(args.Target.Value, out var bugComponent))
            return;

        if (!_container.Insert(args.Target.Value, ent.Comp.InstalledBugs))
            return;

        var mod = _random.Next(-bugComponent.MalfunctionIntervalModifier, bugComponent.MalfunctionIntervalModifier);

        bugComponent.NextMalfunction += _timing.CurTime + bugComponent.MalfunctionDelay + mod;

        if (args.Used == null)
            return;

        var insertEvent = new AfterMachineBugInsertEvent();
        RaiseLocalEvent(args.Used.Value, insertEvent);
    }

    private void OnMachineBugRemove(Entity<BuggableMachineComponent> ent, ref MachineBugRemoveDoAfterEvent args)
    {
        if (args.Used == null)
            return;

        _audio.PlayPredicted(ent.Comp.RemovedSound, ent.Owner, args.Used);
        _container.EmptyContainer(ent.Comp.InstalledBugs, true, Transform(args.Used.Value).Coordinates);
    }
}
