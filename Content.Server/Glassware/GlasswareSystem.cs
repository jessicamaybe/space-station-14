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

        if (ent.Comp.Enabled)
        {
            ent.Comp.Enabled = false;
            _appearance.SetData(ent, DropperFunnelVisuals.State, false);
            return;
        }

        ent.Comp.Enabled = true;
        _appearance.SetData(ent, DropperFunnelVisuals.State, true);
    }

}
