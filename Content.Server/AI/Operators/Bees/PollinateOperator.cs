using Content.Server.AI.EntitySystems;

namespace Content.Server.AI.Operators.Bees
{
    public sealed class PollinateOperator : AiOperator
    {
        private EntityUid _bee;
        private EntityUid _target;

        public PollinateOperator(EntityUid bee, EntityUid target)
        {
            _bee = bee;
            _target = target;
        }

        public override Outcome Execute(float frametime)
        {
            var PollinateSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<PollinateNearbySystem>();
            if (PollinateSystem.Pollinate(_bee, _target))
                return Outcome.Success;

            return Outcome.Failed;
        }
    }
}
