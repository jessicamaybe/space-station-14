using Content.Server.DeviceNetwork.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Shared.Shuttles.BUIStates;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerrySystem : EntitySystem
{

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActorSystem _actor = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FerryComponent, ComponentStartup>(OnFerryStartup);

        SubscribeLocalEvent<FerryComponent, FTLStartedEvent>(OnFerryFTLStarted);
        SubscribeLocalEvent<FerryComponent, FTLCompletedEvent>(OnFerryFTLComplete);

        InitializeFerryConsole();
    }


    private void OnFerryStartup(EntityUid uid, FerryComponent component, ComponentStartup args)
    {
        if (!HasComp<MapGridComponent>(uid))
            return;
    }

    private void UpdateConsoles(EntityUid uid, FerryComponent component)
    {
        var query = EntityQueryEnumerator<FerryConsoleComponent>();
        Log.Debug("queried");

        while (query.MoveNext(out var consoleUid, out var consoleComponent))
        {
            Log.Debug("found console");
            if (consoleComponent.Entity != uid)
            {
                Log.Debug("not same?");
                continue;
            }

            _uiSystem.SetUiState(consoleUid, FerryConsoleUiKey.Key, new FerryConsoleBoundUserInterfaceState()
            {
                AllowSend = component.CanSend,

            });

        }
    }

    private void OnFerryFTLStarted(EntityUid uid, FerryComponent component, ref FTLStartedEvent args)
    {
        if (!HasComp<FerryComponent>(args.Entity))
            return;

        Log.Debug("FTL Started");
        component.Location = args.TargetCoordinates.EntityId;
        component.CanSend = false;
        UpdateConsoles(uid, component);

    }

    private void OnFerryFTLComplete(EntityUid uid, FerryComponent component, ref FTLCompletedEvent args)
    {
        if (!HasComp<FerryComponent>(args.Entity))
            return;

        Log.Debug("FTL completed");
        component.Location = args.MapUid;
        component.CanSend = true;
        UpdateConsoles(uid, component);
    }

}
