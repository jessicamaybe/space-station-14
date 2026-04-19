using Content.Server.Hands.Systems;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;

public sealed partial class ActivateInHandOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValueOrDefault<EntityUid>(NPCBlackboard.Owner, _entManager);

        var handsSystem = _entManager.System<HandsSystem>();

        if (handsSystem.TryActivateItemInHand(owner))
            return HTNOperatorStatus.Finished;

        return HTNOperatorStatus.Failed;
    }
}
