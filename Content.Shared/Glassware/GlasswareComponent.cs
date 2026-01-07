using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareComponent : Component
{
    [DataField]
    public string Solution = "default";

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> InletDevices = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid? OutletDevice;

    [ViewVariables]
    public ProtoId<ToolQualityPrototype> Tool { get; private set; } = "Screwing";

    [ViewVariables, DataField]
    public bool NoOutlet;
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
public record struct OnGlasswareConnectEvent()
{
}

[Serializable, NetSerializable]
public sealed class GlasswareConnectEvent : EntityEventArgs
{
    public NetEntity Origin;
    public NetEntity Target;

    public GlasswareConnectEvent(NetEntity origin, NetEntity target)
    {
        Origin = origin;
        Target = target;
    }
}
