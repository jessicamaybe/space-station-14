using Content.Shared.Tools;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CocoonComponent : Component
{
    /// <summary>
    /// The contents of the cocoon
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container Contents = default!;

    /// <summary>
    /// the ID of the container
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public string ContainerId = "contents";

    /// <summary>
    /// Tool quality required to use a tool on this.
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> Quality = "Slicing";
}
