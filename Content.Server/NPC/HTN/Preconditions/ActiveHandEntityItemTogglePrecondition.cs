using Content.Server.Hands.Systems;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server.NPC.HTN.Preconditions;


/// <summary>
/// Returns true if the item in the entity's hand has the same toggle state as Activated
/// </summary>
public sealed partial class ActiveHandEntityItemTogglePrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    /// <summary>
    /// Precondition returns true if this matches ItemToggle activated state
    /// </summary>
    [DataField(required: true)]
    public bool Activated;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue(NPCBlackboard.Owner, out EntityUid owner, _entManager))
            return false;

        if (!_entManager.System<HandsSystem>().TryGetActiveItem(owner, out var activeItem)
            || !_entManager.TryGetComponent<ItemToggleComponent>(activeItem, out var itemToggleComponent))
            return false;

        return itemToggleComponent.Activated == Activated;
    }
}
