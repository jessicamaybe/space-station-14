using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station.Components;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerrySystem : EntitySystem
{

    private void InitializeFerryConsole()
    {
        SubscribeLocalEvent<FerryConsoleComponent, ComponentStartup>(OnFerryConsoleStartup);
        SubscribeLocalEvent<FerryConsoleComponent, FerrySendShipMessage>(OnSendFerryShuttle);
    }


    private void OnFerryConsoleStartup(EntityUid uid, FerryConsoleComponent component, ComponentStartup args)
    {
        component.Entity = GetFerry(uid, component);
        if (component.Entity == null)
            Log.Debug("Ferry Shuttle NULL");
    }

    private void OnSendFerryShuttle(EntityUid uid, FerryConsoleComponent component, FerrySendShipMessage args)
    {
        if (!TryComp(component.Entity, out TransformComponent? xform) || xform.GridUid == null)
        {
            Log.Debug("xform?");
            return;
        }

        var shuttleGridUid = xform.GridUid;

        if (!TryGetDock(out var dock))
        {
            Log.Debug("Can't get destination");
            return;
        }

        if (TryComp<ShuttleComponent>(shuttleGridUid, out var shuttleComponent))
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent,dock);

        Log.Debug("we should have ftl'd");
    }

    private bool TryGetDock(out EntityUid uid)
    {
        var dockQuery = EntityQueryEnumerator<ArrivalsSourceComponent>(); // TODO: Do specific docking tagging

        while (dockQuery.MoveNext(out uid, out _))
        {
            return true;
        }
        return false;
    }

    private EntityUid? GetFerry(EntityUid uid, FerryConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            Log.Debug("First Resolve Null");
            return null;
        }
/*
        var stationUid = _station.GetOwningStation(uid);

        if (stationUid == null)
        {
            Log.Debug("Station Not Found");
            return null;
        }
*/
        // I know this sucks but needs device linking or something idunno
        var query = AllEntityQuery<FerryComponent, TransformComponent>();

        while (query.MoveNext(out var cUid, out _, out var xform))
        {
            /* TODO: Station member stuff, doesnt work on dev map test on a real round
            if (xform.GridUid == null ||
                !TryComp<StationMemberComponent>(xform.GridUid, out var member) ||
                member.Station != stationUid)
            {
                continue;
            }
            */

            foreach (var compType in component.Components.Values)
            {
                if (!HasComp(xform.GridUid, compType.Component.GetType()))
                {
                    Log.Debug("Component not found");
                    continue;
                }

                return cUid;
            }
        }
        Log.Debug("We suck");
        return null;
    }


}
