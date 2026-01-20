using Content.Server.Medical.CrewMonitoring;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Medical.SuitSensors;
using Robust.Shared.Random;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class CrewMonitorMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly SharedSuitSensorSystem _suitSensorSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CrewMonitoringServerComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    /// <summary>
    /// Turning sensors for random people off
    /// At some point I'd like this to be biased towards kill targets, but I don't feel like it right now
    /// </summary>
    private void OnMachineMalfunction(Entity<CrewMonitoringServerComponent> ent, ref MachineMalfunctionEvent args)
    {
        if (ent.Comp.SensorStatus.Count == 0)
            return;

        var randomSensorStatus = _random.Pick(ent.Comp.SensorStatus);
        var sensor = GetEntity(randomSensorStatus.Value.SuitSensorUid);
        _suitSensorSystem.SetSensor(sensor, SuitSensorMode.SensorOff);
    }
}
