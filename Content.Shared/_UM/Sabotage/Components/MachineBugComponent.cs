using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Sabotage.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MachineBugComponent : Component
{
    /// <summary>
    /// How long this bug will take to cause issues after being installed
    /// </summary>
    [DataField, ViewVariables]
    public TimeSpan MalfunctionDelay = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Time until the next malfunction
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextMalfunction;

    /// <summary>
    /// Average time in between malfunctions
    /// </summary>
    [DataField, ViewVariables]
    [AutoNetworkedField]
    public TimeSpan MalfunctionInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Maximum modifer for the malfunction interval
    /// </summary>
    [DataField, ViewVariables]
    [AutoNetworkedField]
    public TimeSpan MalfunctionIntervalModifier = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long it should take to install
    /// </summary>
    [DataField, ViewVariables]
    public float DoAfterDuration = 5f;

    /// <summary>
    /// Entities that have these components can be installed
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;
}

/// <summary>
/// Event raised on a machine to cause it to malfunction
/// </summary>
[Serializable, NetSerializable]
public sealed class MachineMalfunctionEvent : EntityEventArgs
{
}
