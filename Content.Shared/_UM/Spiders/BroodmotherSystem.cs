using Content.Shared._UM.Spiders.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;

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

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BroodmotherComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BroodmotherComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<BroodmotherComponent, OnWrapActionEvent>(OnWrapAction);
        SubscribeLocalEvent<BroodmotherComponent, OnLayEggActionEvent>(OnLayEggAction);
        SubscribeLocalEvent<BroodmotherComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
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

        if (!_net.IsClient)
            ent.Comp.Energy -= ent.Comp.LayEggCost;

        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, ent.Comp.HungerAlert);
        args.Handled = true;
    }

    private void OnWrapAction(Entity<BroodmotherComponent> ent, ref OnWrapActionEvent args)
    {
        if (args.Handled || !HasComp<TransformComponent>(args.Target))
            return;

        if (_net.IsClient)
            return;

        var position = Transform(args.Target).Coordinates;
        var cocoon = Spawn(ent.Comp.LargeCocoon, position);

        if (!TryComp<CocoonComponent>(cocoon, out var cocoonComponent))
            return;

        _container.Insert(args.Target, cocoonComponent.Contents);

        args.Handled = true;
    }

}
