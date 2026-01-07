namespace Content.Shared.Glassware.Components;

/// <summary>
/// Represents a network of glassware devices and their connections
/// </summary>
[RegisterComponent]
public sealed partial class GlasswareNetworkComponent : Component
{

    /// <summary>
    /// Dictionary of Edges and their tube entities
    /// </summary>
    [ViewVariables]
    public Dictionary<(EntityUid, EntityUid), EntityUid> TubeVisuals = new();

    [ViewVariables]
    public Dictionary<EntityUid, HashSet<EntityUid>> Map = new();
}
