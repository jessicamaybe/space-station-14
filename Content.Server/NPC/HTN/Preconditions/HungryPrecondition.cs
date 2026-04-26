using Content.Shared.Nutrition.Components;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Returns true if the active hand entity has the specified components.
/// </summary>
public sealed partial class HungryPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField(required: true)]
    public HungerThreshold MinHungerState = HungerThreshold.Starving;

    public override bool IsMet(Entity<HTNComponent> ent, NPCBlackboard blackboard)
    {
        return _entManager.TryGetComponent<HungerComponent>(ent, out var hunger) ? hunger.CurrentThreshold <= MinHungerState : false;
    }
}
