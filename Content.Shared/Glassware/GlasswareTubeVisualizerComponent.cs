using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

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
    public float TubeLength = 1.0f;

    /// <summary>
    /// Default RSI path (used for the end of the tube)
    /// </summary>
    [DataField]
    public string RsiPath = string.Empty;

    /// <summary>
    /// Default RSI state (used for the end of the tube)
    /// </summary>
    [DataField]
    public string RsiState = string.Empty;
}


public enum GlasswareTubeLayers : byte
{
    Tube,
    Start,
    End,
}
