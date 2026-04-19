using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Checks if the owner is pulling something or not
/// </summary>
public sealed partial class IsPullingPrecondition : HTNPrecondition
{
    private PullingSystem _pulling = default!;

    [ViewVariables(VVAccess.ReadWrite)] [DataField]
    public bool IsPulling = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pulling = sysManager.GetEntitySystem<PullingSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        return IsPulling && _pulling.IsPulling(owner) ||
               !IsPulling && !_pulling.IsPulling(owner);
    }
}
