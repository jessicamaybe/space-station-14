using Content.Shared.Glassware;
using Content.Shared.Glassware.Components;
using Content.Shared.Interaction;

namespace Content.Server.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class DropperFunnelSystem : SharedDropperFunnelSystem
{
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

        Toggle(ent.Owner);
    }

}
