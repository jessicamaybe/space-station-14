using Content.Shared._UM.Spiders.Components;
using Content.Shared.Alert;
using Content.Shared.StatusEffect;
using Content.Shared.StatusIcon;
using Robust.Shared.Player;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This handles...
/// </summary>
public sealed class BroodmotherSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BroodmotherComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<BroodmotherComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<BroodmotherComponent, OnWrapActionEvent>(OnWrapAction);
    }

    private void OnPlayerAttached(Entity<BroodmotherComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.HungerAlert);
    }

    private void OnPlayerDetached(Entity<BroodmotherComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.HungerAlert);
    }

    private void OnWrapAction(Entity<BroodmotherComponent> ent, ref OnWrapActionEvent args)
    {

    }

}
