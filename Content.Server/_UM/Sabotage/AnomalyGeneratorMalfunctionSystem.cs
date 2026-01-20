using Content.Server.Anomaly;
using Content.Server.Anomaly.Components;
using Content.Shared._UM.Sabotage.Components;
using Robust.Shared.Timing;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class AnomalyGeneratorMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnomalyGeneratorComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    /// <summary>
    /// I dunno, just try creating an anomaly.
    /// It's something, kinda lame though
    /// </summary>
    private void OnMachineMalfunction(Entity<AnomalyGeneratorComponent> ent, ref MachineMalfunctionEvent args)
    {
        _anomaly.TryGeneratorCreateAnomaly(ent);
    }
}
