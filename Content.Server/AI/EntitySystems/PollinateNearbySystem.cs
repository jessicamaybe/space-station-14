using Content.Server.AI.Tracking;
using Content.Server.Botany.Components;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Server.Chat;
using Content.Shared.MobState.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Robust.Shared.Player;
using Robust.Shared.Audio;


namespace Content.Server.AI.EntitySystems
{
    public sealed class PollinateNearbySystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public EntityUid GetNearbyPlants(EntityUid bee, float range = 4)
        {
            foreach (var entity in EntitySystem.Get<EntityLookupSystem>().GetEntitiesInRange(bee, range))
            {
                if (HasComp<PlantHolderComponent>(entity) && !HasComp<RecentlyPollinatedComponent>(entity))
                {
                    return entity;
                }
            }
            return default;
        }

        public bool Pollinate(EntityUid bee, EntityUid target)
        {
            if (HasComp<BeingPollinatedComponent>(target))
                return false;
            if (!TryComp<PlantHolderComponent>(target, out var plant))
                return false;
            if (!_solutionContainerSystem.TryGetSolution(bee, "pollen", out var solution))
                return false;
            if (_solutionContainerSystem.GetReagentQuantity(bee, "Honey") > 9)
                return false;
            if (plant.Age < 1)
                return false;

            if (plant.Age > 1)
            {
                EnsureComp<RecentlyPollinatedComponent>(target);

                _solutionContainerSystem.TryAddReagent(bee, solution, "Honey", 5, out var accepted);

                _popupSystem.PopupEntity("Buzz!", target, Filter.Pvs(target));
                return true;
            }
            return false;
        }

    }
}
