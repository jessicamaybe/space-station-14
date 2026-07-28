using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;


public sealed partial class AmeExistsRequirementSystem : EntitySystem
{
    [Dependency] private AmeTargetSystem _ameTarget = default!;

    [SubscribeLocalEvent]
    private void OnAssigned(Entity<OverloadAmeConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var ameCount = _ameTarget.GetEntities().Count;

        if (ameCount == 0)
            args.Cancelled = true;
    }
}
