using Content.Shared._UM.Sabotage.Components;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class MachingBugSharedSystem : EntitySystem
{

    [Dependency] private readonly SharedContainerSystem _container = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

}
