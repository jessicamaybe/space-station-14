using System.Linq;
using System.Numerics;
using System.Threading;
using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.GameTicking.Events;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.GameTicking;
using Content.Shared.Localizations;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class FerryShuttleSystem : EntitySystem
{

    private void InitializeFerryConsole()
    {
        SubscribeLocalEvent<FerryShuttleConsoleComponent, FerryShuttleSendShipMessage>(OnSendFerryShuttle);
    }

    private void OnSendFerryShuttle(EntityUid uid, FerryShuttleConsoleComponent component, FerryShuttleSendShipMessage args)
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
