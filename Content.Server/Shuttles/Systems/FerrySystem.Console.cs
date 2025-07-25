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

    private static readonly ProtoId<TagPrototype> DockTag = "DockMining";

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

        if (!TryComp<FerryComponent>(component.Entity, out var ferry))
            return;

        if (!TryComp(component.Entity, out TransformComponent? shuttleXform) || shuttleXform.GridUid == null)
            return;

        var shuttleGridUid = shuttleXform.GridUid;


        if (!TryComp<ShuttleComponent>(shuttleGridUid, out var shuttleComponent))
            return;


        if (ferry.Location == ferry.Destination)
        {
            //if (TryGetDestination(out var dock))
            Log.Debug("FTLing to station");
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent, ferry.Station, null, null, DockTag);
        }
        else
        {
            Log.Debug("FTLing to destination");
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent, ferry.Destination, null, null, DockTag);
        }

        Log.Debug("we should have ftl'd");
    }

    private EntityUid? GetFerry(EntityUid uid, FerryConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return null;

        // I know this sucks but needs device linking or something idunno
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
