using Content.Server.Objectives.Components;
using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Server._UM.Objectives.Components;

/// <summary>
/// Objective condition that requires the player to be a spy and have hacked a random number of doors with specific access on them.
/// Requires <see cref="NumberObjectiveComponent"/> to function.
/// </summary>
[RegisterComponent]
public sealed partial class SpyDoorJackObjectiveComponent : Component
{
    /// <summary>
    /// Number of doors jacked
    /// </summary>
    [DataField, ViewVariables]
    public int JackedDoors;

    /// <summary>
    /// Potential access targets
    /// </summary>
    [DataField]
    public List<HashSet<ProtoId<AccessLevelPrototype>>> Access = new();

    /// <summary>
    /// Accesses required on a door for the objective to count.
    /// </summary>
    [DataField]
    public List<HashSet<ProtoId<AccessLevelPrototype>>> RequiredAccess = new();
}
