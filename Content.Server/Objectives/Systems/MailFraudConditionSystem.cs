using Content.Server.Objectives.Components;
using Content.Shared.Delivery;
using Content.Shared.Mind;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class MailFraudConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MailFraudEvent>(OnMailFraud);
    }

    private void OnMailFraud(MailFraudEvent ev)
    {
        if (!_mind.TryGetMind(ev.User, out _, out var mindComponent) || !_mind.TryGetObjectiveComp<MailFraudConditionComponent>(ev.User, out var obj, mindComponent))
            return;

        var counterQuery = EntityQueryEnumerator<MailFraudConditionComponent, CounterConditionComponent>();

        while (counterQuery.MoveNext(out var uid, out var mailFraudConditionComponent, out var counterObjComp))
        {
            if (!mindComponent.Objectives.Contains(uid))
                continue;

            counterObjComp.Count++;
        }

    }
}
