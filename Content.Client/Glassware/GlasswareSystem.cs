using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Glassware;
using Robust.Client.Debugging;
using Robust.Client.GameObjects;

namespace Content.Client.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareSystem : EntitySystem
{
    /// <inheritdoc/>m
    public override void Initialize()
    {
        SubscribeLocalEvent<GlasswareComponent, GlasswareChangeEvent>(OnGlasswareChange);
    }


    private void OnGlasswareChange(Entity<GlasswareComponent> ent, ref GlasswareChangeEvent ev)
    {

    }

}
