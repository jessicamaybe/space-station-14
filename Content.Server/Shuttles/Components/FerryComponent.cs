namespace Content.Server.Shuttles.Components;

[RegisterComponent, AutoGenerateComponentPause]

public sealed partial class FerryComponent : Component
{
    /// <summary>
    /// The home station of the ferry
    /// </summary>
    [DataField("station")]
    public EntityUid Station;

    /// <summary>
    /// The off station target of the ferry
    /// </summary>
    [DataField("destination")]
    public EntityUid Destination;

    [DataField("currentLocation")]
    public EntityUid Location;

    public bool CanSend = true;


}
