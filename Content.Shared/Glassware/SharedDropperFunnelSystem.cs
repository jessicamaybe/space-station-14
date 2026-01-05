using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Glassware;

public sealed class SharedDropperFunnelSystem : EntitySystem
{

    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedGlasswareSystem _glasswareSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DropperFunnelComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DropperFunnelComponent, GlasswareUpdateEvent>(OnGlasswareUpdate);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        // Set all of our eye rotations to the relevant values.
        var query = EntityQueryEnumerator<DropperFunnelComponent>();

        //Only updating dropper funnels since they are the "start" of most chains.
        while (query.MoveNext(out var uid, out var dropperFunnelComponent))
        {
            if (_timing.CurTime < dropperFunnelComponent.NextUpdate)
                continue;

            dropperFunnelComponent.NextUpdate = _timing.CurTime + dropperFunnelComponent.UpdateInterval;

            if (!dropperFunnelComponent.Enabled)
                continue;

            var ev = new GlasswareUpdateEvent();
            RaiseLocalEvent(uid, ref ev);
        }
    }

    private void OnMapInit(Entity<DropperFunnelComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;
    }

    /// <summary>
    /// Toggles the dropper funnel on/off state
    /// </summary>
    /// <param name="ent"></param>
    public void Toggle(Entity<DropperFunnelComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (ent.Comp.Enabled)
        {
            SetEnabled((ent, ent.Comp), false);
            return;
        }
        SetEnabled((ent, ent.Comp), true);
    }

    /// <summary>
    /// Toggles the dropper funnel on/off state
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="enabled"></param>
    public void SetEnabled(Entity<DropperFunnelComponent?> ent, bool enabled)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.Enabled = enabled;
        _audio.PlayPredicted(ent.Comp.ValveSound, ent.Owner, null, AudioParams.Default.WithVariation(0.25f));
        _appearance.SetData(ent, DropperFunnelVisuals.State, enabled);
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

        var currentLevel = funnelSolution.Value.Comp.Solution.FillFraction;

        //its spill time
        if (glasswareComponent.OutletDevice == null)
        {
            var spillSolution = _solutionContainer.SplitSolution(funnelSolution.Value, speed);
            _puddle.TrySpillAt(ent.Owner, spillSolution, out _);
            return;
        }

        if (!_solutionContainer.TryGetGlasswareSolution(glasswareComponent.OutletDevice.Value, out var outletSolution, out _))
            return;

        var solution = _solutionContainer.SplitSolution(funnelSolution.Value, speed);

        if (!_solutionContainer.TryAddSolution(outletSolution.Value, solution))
            _solutionContainer.TryAddSolution(funnelSolution.Value, solution);

        args.Handled = true;

        //shutoff the other funnels connected to this ones outlet if it empties
        //QOL to prevent condenser from getting overly full
        if (funnelSolution.Value.Comp.Solution.FillFraction == 0 && currentLevel != 0)
        {
            SetEnabled((ent.Owner, ent.Comp), false);
            if (_glasswareSystem.TryGetNeighbors(ent.Owner, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (!HasComp<DropperFunnelComponent>(neighbor))
                        continue;
                    SetEnabled(neighbor.Owner, false);
                }
            }
        }
    }
}
