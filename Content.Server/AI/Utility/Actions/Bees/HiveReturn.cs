using Content.Server.AI.Operators;
using Content.Server.AI.Operators.Bees;
using Content.Server.AI.Operators.Generic;
using Content.Server.AI.Operators.Movement;
using Content.Server.AI.WorldState;
using Content.Server.AI.Utility.Considerations.Containers;
using Content.Server.AI.Utility.Considerations;
using Content.Server.AI.Utility.Considerations.ActionBlocker;
using Content.Server.AI.Utility.Considerations.Bees;
using Content.Server.AI.WorldState.States.Movement;
using Content.Server.AI.WorldState.States;

namespace Content.Server.AI.Utility.Actions.Bees
{
    public sealed class HiveReturn : UtilityAction
    {
        public EntityUid Target { get; set; } = default;

        public override void SetupOperators(Blackboard context)
        {
            MoveToEntityOperator moveOperator = new MoveToEntityOperator(Owner, Target);
            float waitTime = 3f;

            ActionOperators = new Queue<AiOperator>(new AiOperator[]
            {
                moveOperator,
                new HiveReturnOperator(Owner, Target),
            });
        }

        protected override void UpdateBlackboard(Blackboard context)
        {
            base.UpdateBlackboard(context);
            context.GetState<TargetEntityState>().SetValue(Target);
            context.GetState<MoveTargetState>().SetValue(Target);
        }
        protected override IReadOnlyCollection<Func<float>> GetConsiderations(Blackboard context)
        {
            var considerationsManager = IoCManager.Resolve<ConsiderationsManager>();

            return new[]
            {
                considerationsManager.Get<CanMoveCon>()
                    .BoolCurve(context),
                considerationsManager.Get<CanReturnCon>()
                    .BoolCurve(context),
            };
        }
    }
}
