using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerrySystem : EntitySystem
{

    private static readonly ProtoId<TagPrototype> DockTag = "DockMining"; //TODO: Yeah

    private void InitializeFerryConsole()
    {
        SubscribeLocalEvent<FerryConsoleComponent, ComponentStartup>(OnFerryConsoleStartup);
        SubscribeLocalEvent<FerryConsoleComponent, FerrySendShipMessage>(OnSendFerryShuttle);
        SubscribeLocalEvent<FerryConsoleComponent, FerryConsoleUpdateEvent>(OnFerryUpdate);
    }


    private void OnFerryUpdate(EntityUid uid, FerryConsoleComponent component, FerryConsoleUpdateEvent args)
    {
        if (args.Uid != component.Entity)
            return;

        var location = args.Ferry.Location;

        if (!TryComp(args.Uid, out TransformComponent? xform))
        {
            Log.Debug("Xform issue");
            return;
        }

        var locationName = "Space"; // TODO: Localize this

        if (_station.GetStationInMap(xform.MapID) is { } stationId)
            locationName = Name(stationId);

        _uiSystem.SetUiState(uid,
            FerryConsoleUiKey.Key,
            new FerryConsoleBoundUserInterfaceState()
        {
            AllowSend = args.Ferry.CanSend,
            LocationName =  locationName,
        });

    }

    private void OnFerryConsoleStartup(EntityUid uid, FerryConsoleComponent component, ComponentStartup args)
    {
        component.Entity = GetFerry(uid, component);
        if (!TryComp(component.Entity, out FerryComponent? ferryComponent))
            return;

        var ev = new FerryConsoleUpdateEvent(uid, ferryComponent);
        RaiseLocalEvent(component.Entity.Value, ref ev, false);
    }


    private void OnSendFerryShuttle(EntityUid uid, FerryConsoleComponent component, FerrySendShipMessage args)
    {

        if (!TryComp<FerryComponent>(component.Entity, out var ferry))
            return;

        if (!TryComp(component.Entity, out TransformComponent? shuttleXform) || shuttleXform.GridUid == null)
            return;

        var shuttleGridUid = shuttleXform.GridUid;

        if (!TryComp<ShuttleComponent>(shuttleGridUid, out var shuttleComponent))
            return;

        if (ferry.Location == ferry.Station)
        {
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent, ferry.Destination, null, null, DockTag);
        }
        else
        {
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent, ferry.Station, null, null);
        }
    }

    private EntityUid? GetFerry(EntityUid uid, FerryConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return null;

        // TODO: I know this sucks but needs device linking or something idunno
        var query = AllEntityQuery<FerryComponent, TransformComponent>();

        while (query.MoveNext(out var cUid, out _, out var xform))
        {
            foreach (var compType in component.Components.Values)
            {
                if (!HasComp(xform.GridUid, compType.Component.GetType()))
                {
                    continue;
                }

                return cUid;
            }
        }
        return null;
    }


}
