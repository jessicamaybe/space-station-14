using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlasswareTubeVisualizerComponent : Component
{
    /// <summary>
    /// length of the tube
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TubeLength = new();
}
