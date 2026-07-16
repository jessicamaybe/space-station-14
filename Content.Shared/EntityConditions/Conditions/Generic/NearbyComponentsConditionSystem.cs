using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions.Conditions.Generic;

/// <summary>
/// Checks if an entity is in range of a specified number of entities with specific components.
/// </summary>
public sealed partial class NearbyComponentsConditionSystem : EntityConditionSystem<TransformComponent, NearbyComponentsCondition>
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<NearbyComponentsCondition> args)
    {
        var inRange = new HashSet<Entity<IComponent>>();

        var found = false;
        var worldPos = _transform.GetWorldPosition(entity.Comp);
        var count = 0;

        foreach (var compType in args.Condition.Components.Values)
        {
            _lookup.GetEntitiesInRange(compType.Component.GetType(), entity.Comp.MapID, worldPos, args.Condition.Range, inRange);
            foreach (var ent in inRange)
            {
                if (args.Condition.Anchored && !Transform(ent).Anchored)
                    continue;

                count++;

                if (count >= args.Condition.Count)
                {
                    found = true;
                    break;
                }
            }
        }

        args.Result = found;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class NearbyComponentsCondition : EntityConditionBase<NearbyComponentsCondition>
{
    /// <summary>
    /// Does the entity need to be anchored.
    /// </summary>
    [DataField]
    public bool Anchored;

    [DataField]
    public int Count;

    [DataField(required: true)]
    public ComponentRegistry Components = default!;

    [DataField]
    public float Range = 10f;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => String.Empty;
}
