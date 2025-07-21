namespace Content.Server.Shuttles.Components;

[RegisterComponent, AutoGenerateComponentPause]

public sealed partial class FerryComponent : Component
{
    [DataField("location")]
    public EntityUid Location;

    [DataField("destination")]
    public EntityUid Destination;
}
