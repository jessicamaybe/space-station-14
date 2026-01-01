using Content.Server.Beam;
using Content.Shared.Glassware;
using Robust.Shared.Prototypes;

namespace Content.Server.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }
}
