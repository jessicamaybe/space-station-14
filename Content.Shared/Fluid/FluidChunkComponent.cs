using Robust.Shared.GameStates;

namespace Content.Shared.Fluid;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true, fieldDeltas: true)]
public sealed partial class FluidChunkComponent : Component
{
    /// <summary>
    /// Dictionary of tile indices and the tile contents
    /// does not contain "pools"
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, TileSolution> Tiles = new();

    /// <summary>
    /// Currently active tiles
    /// </summary>
    [ViewVariables]
    public HashSet<Vector2i> ActiveTiles = new(256);
}
