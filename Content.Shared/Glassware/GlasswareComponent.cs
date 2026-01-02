using System.Numerics;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareComponent : Component
{

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> InletDevices = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid? OutletDevice;

    [ViewVariables]
    public TimeSpan NextUpdate;

    [ViewVariables]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    [ViewVariables]
    public ProtoId<ToolQualityPrototype> Tool { get; private set; } = "Screwing";

}

[ByRefEvent]
public record struct GlasswareUpdateEvent()
{
    public bool Handled = false;
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
