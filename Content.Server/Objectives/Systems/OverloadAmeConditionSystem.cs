using Content.Server.Ame;
using Content.Server.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class OverloadAmeConditionSystem : EntitySystem
{
    [Dependency] private CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<AmeExplodedEvent>(OnAmeExploded);
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
