using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;


[RegisterComponent, NetworkedComponent]
public sealed partial class DropperFunnelComponent : Component
{

    /// <summary>
    ///
    /// </summary>
    [ViewVariables]
    public int Speed = 1;
}
