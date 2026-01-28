using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Spiders.SpiderEnergy;

/// <summary>
/// This is used for keeping track of spiders energy levels
/// Used for ability costs
/// </summary>
[AutoGenerateComponentState, AutoGenerateComponentPause]
[RegisterComponent, NetworkedComponent]
public sealed partial class SpiderEnergyComponent : Component
{
    /// <summary>
    /// Can this entity just ignore energy requirements
    /// </summary>
    [DataField]
    public bool IgnoreEnergy;

    /// <summary>
    /// The alert for your current energy level
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> EnergyAlert = "Essence";

    /// <summary>
    /// How much energy should this spider regenerate every update cycle?
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 PassiveRegen = 0;

    /// <summary>
    /// Energy level in which we can't passively regen any higher
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 MaxRegenEnergy = 100;

    /// <summary>
    /// Current energy level
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Energy = 50;

    /// <summary>
    /// Time when next passive energy regen update will happen.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How frequently will regeneration occur
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

}
