using Content.Server.DeviceNetwork.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
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

    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

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
        component.Station = station;

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

        Log.Debug("FTL Started");
        //component.Location = component.Location;
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
