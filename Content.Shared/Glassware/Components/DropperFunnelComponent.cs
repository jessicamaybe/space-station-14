using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Glassware.Components;


[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DropperFunnelComponent : Component
{
    /// <summary>
    /// how many units per tick it should "drop"
    /// </summary>
    [ViewVariables]
    public int Speed = 1;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [ViewVariables]
    [AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

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
