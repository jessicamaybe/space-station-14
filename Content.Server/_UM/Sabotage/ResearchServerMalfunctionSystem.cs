using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Research.Components;
using Content.Shared.Research.Systems;
using Robust.Shared.Random;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class ResearchServerMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly SharedResearchSystem _researchSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ResearchServerComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    private void OnMachineMalfunction(Entity<ResearchServerComponent> ent, ref MachineMalfunctionEvent args)
    {
        if (!TryComp<TechnologyDatabaseComponent>(ent, out var database))
            return;

        if (database.UnlockedTechnologies.Count == 0)
            return;

        var toRemove = _random.Pick(database.UnlockedTechnologies);

        _researchSystem.TryRemoveTechnology((ent.Owner, database), toRemove);

    }
}
