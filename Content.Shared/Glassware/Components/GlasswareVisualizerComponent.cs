using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Glassware.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareVisualizerComponent : Component
{
    /// <summary>
    /// Entities that are representing the tubes from this piece of glasswares outlet
    /// </summary>
    [DataField]
    public List<EntityUid> TubeSprites = new();

    [DataField]
    public EntProtoId Prototype = "GlasswareTube";

    /// <summary>
    /// Sprite offset for where the inlet tube should connect to
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 InletOffset;

    /// <summary>
    /// Sprite offset for where the outlet tube should connect to
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 OutletOffset;
}
