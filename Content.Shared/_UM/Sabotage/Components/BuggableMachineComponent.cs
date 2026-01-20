using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Sabotage.Components;

/// <summary>
/// This handles machines that are able to have bugs/spy equipment install inside of them.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBuggableMachineSystem))]
public sealed partial class BuggableMachineComponent : Component
{
    public const string ContainerId = "InstalledBugs";

    /// <summary>
    /// Container slot containing any installed bugs
    /// </summary>
    [ViewVariables]
    public ContainerSlot InstalledBugs = default!;

    /// <summary>
    /// Sound played when a bug is broken
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier BrokenSound = new SoundCollectionSpecifier("sparks");

    /// <summary>
    /// Sound played when a bug is removed
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier RemovedSound = new SoundPathSpecifier("/Audio/Items/wirecutter.ogg");
}


/// <summary>
/// Raised on the user of a Machine Bug after the doafter to install the bug
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MachineBugInsertDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
/// Raised on the user of a Machine Bug after the doafter to install the bug
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MachineBugRemoveDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
/// Event raised on a user after they install a bug
/// Normally for tracking install bug objectives
/// </summary>
[Serializable, NetSerializable]
public sealed class AfterMachineBugInsertEvent : EntityEventArgs
{
}
