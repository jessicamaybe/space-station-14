using Content.Shared.Glassware;
using Content.Shared.Interaction;

namespace Content.Server.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class DropperFunnelSystem : EntitySystem
{
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
