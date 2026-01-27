using Content.Shared._UM.Spiders.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This handles...
/// </summary>
public sealed class BroodmotherSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BroodmotherComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BroodmotherComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<BroodmotherComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);

        SubscribeLocalEvent<BroodmotherComponent, OnCocoonWrapActionEvent>(OnCocoonWrapAction);
        SubscribeLocalEvent<BroodmotherComponent, OnCocoonWrapDoAfterEvent>(OnCocoonWrapDoAfter);
        SubscribeLocalEvent<BroodmotherComponent, OnCocoonEnergyAbsorbDoAfterEvent>(OnCocoonEnergyAbsorbDoAfter);

        SubscribeLocalEvent<BroodmotherComponent, OnLayEggActionEvent>(OnLayEggAction);
    }

    private void OnComponentInit(Entity<BroodmotherComponent> ent, ref ComponentInit args)
    {
        var cocoonAction = "ActionCocoonize";
        var eggsAction = "ActionLayEggs";
        _actionsSystem.AddAction(ent, cocoonAction);
        _actionsSystem.AddAction(ent, eggsAction);
        _alerts.ShowAlert(ent.Owner, ent.Comp.HungerAlert);
    }

    private void OnGetCounterAmount(Entity<BroodmotherComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.HungerAlert != args.Alert)
            return;

        args.Amount = ent.Comp.Energy.Int();
    }

    private void OnPlayerDetached(Entity<BroodmotherComponent> ent, ref LocalPlayerDetachedEvent args)
    {

        _alerts.ClearAlert(ent.Owner, ent.Comp.HungerAlert);
    }

    private void OnLayEggAction(Entity<BroodmotherComponent> ent, ref OnLayEggActionEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Energy < ent.Comp.LayEggCost)
            return;

        var eggs = PredictedSpawnAtPosition(ent.Comp.EggProto, Transform(ent).Coordinates);
        _audio.PlayPredicted(ent.Comp.LayEggSound, eggs, null);

        if (!_net.IsClient)
            ent.Comp.Energy -= ent.Comp.LayEggCost;

        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, ent.Comp.HungerAlert);
        args.Handled = true;
    }

    private void OnCocoonWrapAction(Entity<BroodmotherComponent> ent, ref OnCocoonWrapActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MobStateComponent>(args.Target, out var mobState))
            return;

        if (mobState.CurrentState == MobState.Alive)
            return;

        var selfMessage = Loc.GetString("broodmother-start-cocooning-self", ("target", args.Target));
        var othersMessage = Loc.GetString("broodmother-start-cocooning-others", ("spider", ent.Owner), ("target", args.Target));
        _popup.PopupPredicted(
            selfMessage,
            othersMessage,
            ent,
            ent,
            PopupType.LargeCaution);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 5f, new OnCocoonWrapDoAfterEvent(), ent, args.Target)
        {
            BreakOnMove = true,
            NeedHand = false,
        });

        args.Handled = true;
    }

    private void OnCocoonWrapDoAfter(Entity<BroodmotherComponent> ent, ref OnCocoonWrapDoAfterEvent args)
    {
        if (_net.IsClient || args.Target == null || args.Cancelled)
            return;

        var position = Transform(args.Target.Value).Coordinates;
        var cocoon = Spawn(ent.Comp.LargeCocoon, position);

        if (!TryComp<CocoonComponent>(cocoon, out var cocoonComponent))
            return;

        _container.Insert(args.Target.Value, cocoonComponent.Contents);
    }

    private void OnCocoonEnergyAbsorbDoAfter(Entity<BroodmotherComponent> ent,
        ref OnCocoonEnergyAbsorbDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        if (!TryComp<CocoonComponent>(args.Target, out var cocoon) || cocoon.Harvested)
            return;

        if (!_net.IsClient)
            ent.Comp.Energy += 20;
        Dirty(ent);
        cocoon.Harvested = true;

        ProtoId<DamageTypePrototype> damageType = "Cellular";

        foreach (var mob in cocoon.Contents.ContainedEntities)
        {
            _damage.TryChangeDamage(mob, new DamageSpecifier(_prototypeManager.Index(damageType), 100));
        }
    }
}
