using Content.Server.Beam;
using Content.Shared.Glassware;
using Robust.Shared.Prototypes;

namespace Content.Server.Glassware;

/// <summary>
/// This handles...
/// </summary>
public sealed class GlasswareSystem : EntitySystem
{
    [Dependency] private readonly BeamSystem _beam = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlasswareComponent, GlasswareChangeEvent>(OnGlasswareChange);
    }

    private void OnGlasswareChange(Entity<GlasswareComponent> ent, ref GlasswareChangeEvent ev)
    {
        //EntProtoId linkBeamProto = "GlasswareBeam";

        //Log.Debug("Trying to create beam");
        //_beam.TryCreateBeam(ev.Origin, ev.Target, linkBeamProto);
    }
}
