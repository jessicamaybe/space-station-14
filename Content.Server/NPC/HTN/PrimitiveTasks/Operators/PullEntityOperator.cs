using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PullEntityOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private PullingSystem _pullingSystem = default!;

    [DataField("targetKey")]
    public string Key = "Target";

    /// <summary>
    /// Should we start pulling or stop pulling?
    /// </summary>
    [DataField]
    public bool Pull = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pullingSystem = sysManager.GetEntitySystem<PullingSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(Key);

        if (Pull && _pullingSystem.TryStartPull(owner, target))
            return HTNOperatorStatus.Finished;

        if (!Pull)
        {
            if (!_entManager.TryGetComponent<PullerComponent>(owner, out var pullerComponent) ||
                !_entManager.TryGetComponent<PullableComponent>(pullerComponent.Pulling, out var pullableComponent))
                return HTNOperatorStatus.Failed;

            if (_pullingSystem.TryStopPull(pullerComponent.Pulling.Value, pullableComponent))
                return HTNOperatorStatus.Finished;
        }

        return HTNOperatorStatus.Failed;
    }

}
