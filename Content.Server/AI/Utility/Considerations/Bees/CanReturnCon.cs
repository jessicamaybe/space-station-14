using Content.Server.AI.Components;
using Content.Server.AI.Tracking;
using Content.Server.AI.WorldState;
using Content.Server.AI.WorldState.States;
using Content.Server.Beekeeping.Components;

namespace Content.Server.AI.Utility.Considerations.Bees
{
    public sealed class CanReturnCon : Consideration
    {
        protected override float GetScore(Blackboard context)
        {
            var target = context.GetState<TargetEntityState>().GetValue();
            var owner = context.GetState<SelfState>().GetValue();


            if (target == null || !IoCManager.Resolve<EntityManager>().TryGetComponent(target, out BeehiveComponent? beehiveComponent))
                return 0f;

            if (!IoCManager.Resolve<IEntityManager>().TryGetComponent(owner, out BeeComponent? beeComponent))
                return 0f;

            if (beeComponent.full)
            {
                return 1f;
            }

            return 0f;

        }
    }
}
