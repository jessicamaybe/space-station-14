using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Components;

/// <summary>
/// This is used for...
/// </summary>
[AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]
public sealed partial class BroodmotherComponent : Component
{
    /// <summary>
    /// The alert for your current hunger level
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> HungerAlert = "Essence";

    /// <summary>
    /// Entity to use for large cocoons (containing people)
    /// </summary>
    [DataField]
    public EntProtoId LargeCocoon = "SpiderCocoonLarge";

    /// <summary>
    /// Entity to use for small cocoons (smaller mobs)
    /// </summary>
    [DataField]
    public EntProtoId SmallCocoon = "SpiderCocoonSmall";

    /// <summary>
    /// Entity to use for eggs
    /// </summary>
    [DataField]
    public EntProtoId EggProto = "BroodmotherEggs";


    /// <summary>
    /// How much energy the Broodmother has. Is spent building webs and laying eggs.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 Energy = 100;

    /// <summary>
    /// How much energy should it cost to lay eggs
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 LayEggCost = 20;

    /// <summary>
    /// How much energy should it cost to shit out webs
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 WebCost = 5;

}


public sealed partial class OnLayEggActionEvent : InstantActionEvent
{
}

public sealed partial class OnWrapActionEvent : EntityTargetActionEvent
{
}
