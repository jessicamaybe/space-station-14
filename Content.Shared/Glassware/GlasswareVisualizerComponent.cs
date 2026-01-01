using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GlasswareVisualizerComponent : Component
{

    public List<EntityUid> TubeSprites = new();
}
