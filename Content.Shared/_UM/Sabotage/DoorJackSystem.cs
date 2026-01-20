using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Charges.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class DoorJackSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoorJackComponent, AfterInteractEvent>(OnInteractUsing);
        SubscribeLocalEvent<DoorJackComponent, DoorJackDoAfterEvent>(JackDoor);

    }

    private void OnInteractUsing(Entity<DoorJackComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null)
            return;

        if (_sharedCharges.IsEmpty(ent.Owner))
        {
            _popup.PopupClient(Loc.GetString("emag-no-charges"), args.User, args.User);
            return;
        }

        if (TryComp<WiresPanelComponent>(args.Target, out var panel) && !panel.Open)
            return;

        if (TryComp<WiresPanelSecurityComponent>(args.Target, out var wiresPanelSecurity) && !wiresPanelSecurity.WiresAccessible)
            return;

        if (!HasComp<DoorComponent>(args.Target) || (!HasComp<AccessReaderComponent>(args.Target.Value)))
            return;

        var ev = new DoorJackDoAfterEvent();

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.JackDuration, ev, ent, args.Target)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        };
        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }

    private void JackDoor(Entity<DoorJackComponent> ent, ref DoorJackDoAfterEvent args)
    {
        if (args.Target == null)
            return;

        if (!_accessReader.GetMainAccessReader(args.Target.Value, out var accessReaderEnt))
            return;

        if (TryComp<DoorComponent>(args.Target.Value, out var doorComponent))
            _audio.PlayPredicted(doorComponent.SparkSound, args.Target.Value, args.User);

        _accessReader.TryAddAccesses(accessReaderEnt.Value, ent.Comp.Access);

        _sharedCharges.TryUseCharge(ent.Owner);

        args.Handled = true;

        var insertEvent = new InstalledDoorJackEvent(args.Target.Value);
        RaiseLocalEvent(args.User, insertEvent);
    }
}
