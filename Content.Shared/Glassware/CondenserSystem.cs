using System.Linq;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
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
    }

    private void OnGlasswareUpdate(Entity<CondenserComponent> ent, ref GlasswareUpdateEvent args)
    {

        if (!TryComp<GlasswareComponent>(ent, out var glasswareComponent))
            return;

        if (!_solutionContainer.TryGetSolution(ent.Owner, "beaker", out var condenserSolution))
            return;


        var currentContents = condenserSolution.Value.Comp.Solution.Contents.ShallowClone();

        _chemicalReactionSystem.FullyReactSolution(condenserSolution.Value);

        if (glasswareComponent.OutletDevice == null)
            return;

        if (!_solutionContainer.TryGetSolution(glasswareComponent.OutletDevice.Value, "beaker", out var outletSolution))
            return;

        var reactedContents = condenserSolution.Value.Comp.Solution.Contents.ShallowClone();


        var newContents = new List<ReagentQuantity>();

        Log.Debug("---Looping through reagents---");
        foreach (var reactedContent in reactedContents)
        {
            Log.Debug("Checking reagent: " + reactedContent);
            if (currentContents.Contains(reactedContent))
            {
                Log.Debug("    Reagent was already here!");
                var index = currentContents.FindIndex(x => x.Reagent == reactedContent.Reagent);
                if (reactedContent.Quantity > currentContents[index].Quantity)
                {
                    Log.Debug("         Reagent has more now! Probably from reaction?");
                    var difference = reactedContent.Quantity - currentContents[index].Quantity;

                    var outletRoom = outletSolution.Value.Comp.Solution.AvailableVolume;

                    if (outletRoom < difference)
                        difference = outletRoom;

                    var removedAmount = _solutionContainer.RemoveReagent(condenserSolution.Value, reactedContent.Reagent, difference);
                    _solutionContainer.TryAddReagent(outletSolution.Value, reactedContent.Reagent.Prototype, removedAmount);
                }
                continue;
            }
            newContents.Add(reactedContent);
        }

        foreach (var reagent in newContents)
        {
            if (currentContents.Contains(reagent))
                continue;

            Log.Debug("New reagent: " + reagent);
            var outletRoom = outletSolution.Value.Comp.Solution.AvailableVolume;
            var amount = _solutionContainer.RemoveReagent(condenserSolution.Value, reagent.Reagent.Prototype, outletRoom);
            _solutionContainer.TryAddReagent(outletSolution.Value, reagent.Reagent.Prototype, amount);

        }

    }
}
