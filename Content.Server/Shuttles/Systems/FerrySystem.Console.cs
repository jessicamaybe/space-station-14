using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;

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
            return;

        var shuttleGridUid = xform.GridUid;

        if (!TryGetDock(out var dock))
            return;

        if (TryComp<ShuttleComponent>(shuttleGridUid, out var shuttleComponent))
        {
            _shuttles.FTLToDock(shuttleGridUid.Value, shuttleComponent,dock);
            /*
            _uiSystem.SetUiState(uid, FerryConsoleUiKey.Key, new FerryConsoleBoundUserInterfaceState()
                {
                    AllowSend = false,
                });
                */
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
