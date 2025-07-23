namespace Content.Server.Shuttles.Components;

[RegisterComponent, AutoGenerateComponentPause]

public sealed partial class FerryComponent : Component
{
    [DataField("station")]
    public EntityUid Station;

    [DataField("destination")]
    public EntityUid Destination;

    [DataField("location")]
    public EntityUid Location;

    public bool CanSend = true;


}
