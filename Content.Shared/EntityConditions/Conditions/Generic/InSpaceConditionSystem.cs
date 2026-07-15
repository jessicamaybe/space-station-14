using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions.Conditions.Generic;

/// <summary>
/// Returns true if the entity is in space.
/// </summary>
public sealed class InSpaceConditionSystem : EntityConditionSystem<TransformComponent, InSpaceCondition>
{
    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<InSpaceCondition> args)
    {
        if (entity.Comp.GridUid != null)
        {
            args.Result = false;
            return;
        }

        args.Result = true;
    }
}


/// <inheritdoc cref="EntityCondition"/>
public sealed partial class InSpaceCondition : EntityConditionBase<InSpaceCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => String.Empty;
}
