using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerrySystem : EntitySystem
{

    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entManager  = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly AnchorableSystem _anchorable = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;


    private const double SalvageJobTime = 0.002;


    private const double DungeonJobTime = 0.005;
    private readonly JobQueue _ferryJobQueue = new(DungeonJobTime);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FerryComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<FerryComponent, FTLStartedEvent>(OnFerryFTLStarted);
        SubscribeLocalEvent<FerryComponent, FTLCompletedEvent>(OnFerryFTLComplete);

        InitializeFerryConsole();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _ferryJobQueue.Process();
    }

    private static readonly ProtoId<BiomeTemplatePrototype> MiningBiome = "LavaLandBiome";
    private static readonly ProtoId<DungeonConfigPrototype> DungeonConfig = "LavaLand";


    private void OnComponentStartup(EntityUid uid, FerryComponent component, ComponentStartup args)
    {
        if (!HasComp<MapGridComponent>(uid))
            return;

        if (!TryComp(uid, out TransformComponent? xform))
            return;

        if (_station.GetStationInMap(xform.MapID) is not { } station)
            return;

        if (_station.GetLargestGrid(station) is not { } largestGrid)
            return;

        component.Station = largestGrid;
        component.Location = component.Station;

        SetupMiningPlanet(component);

    }

    private void UpdateConsoles(EntityUid uid, FerryComponent component)
    {
        var ev = new FerryConsoleUpdateEvent(uid, component);
        RaiseLocalEvent(uid, ref ev, false);

        var query = EntityQueryEnumerator<FerryConsoleComponent>();

        while (query.MoveNext(out var consoleUid, out var consoleComponent))
        {
            if (consoleComponent.Entity != uid)
                continue;

            RaiseLocalEvent(consoleUid, ref ev, false);

        }

    }

    private void OnFerryFTLStarted(EntityUid uid, FerryComponent component, ref FTLStartedEvent args)
    {
        if (!HasComp<FerryComponent>(args.Entity))
            return;
        component.Location = args.TargetCoordinates.EntityId;
        component.CanSend = false;
        UpdateConsoles(uid, component);

    }

    private void OnFerryFTLComplete(EntityUid uid, FerryComponent component, ref FTLCompletedEvent args)
    {
        if (!HasComp<FerryComponent>(args.Entity))
            return;
        component.CanSend = true;
        UpdateConsoles(uid, component);
    }

    private void SetupMiningPlanet(FerryComponent component)
    {
        //
        // Shitty shitty shitty planet generation, this is just testing stuff TODO: PELASE REMOVE THIS
        //
        var path = new ResPath("/Maps/Misc/miningoutpost.yml");
        _mapSystem.CreateMap(out var mapId, runMapInit: false);
        var mapUid = _mapSystem.GetMap(mapId);

        if (!_mapLoader.TryLoadGrid(mapId, path, out var outpostGrid))
            return;

        if (!TryComp(mapUid, out TransformComponent? mapTransform))
        {
            Log.Debug("no map transform?");
            return;
        }


        _metaData.SetEntityName(mapUid, "Mining Outpost");

        EnsureComp<FerryDestinationComponent>(outpostGrid.Value);
        EnsureComp<PreventPilotComponent>(outpostGrid.Value);

        _biomes.EnsurePlanet(mapUid, _prototypeManager.Index(MiningBiome));

        if (!TryComp(mapUid, out MapGridComponent? mapGridComponent))
            return;

        _mapSystem.InitializeMap(mapId);

        // Do dungeon
        var seed = _random.Next();
        var dungeonPosition = Vector2i.Zero;

        _dungeon.GenerateDungeon(_prototypeManager.Index(DungeonConfig), mapUid, mapGridComponent, dungeonPosition, seed);

        component.Destination = outpostGrid.Value;
    }

}
