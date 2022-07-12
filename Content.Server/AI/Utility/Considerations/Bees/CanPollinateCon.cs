using Content.Server.AI.Tracking;
using Content.Server.AI.WorldState;
using Content.Server.AI.WorldState.States;
using Content.Server.Botany.Components;

namespace Content.Server.AI.Utility.Considerations.Bees
{
    public sealed class CanPollinateCon : Consideration
    {
        protected override float GetScore(Blackboard context)
        {
            var target = context.GetState<TargetEntityState>().GetValue();

            if (target == null || !IoCManager.Resolve<EntityManager>().TryGetComponent(target, out PlantHolderComponent? plantHolderComponent))
                return 0;

            if (IoCManager.Resolve<IEntityManager>().TryGetComponent(target, out RecentlyPollinatedComponent? recently))
                return 0f;

            if (plantHolderComponent.Age < 1)
                return 0f;

            if (plantHolderComponent.Age > 1)
                return 1f;

            return 0f;
        }
    }
}
