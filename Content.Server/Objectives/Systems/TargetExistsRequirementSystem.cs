using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class TargetExistsRequirementSystem : EntitySystem
{
    [Dependency] private IDependencyCollection _dependency = default!;

    private readonly HashSet<EntityUid> _entities = new();

    [SubscribeLocalEvent]
    private void OnAssigned(Entity<TargetExistsRequirementComponent> ent, ref ObjectiveAssignedEvent args)
    {
        _entities.Clear();

        ent.Comp.Pool.FindEntities(_entities, _dependency, ent.Owner, ent.Comp.Conditions);

        if (_entities.Count == 0)
            args.Cancelled = true;

    }
}
