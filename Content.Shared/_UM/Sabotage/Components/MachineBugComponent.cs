namespace Content.Shared._UM.Sabotage.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class MachineBugComponent : Component
{

    /// <summary>
    /// How long it should take to install
    /// </summary>
    [DataField, ViewVariables]
    public float DoAfterDuration = 5f;
}
