using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Components;

[RegisterComponent]
public sealed partial class FerryConsoleComponent : Component
{
    //TODO: Make this use tags or linking? Does linking work across grids?
    [DataField("components", required: true)]
    public ComponentRegistry Components = default!;

    /// <summary>
    /// The entity this console is controlling
    /// </summary>
    [DataField("entity")]
    public EntityUid? Entity;
}
