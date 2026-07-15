using Content.Shared.Ghost;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions.Conditions.Generic;

/// <summary>
/// Returns true if the entity is in space.
/// </summary>
public sealed class IsGhostConditionSystem : EntityConditionSystem<TransformComponent, IsGhostCondition>
{
    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<IsGhostCondition> args)
    {
        if (HasComp<GhostComponent>(entity))
        {
            args.Result = true;
            return;
        }

        args.Result = false;
    }
}


/// <inheritdoc cref="EntityCondition"/>
public sealed partial class IsGhostCondition : EntityConditionBase<IsGhostCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => String.Empty;
}
