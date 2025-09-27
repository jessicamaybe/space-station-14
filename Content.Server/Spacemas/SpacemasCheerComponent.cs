using Robust.Shared.GameStates;

namespace Content.Server.Spacemas;

/// <inheritdoc/>
[RegisterComponent]
public sealed partial class SpacemasCheerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int CheerScore = default!;
}
