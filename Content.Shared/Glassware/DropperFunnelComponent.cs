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

    [ViewVariables]
    public TimeSpan NextUpdate;

    [ViewVariables]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// The solution to drop from.
    /// </summary>
    [DataField]
    public string SolutionName = "beaker";

    [DataField]
    public bool Enabled;

    /// <summary>
    /// Sound when toggled.
    /// </summary>
    [DataField]
    public SoundSpecifier ValveSound = new SoundCollectionSpecifier("valveSqueak");
}


[Serializable, NetSerializable]
public enum DropperFunnelVisuals : byte
{
    State,
}
