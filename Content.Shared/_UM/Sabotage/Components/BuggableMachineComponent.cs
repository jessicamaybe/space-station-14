using Content.Shared.DoAfter;
using Content.Shared.Power.Generator;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Sabotage.Components;

/// <summary>
/// This handles machines that are able to have bugs/spy equipment install inside of them.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(BuggableMachineSharedSystem))]
public sealed partial class BuggableMachineComponent : Component
{
    public const string ContainerID = "InstalledBugs";

    /// <summary>
    /// Entities that have these components can be installed
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry BugComponentTypes = default!;

    [ViewVariables]
    public ContainerSlot InstalledBugs = default!;
}


/// <summary>
/// Raised on the user of a Machine Bug after the doafter to install the bug
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MachineBugInsertDoAfterEvent : SimpleDoAfterEvent
{
}
