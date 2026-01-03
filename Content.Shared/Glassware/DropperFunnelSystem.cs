using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.Timing;

namespace Content.Shared.Glassware;

public sealed class DropperFunnelSystem : EntitySystem
{

    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DropperFunnelComponent, GlasswareUpdateEvent>(OnGlasswareUpdate);
    }

    private void OnGlasswareUpdate(Entity<DropperFunnelComponent> ent, ref GlasswareUpdateEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        var speed = FixedPoint2.New(ent.Comp.Speed);
        if (TryComp<SolutionTransferComponent>(ent, out var transfer))
            speed = transfer.TransferAmount;

        if (!TryComp<GlasswareComponent>(ent, out var glasswareComponent))
            return;

        if (!_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var funnelSolution))
            return;

        if (glasswareComponent.OutletDevice == null)
            return;

        if (!_solutionContainer.TryGetSolution(glasswareComponent.OutletDevice.Value, ent.Comp.SolutionName, out var outletSolution))
            return;

        var solution = _solutionContainer.SplitSolution(funnelSolution.Value, speed);

        if (!_solutionContainer.TryAddSolution(outletSolution.Value, solution))
            _solutionContainer.TryAddSolution(funnelSolution.Value, solution);

        args.Handled = true;

        if (TryComp<GlasswareComponent>(glasswareComponent.OutletDevice, out var outlet))
        {
            if (_timing.CurTime < outlet.NextUpdate)
                return;

            outlet.NextUpdate = _timing.CurTime + outlet.UpdateInterval;

            var ev = new GlasswareUpdateEvent();
            RaiseLocalEvent(glasswareComponent.OutletDevice.Value, ref ev);
        }
    }
}
