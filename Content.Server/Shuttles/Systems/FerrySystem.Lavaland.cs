using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Physics;
using Content.Shared.Procedural;
using Content.Shared.Procedural.Loot;
using Content.Shared.Random;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station.Components;
using Content.Shared.Tag;
using Robust.Shared.Collections;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles;

public sealed class SpawnLavalandJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    private readonly IGameTiming _timing;
    private readonly IPrototypeManager _prototypeManager;
    private readonly AnchorableSystem _anchorable;
    private readonly BiomeSystem _biomes;
    private readonly DungeonSystem _dungeon;
    private readonly MetaDataSystem _metaData;
    private readonly SharedMapSystem _mapSystem;
    private readonly ISawmill _sawmill;
    private readonly MapLoaderSystem _mapLoader;
    private readonly IRobustRandom _random;

    public readonly FerryComponent Ferry;

    public SpawnLavalandJob(
        double maxTime,
        IEntityManager entManager,
        IGameTiming timing,
        ILogManager logManager,
        IPrototypeManager prototypeManager,
        AnchorableSystem anchorable,
        BiomeSystem biomes,
        MapLoaderSystem mapLoader,
        DungeonSystem dungeon,
        MetaDataSystem metaData,
        SharedMapSystem mapSystem,
        IRobustRandom random,
        FerryComponent ferry,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _timing = timing;
        _prototypeManager = prototypeManager;
        _anchorable = anchorable;
        _biomes = biomes;
        _mapLoader = mapLoader;
        _dungeon = dungeon;
        _metaData = metaData;
        _mapSystem = mapSystem;
        _random = random;
        Ferry = ferry;
        _sawmill = logManager.GetSawmill("salvage_job");
#if !DEBUG
        _sawmill.Level = LogLevel.Info;
#endif
    }


    private readonly List<ProtoId<BiomeTemplatePrototype>> _miningBiomeOptions = new()
    {
        "Lava",
    };

    protected override async Task<bool> Process()
    {
        var path = new ResPath("/Maps/Misc/miningoutpost.yml"); //TODO: maybe a cvar
        _mapSystem.CreateMap(out var mapId, runMapInit: false);
        var mapUid = _mapSystem.GetMap(mapId);

        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
            return false;

        _metaData.SetEntityName(mapUid, "Mining Outpost"); //TODO: Localize this


        _entManager.EnsureComponent<FerryDestinationComponent>(grid.Value);
        _entManager.EnsureComponent<PreventPilotComponent>(grid.Value);
        Ferry.Destination = grid.Value;

        var template = _random.Pick(_miningBiomeOptions);
        _biomes.EnsurePlanet(mapUid, _prototypeManager.Index(template));

        _mapSystem.InitializeMap(mapId);
        _mapSystem.SetPaused(mapUid, true);


        List<Vector2i> reservedTiles = new();
        var outpostRadius = 128;

        // ore
        foreach (var lootProto in _prototypeManager.EnumeratePrototypes<SalvageLootPrototype>())
        {
            if (!lootProto.Guaranteed)
                continue;
            try
            {
                await SpawnDungeonOre(lootProto, mapUid);
            }
            catch (Exception e)
            {
                _sawmill.Error($"Failed to spawn guaranteed loot {lootProto.ID}: {e}");
            }
        }

        var budgetEntries = new List<IBudgetEntry>();
        var probSum = budgetEntries.Sum(x => x.Prob);
        var random = new Random();
        var randomSystem = _entManager.System<RandomSystem>();

        var allLoot = _prototypeManager.Index(SharedSalvageSystem.ExpeditionsLootProto);
        var lootBudget = 100f;
        foreach (var rule in allLoot.LootRules)
        {
            switch (rule)
            {
                case RandomSpawnsLoot randomLoot:
                    budgetEntries.Clear();

                    foreach (var entry in randomLoot.Entries)
                    {
                        budgetEntries.Add(entry);
                    }

                    probSum = budgetEntries.Sum(x => x.Prob);

                    while (lootBudget > 0f)
                    {
                        var entry = randomSystem.GetBudgetEntry(ref lootBudget, ref probSum, budgetEntries, random);
                        if (entry == null)
                            break;

                        _sawmill.Debug($"Spawning dungeon loot {entry.Proto}");
                        await SpawnRandomEntry((mapUid, grid), entry, random);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        return true;
    }

    private async Task SpawnRandomEntry(Entity<MapGridComponent> grid, IBudgetEntry entry, Random random)
    {
        await SuspendIfOutOfTime();

        var availableTiles = new List<Vector2i>();

        while (availableTiles.Count > 0)
        {
            var tile = availableTiles.RemoveSwap(random.Next(availableTiles.Count));

            if (!_anchorable.TileFree(grid, tile, (int)CollisionGroup.MachineLayer,
                    (int)CollisionGroup.MachineLayer))
            {
                continue;
            }

            var uid = _entManager.SpawnAtPosition(entry.Proto, _mapSystem.GridTileToLocal(grid, grid, tile));
            _entManager.RemoveComponent<GhostRoleComponent>(uid);
            _entManager.RemoveComponent<GhostTakeoverAvailableComponent>(uid);
            return;
        }
    }

    private async Task SpawnDungeonOre(SalvageLootPrototype loot, EntityUid gridUid)
    {
        for (var i = 0; i < loot.LootRules.Count; i++)
        {
            var rule = loot.LootRules[i];

            switch (rule)
            {
                case BiomeMarkerLoot biomeLoot:
                {
                    if (_entManager.TryGetComponent<BiomeComponent>(gridUid, out var biome))
                    {
                        _biomes.AddMarkerLayer(gridUid, biome, biomeLoot.Prototype);
                    }
                }
                    break;
                case BiomeTemplateLoot biomeLoot:
                {
                    if (_entManager.TryGetComponent<BiomeComponent>(gridUid, out var biome))
                    {
                        _biomes.AddTemplate(gridUid, biome, "Loot", _prototypeManager.Index<BiomeTemplatePrototype>(biomeLoot.Prototype), i);
                    }
                }
                    break;
            }
        }
    }

}
