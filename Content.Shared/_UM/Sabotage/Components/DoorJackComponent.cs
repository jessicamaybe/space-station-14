using Content.Shared.Access;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Sabotage.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class DoorJackComponent : Component
{
    /// <summary>
    /// Access given to a door when the doorjack is installed.
    /// </summary>
    [DataField]
    public List<HashSet<ProtoId<AccessLevelPrototype>>> Access = new();

    /// <summary>
    /// How long it should take to jack the door
    /// </summary>
    [DataField]
    public TimeSpan JackDuration = TimeSpan.FromSeconds(10);
}


/// <summary>
/// Event raised on the person who installed a doorjack
/// Used for tracking objectives
/// </summary>
public sealed class InstalledDoorJackEvent(EntityUid door) : EntityEventArgs
{
    public EntityUid Door = door;
}

/// <summary>
/// Raised after the doafter on a doorjack
/// </summary>
[Serializable, NetSerializable]
public sealed partial class DoorJackDoAfterEvent : SimpleDoAfterEvent
{
}
