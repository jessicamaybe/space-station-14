using Content.Server.Ame;
using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class OverloadAmeConditionSystem : EntitySystem
{
    [Dependency] private CodeConditionSystem _codeCondition = default!;
    [Dependency] private AmeTargetSystem _ameTarget = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<AmeExplodedEvent>(OnAmeExploded);
    }

    [SubscribeLocalEvent]
    private void OnAssigned(Entity<OverloadAmeConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var ameCount = _ameTarget.GetEntities().Count;

        if (ameCount == 0)
        {
            args.Cancelled = true;
            return;
        }
    }

    private void OnAmeExploded(AmeExplodedEvent args)
    {
        var query = EntityQueryEnumerator<OverloadAmeConditionComponent, CodeConditionComponent>();
        while (query.MoveNext(out var uid, out _, out var codeCondition))
        {
            _codeCondition.SetCompleted((uid, codeCondition));
        }
    }
}
