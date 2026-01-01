using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;

namespace Content.Shared.Glassware;

public sealed class DropperFunnelSystem : EntitySystem
{

    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {

        base.Initialize();
        SubscribeLocalEvent<DropperFunnelComponent, GlasswareUpdateEvent>(OnGlasswareUpdate);
    }

    private void OnGlasswareUpdate(Entity<DropperFunnelComponent> ent, ref GlasswareUpdateEvent args)
    {
        if (!TryComp<GlasswareComponent>(ent, out var glasswareComponent))
            return;


        if (!_solutionContainer.TryGetSolution(ent.Owner, "beaker", out var funnelSolution))
            return;

        if (glasswareComponent.OutletDevice == null)
            return;

        if (!_solutionContainer.TryGetSolution(glasswareComponent.OutletDevice.Value, "beaker", out var outletSolution))
            return;

        var solution = _solutionContainer.SplitSolution(funnelSolution.Value, ent.Comp.Speed);

        if (!_solutionContainer.TryAddSolution(outletSolution.Value, solution))
            _solutionContainer.TryAddSolution(funnelSolution.Value, solution);

    }
}
