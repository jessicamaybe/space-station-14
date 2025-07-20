using Content.Server.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerrySystem : EntitySystem
{

    private void InitializeFerryConsole()
    {
        SubscribeLocalEvent<FerryConsoleComponent, FerrySendShipMessage>(OnSendFerryShuttle);
    }

    private void OnSendFerryShuttle(EntityUid uid, FerryConsoleComponent component, FerrySendShipMessage args)
    {
        if (!TryComp(uid, out TransformComponent? xform) || xform.GridUid == null)
        {
            Log.Debug("No grid??");
            return;
        }

        var shuttleGridUid = xform.GridUid;

        if (!TryGetDock(out var dock))
        {
            Log.Debug("No arrivals");
            return;
        }

        if (TryComp<ShuttleComponent>(shuttleGridUid, out var shuttleComponent))
        {
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent,dock);
        }
        else
        {
            Log.Debug("No shuttle comp");
        }

    }

    private bool TryGetDock(out EntityUid uid)
    {
        var arrivalsQuery = EntityQueryEnumerator<ArrivalsSourceComponent>();

        while (arrivalsQuery.MoveNext(out uid, out _))
        {
            return true;
        }

        return false;
    }
}
