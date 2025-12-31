using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent]
public sealed partial class GlasswareComponent : Component
{

    [ViewVariables]
    public List<EntityUid> InletDevices = new();

    [ViewVariables]
    public EntityUid? OutletDevice;

}

[ByRefEvent]
public struct GlasswareUpdateEvent
{
}

[ByRefEvent]
public struct GlasswareChangeEvent
{
}
