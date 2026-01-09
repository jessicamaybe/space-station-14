using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Glassware;

/// <summary>
/// This is used for visualizing tube connections between two pieces of glassware
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareVisualizerComponent : Component
{

    /// <summary>
    /// Dictionary containing the target of the tube, and the tubes entity ID
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, EntityUid> TubeSprites = new();

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
