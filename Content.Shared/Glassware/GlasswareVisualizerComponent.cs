using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareVisualizerComponent : Component
{
    [DataField]
    public List<EntityUid> TubeSprites = new();

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
