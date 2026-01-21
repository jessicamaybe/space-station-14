using Content.Shared.Whitelist;

namespace Content.Server._UM.Objectives.Components;

/// <summary>
/// This is used for tracking sabotage/bug planting objectives for Spys
/// </summary>
[RegisterComponent]
public sealed partial class PlantBugObjectiveComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist = new();

    [DataField]
    public bool Complete = false;
}
