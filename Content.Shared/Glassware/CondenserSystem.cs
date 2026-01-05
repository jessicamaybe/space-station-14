using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Utility;

namespace Content.Shared.Glassware;

public sealed class CondenserSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ChemicalReactionSystem _chemicalReactionSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {

        base.Initialize();
        SubscribeLocalEvent<CondenserComponent, GlasswareUpdateEvent>(OnGlasswareUpdate);
        SubscribeLocalEvent<CondenserComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    private void OnSolutionChanged(Entity<CondenserComponent> ent, ref SolutionContainerChangedEvent args)
    {

        var ev = new GlasswareUpdateEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnGlasswareUpdate(Entity<CondenserComponent> ent, ref GlasswareUpdateEvent args)
    {

        if (!TryComp<GlasswareComponent>(ent, out var glasswareComponent))
            return;

        if (!_solutionContainer.TryGetSolution(ent.Owner, glasswareComponent.Solution, out var condenserSolution))
            return;

        var currentContents = condenserSolution.Value.Comp.Solution.Contents.ShallowClone();


        _chemicalReactionSystem.FullyReactSolution(condenserSolution.Value);

        if (glasswareComponent.OutletDevice == null)
            return;

        if (!_solutionContainer.TryGetGlasswareSolution(glasswareComponent.OutletDevice.Value, out var outletSolution, out _))
            return;

        var reactedContents = condenserSolution.Value.Comp.Solution.Contents.ShallowClone();


        var newContents = new List<ReagentQuantity>();

        foreach (var reactedContent in reactedContents)
        {
            if (currentContents.Contains(reactedContent))
                continue;

            if (!currentContents.Exists(x => x.Reagent == reactedContent.Reagent))
            {
                newContents.Add(reactedContent);
                continue;
            }
            var index = currentContents.FindIndex(x => x.Reagent == reactedContent.Reagent);

            if (reactedContent.Quantity > currentContents[index].Quantity)
            {
                var difference = reactedContent.Quantity - currentContents[index].Quantity;
                var outletRoom = outletSolution.Value.Comp.Solution.AvailableVolume;

                if (outletRoom < difference)
                    difference = outletRoom;

                var temperature = condenserSolution.Value.Comp.Solution.Temperature;
                var removedAmount = _solutionContainer.RemoveReagent(condenserSolution.Value, reactedContent.Reagent, difference);
                _solutionContainer.TryAddReagent(outletSolution.Value, reactedContent.Reagent.Prototype, removedAmount);
                _solutionContainer.SetTemperature(outletSolution.Value, temperature);
            }
            newContents.Add(reactedContent);
        }

        foreach (var reagent in newContents)
        {
            if (currentContents.Exists(x => x.Reagent == reagent.Reagent))
                continue;

            var outletRoom = outletSolution.Value.Comp.Solution.AvailableVolume;

            var temperature = condenserSolution.Value.Comp.Solution.Temperature;
            var amount = _solutionContainer.RemoveReagent(condenserSolution.Value, reagent.Reagent.Prototype, outletRoom);
            _solutionContainer.TryAddReagent(outletSolution.Value, reagent.Reagent.Prototype, amount);
            _solutionContainer.SetTemperature(outletSolution.Value, temperature);
        }

    }
}
