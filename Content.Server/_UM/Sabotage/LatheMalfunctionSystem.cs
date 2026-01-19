using Content.Server.Lathe;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class LatheMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly LatheSystem _lathe = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LatheComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    private void OnMachineMalfunction(Entity<LatheComponent> ent, ref MachineMalfunctionEvent args)
    {
        var recipes = _lathe.GetAvailableRecipes(ent, ent.Comp);

        var produceableRecipes = new List<LatheRecipePrototype>();

        foreach (var recipe in recipes)
        {
            var producable = _lathe.CanProduce(ent, recipe, 1, ent.Comp);

            if (!_prototypeManager.Resolve(recipe, out var proto))
                continue;

            if (producable)
                produceableRecipes.Add(proto);
        }

        if (produceableRecipes.Count == 0)
            return;

        //Picking one and start making as many as possible >:)
        var pick = _random.Pick(produceableRecipes);
        var canProduce = true;
        while (canProduce)
        {
            canProduce = _lathe.TryAddToQueue(ent, pick, 1);
        }
        _lathe.TryStartProducing(ent);
    }
}
