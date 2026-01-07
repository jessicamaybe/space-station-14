using System.Linq;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Glassware.Components;
using Robust.Shared.Utility;

namespace Content.Shared.Glassware;

public sealed class CondenserSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ChemicalReactionSystem _chemicalReactionSystem = default!;
    [Dependency] private readonly SharedGlasswareSystem _glasswareSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {

        base.Initialize();
        SubscribeLocalEvent<CondenserComponent, GlasswareUpdateEvent>(OnGlasswareUpdate);
        SubscribeLocalEvent<CondenserComponent, OnGlasswareConnectEvent>(OnGlasswareConnect);
        SubscribeLocalEvent<CondenserComponent, SolutionContainerChangedEvent>(OnSolutionChanged);

    }

    private void OnSolutionChanged(Entity<CondenserComponent> ent, ref SolutionContainerChangedEvent args)
    {
        Update(ent);
    }

    private void OnGlasswareUpdate(Entity<CondenserComponent> ent, ref GlasswareUpdateEvent args)
    {
        Update(ent);
        args.Handled = true;
    }

    private void OnGlasswareConnect(Entity<CondenserComponent> ent, ref OnGlasswareConnectEvent args)
    {
        Update(ent);
    }

    public void Update(Entity<CondenserComponent> ent)
    {

        if (!TryComp<GlasswareComponent>(ent, out var glasswareComponent))
            return;

        if (!_solutionContainer.TryGetSolution(ent.Owner, glasswareComponent.Solution, out var condenserSolution))
            return;

        var currentContents = condenserSolution.Value.Comp.Solution.Contents.ShallowClone();

        //Only do the condenser reaction if it's connected to an output, otherwise do normal reacting.
        if (TryComp<ReactionMixerComponent>(ent, out var reactionMixer) && _glasswareSystem.TryGetOutlets((ent, glasswareComponent), out var outlet) && outlet.Count > 0)
        {
            _chemicalReactionSystem.FullyReactSolution(condenserSolution.Value, reactionMixer);
        }
        else
        {
            _chemicalReactionSystem.FullyReactSolution(condenserSolution.Value);
        }

        if (!_glasswareSystem.TryGetOutlets((ent, glasswareComponent), out var outlets) || outlets.Count == 0)
            return;



        //TODO: proper multiple outlets
        if (!_solutionContainer.TryGetGlasswareSolution(outlets.First(), out var outletSolution, out _))
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
