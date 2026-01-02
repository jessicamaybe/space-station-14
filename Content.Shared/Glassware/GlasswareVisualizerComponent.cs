using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GlasswareVisualizerComponent : Component
{

    public List<EntityUid> TubeSprites = new();

    /// <summary>
    /// Sprite offset for where the inlet tube should connect to
    /// </summary>
    [DataField]
    public Vector2 InletLocation = new();

    /// <summary>
    /// Sprite offset for where the outlet tube should connect to
    /// </summary>
    [DataField]
    public Vector2 OutletLocation = new();
}
