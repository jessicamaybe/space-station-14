using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Glassware;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareComponent : Component
{
    //TODO: Multiple outlets. Max inlets and max outlets

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
/// Raised when two pieces of glassware are connected. Raised on origin entity, target is the output it connects to.
/// </summary>
[ByRefEvent]
public record struct OnGlasswareConnectEvent(Entity<GlasswareComponent?> Target)
{
    public bool Handled = false;
}

/// <summary>
/// Raised when two pieces of glassware are connected. Raised on origin entity, target is the output it connects to.
/// </summary>
[ByRefEvent]
public record struct OnGlasswareDisconnectEvent(Entity<GlasswareComponent?> Target)
{
    public bool Handled = false;
}
