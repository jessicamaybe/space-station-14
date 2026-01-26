using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BroodmotherComponent : Component
{
    /// <summary>
    /// The alert for your current hunger level
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> HungerAlert = "BorgHealth";
}


public sealed partial class OnLayEggActionEvent : InstantActionEvent
{
}

public sealed partial class OnWrapActionEvent : EntityTargetActionEvent
{
}
