namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class HasOrdersPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("orders", required: true)] public Enum Orders = default!;

    public override bool IsMet(Entity<HTNComponent> ent, NPCBlackboard blackboard)
    {
        return Equals(blackboard.GetValueOrDefault<Enum>(NPCBlackboard.CurrentOrders, _entManager), Orders);
    }
}
