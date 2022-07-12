using Content.Server.AI.Components;
using Content.Server.AI.Tracking;
using Content.Server.Beekeeping.Components;
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
    public sealed class HiveReturnSystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public bool HiveReturn(EntityUid bee, EntityUid target)
        {
            if (!TryComp<BeehiveComponent>(target, out var hive))
                return false;
            if (!TryComp<BeeComponent>(bee, out var beeComponent))
                return false;
            if (!_solutionContainerSystem.TryGetSolution(bee, "pollen", out var beesolution))
                return false;
            if (!_solutionContainerSystem.TryGetSolution(target, "hive", out var hivesolution))
                return false;

            if (beesolution.CurrentVolume > 5)
            {
                _solutionContainerSystem.TryAddReagent(target, hivesolution, "Honey", beesolution.CurrentVolume, out var accepted);
                _solutionContainerSystem.TryRemoveReagent(bee, beesolution, "Honey", beesolution.CurrentVolume);
                _popupSystem.PopupEntity("Returned Honey to hive", target, Filter.Pvs(target));
                beeComponent.full = false;
                return true;
            }

            return false;

        }
    }
}
