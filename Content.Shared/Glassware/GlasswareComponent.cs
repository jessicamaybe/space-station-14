using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent]
public sealed partial class GlasswareComponent : Component
{

    [ViewVariables]
    public List<EntityUid> InletDevices = new();

    [ViewVariables]
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
}
