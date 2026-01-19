using Content.Server.DeviceNetwork.Systems;
using Content.Server.DeviceNetwork.Systems.Devices;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Lathe;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class ApcMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly ApcSystem _apcSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ApcComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    private void OnMachineMalfunction(Entity<ApcComponent> ent, ref MachineMalfunctionEvent args)
    {
        if (!TryComp<ApcPowerProviderComponent>(ent, out var powerProviderComponent))
            return;

        if (!ent.Comp.MainBreakerEnabled)
            return;

        ent.Comp.TripFlag = true;
        _apcSystem.ApcToggleBreaker(ent);

    }
}
