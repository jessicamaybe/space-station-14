using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Glassware.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareComponent : Component
{
    /// <summary>
    /// Solution the glassware will be interacting with
    /// </summary>
    [DataField]
    public string Solution = "default";

    /// <summary>
    /// The glassware network entity that this device is a part of
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Network;

    /// <summary>
    /// The number devices you can connect to the inlet
    /// </summary>
    [DataField, AutoNetworkedField]
    public int NumInlets = 4;

    /// <summary>
    /// The number devices you can connect to the outlet
    /// </summary>
    [DataField, AutoNetworkedField]
    public int NumOutlets = 1;

    /// <summary>
    /// Tool used to remove this device from the network
    /// </summary>
    [ViewVariables]
    public ProtoId<ToolQualityPrototype> Tool { get; private set; } = "Screwing";
}

[ByRefEvent]
public record struct GlasswareUpdateEvent()
{
    public bool Handled = false;
}

/// <summary>
/// Raised when two pieces of glassware are connected
/// </summary>
[ByRefEvent]
public record struct OnGlasswareConnectEvent
{
}
