using Content.Server.Anomaly.Components;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Anomaly;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class AnomalyVesselMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly SharedAnomalySystem _anomaly = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnomalyVesselComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    /// <summary>
    /// Pulses contained anomaly every malfunction tick
    /// </summary>
    private void OnMachineMalfunction(Entity<AnomalyVesselComponent> ent, ref MachineMalfunctionEvent args)
    {
        if (ent.Comp.Anomaly == null)
            return;

        _anomaly.DoAnomalyPulse(ent.Comp.Anomaly.Value);
    }
}
