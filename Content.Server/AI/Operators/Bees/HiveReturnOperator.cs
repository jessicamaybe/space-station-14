using Content.Server.AI.EntitySystems;

namespace Content.Server.AI.Operators.Bees
{


    public sealed class HiveReturnOperator : AiOperator
    {
        private EntityUid _bee;
        private EntityUid _target;

        public HiveReturnOperator(EntityUid bee, EntityUid target)
        {
            _bee = bee;
            _target = target;
        }

        public override Outcome Execute(float frametime)
        {
            var hiveReturnSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<HiveReturnSystem>();
            if (hiveReturnSystem.HiveReturn(_bee, _target))
                return Outcome.Success;

            return Outcome.Failed;
        }
    }
}
