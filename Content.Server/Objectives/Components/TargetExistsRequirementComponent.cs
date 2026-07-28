using Content.Server.Objectives.Systems;
using Content.Shared.EntityConditions;
using Content.Shared.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Ensures that a type of entity exists before assigning this objective
/// </summary>
[RegisterComponent, Access(typeof(TargetExistsRequirementSystem))]
public sealed partial class TargetExistsRequirementComponent : Component
{
    /// <summary>
    /// A pool to check entities from
    /// </summary>
    [DataField]
    public IEntityPool Pool = new AmePool();

    /// <summary>
    /// EntityConditions to apply to <see cref="Pool"/>.
    /// If these conditions pass the entity is valid.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public EntityCondition[]? Conditions;
}
