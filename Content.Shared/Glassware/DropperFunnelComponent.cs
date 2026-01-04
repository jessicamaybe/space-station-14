using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

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

    [DataField]
    public bool Enabled = false;

    /// <summary>
    /// Sound when <see cref="Open"/> is toggled.
    /// </summary>
    [DataField]
    public SoundSpecifier ValveSound = new SoundCollectionSpecifier("valveSqueak");
}


[Serializable, NetSerializable]
public enum DropperFunnelVisuals : byte
{
    State,
}
