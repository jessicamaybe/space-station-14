using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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
public struct GlasswareUpdateEvent
{
}

[ByRefEvent]
public struct GlasswareChangeEvent
{
    public EntityUid Origin;
    public EntityUid Target;

    public GlasswareChangeEvent(EntityUid origin, EntityUid target)
    {
        Origin = origin;
        Target = target;
    }
}
