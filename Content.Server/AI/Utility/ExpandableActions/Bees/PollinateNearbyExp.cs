using Content.Server.AI.Components;
using Content.Server.AI.EntitySystems;
using Content.Server.AI.Utility.Actions;
using Content.Server.AI.Utility.Actions.Bees;
using Content.Server.AI.Utility.Considerations;
using Content.Server.AI.Utility.Considerations.Combat.Melee;
using Content.Server.AI.WorldState;
using Content.Server.AI.WorldState.States;
using Content.Server.AI.Utility.Considerations.ActionBlocker;
using Content.Server.AI.Utility.Considerations.Bees;

namespace Content.Server.AI.Utility.ExpandableActions.Bees
{
    public class PollinateNearbyExp : ExpandableUtilityAction
    {
        public override float Bonus => 30;
        protected override IReadOnlyCollection<Func<float>> GetCommonConsiderations(Blackboard context)
        {

            var considerationsManager = IoCManager.Resolve<ConsiderationsManager>();

            return new[]
            {
                considerationsManager.Get<CanMoveCon>()
                    .BoolCurve(context),
            };
        }
        public override IEnumerable<UtilityAction> GetActions(Blackboard context)
        {
            var owner = context.GetState<SelfState>().GetValue();
            if (!IoCManager.Resolve<IEntityManager>().TryGetComponent(owner, out AiControllerComponent? controller))
            {
                throw new InvalidOperationException();
            }

            yield return new PollinateNearby()
            {
                Owner = owner, Target = EntitySystem.Get<PollinateNearbySystem>().GetNearbyPlants(Owner), Bonus = Bonus
            };
        }
    }
}
