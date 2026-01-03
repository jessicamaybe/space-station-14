using Content.Server.Beam;
using Content.Shared.Glassware;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;

namespace Content.Server.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDropperFunnelSystem _dropperFunnelSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DropperFunnelComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<DropperFunnelComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        _dropperFunnelSystem.Toggle(ent.Owner);
    }

}
