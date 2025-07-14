using Content.Server.Shuttles.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Shuttles.Components;

[RegisterComponent, AutoGenerateComponentPause]

public sealed partial class FerryShuttleComponent : Component
{
    [DataField("station")]
    public EntityUid Station;

    [DataField("destination")]
    public EntityUid Destination;

    [DataField("nextTransfer", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextTransfer;

    [DataField("nextArrivalsTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextArrivalsTime;

}
