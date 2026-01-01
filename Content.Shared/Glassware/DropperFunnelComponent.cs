using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent]
public sealed partial class DropperFunnelComponent : Component
{
    /// <summary>
    /// how many units per tick it should "drop"
    /// </summary>
    [ViewVariables]
    public int Speed = 1;

    /// <summary>
    /// The solution to drop from.
    /// </summary>
    [DataField]
    public string SolutionName = "beaker";
}
