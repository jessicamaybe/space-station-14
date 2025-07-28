using Content.Server.DeviceNetwork.Systems;
using Content.Server.Parallax;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tiles;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
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
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly List<ProtoId<BiomeTemplatePrototype>> _miningBiomeOptions = new()
    {
        "Lava",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FerryComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<FerryComponent, FTLStartedEvent>(OnFerryFTLStarted);
        SubscribeLocalEvent<FerryComponent, FTLCompletedEvent>(OnFerryFTLComplete);

        InitializeFerryConsole();
    }

    private void OnComponentStartup(EntityUid uid, FerryComponent component, ComponentStartup args)
    {
        if (!HasComp<MapGridComponent>(uid))
            return;

        if (!TryComp(uid, out TransformComponent? xform))
            return;

        if (_station.GetStationInMap(xform.MapID) is not { } station)
            return;

        if (!TryComp<StationDataComponent>(station, out var stationData))
            return;

        if (_station.GetLargestGrid(stationData) is not { } largestGrid)
            return;

        component.Station = largestGrid;

        SetupMiningStation();

        var destinationQuery = EntityQueryEnumerator<FerryDestinationComponent>(); // TODO: Do specific docking tagging
        while (destinationQuery.MoveNext(out uid, out _))
        {
            component.Destination = uid;
        }
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

    private void SetupMiningStation()
    {
        var path = new ResPath("/Maps/Misc/miningoutpost.yml"); //TODO: maybe a cvar
        _mapSystem.CreateMap(out var mapId, runMapInit: false);
        var mapUid = _mapSystem.GetMap(mapId);

        if (!_loader.TryLoadGrid(mapId, path, out var grid))
            return;

        _metaData.SetEntityName(mapUid, "Mining Outpost"); //TODO: Localize this

        EnsureComp<FerryDestinationComponent>(grid.Value);
        EnsureComp<PreventPilotComponent>(grid.Value);

        var template = _random.Pick(_miningBiomeOptions);
        _biomes.EnsurePlanet(mapUid, _protoManager.Index(template));

        _mapSystem.InitializeMap(mapId);
    }

}
