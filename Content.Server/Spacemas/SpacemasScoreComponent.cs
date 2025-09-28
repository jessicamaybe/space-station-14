namespace Content.Server.Spacemas;

/// <inheritdoc/>
[RegisterComponent]
public sealed partial class SpacemasScoreComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int CheerScore;
}
