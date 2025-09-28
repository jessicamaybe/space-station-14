using Robust.Shared.GameStates;

namespace Content.Server.Spacemas;

/// <inheritdoc/>
[RegisterComponent]
public sealed partial class SpacemasCheerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int CheerScore = 30;

    /// <summary>
    /// Spacemas score threshold for when Santa should spawn
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int SantaThreshold = 40;

    /// <summary>
    /// Spacemas score threshold for when the Grinch should spawn
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int GrinchThreshold = 20;

    /// <summary>
    /// Gamerule for "santa" spawning
    /// </summary>
    [DataField]
    public string SantaRule = "DragonSpawn";

    [DataField]
    public bool SantaSpawned = false;

    [DataField]
    public bool GrinchSpawned = false;

    /// <summary>
    /// Gamerule for "grinch" spawning
    /// </summary>
    [DataField]
    public string GrinchRule = "DragonSpawn";
}
