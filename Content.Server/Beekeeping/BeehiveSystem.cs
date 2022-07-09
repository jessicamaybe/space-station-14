using Content.Server.Animals.Components;
using Content.Server.Beekeeping.Components;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Map;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Nutrition.Components;
using Content.Shared.Verbs;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Coordinates;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server.Beekeeping
{

    public sealed class BeehiveSystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BeehiveComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<BeehiveComponent, AttackedEvent>(OnAttacked);
            SubscribeLocalEvent<BeehiveComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<BeehiveComponent, GetVerbsEvent<AlternativeVerb>>(AddHarvestVerb);
            SubscribeLocalEvent<BeehiveComponent, HarvestFinishedEvent>(OnHarvestFinished);
            SubscribeLocalEvent<BeehiveComponent, EntInsertedIntoContainerMessage>(OnQueenInserted);
            SubscribeLocalEvent<BeehiveComponent, EntRemovedFromContainerMessage>(OnQueenRemoved);
            SubscribeLocalEvent<BeehiveComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        }

        public override void Update(float frameTime)
        {
            foreach (var hive in EntityManager.EntityQuery<BeehiveComponent>(false))
            {
                if (hive.HasQueen)
                {
                    hive.AccumulatedTime += frameTime;

                    while (hive.AccumulatedTime > hive.UpdateRate)
                    {
                        hive.AccumulatedTime -= hive.UpdateRate;

                        if (!_solutionContainerSystem.TryGetSolution(hive.Owner, hive.TargetSolutionName,
                                out var solution))
                            continue;

                        hive.BeeCount += 1;
                        _solutionContainerSystem.TryAddReagent(hive.Owner, solution, "Honey", hive.BeeCount * 0.25,
                            out var accepted);

                    }
                }

            }
        }

        private void OnQueenInserted(EntityUid uid, BeehiveComponent component, ContainerModifiedMessage args)
        {
            if (!component.Initialized)
                return;
            if (args.Container.ID != component.QueenSlot.ID)
                return;

            component.HasQueen = true;

        }

        private void OnQueenRemoved(EntityUid uid, BeehiveComponent component, ContainerModifiedMessage args)
        {
            if (args.Container.ID != component.QueenSlot.ID)
                return;

            component.HasQueen = false;
        }

        private void OnInsertAttempt(EntityUid uid, BeehiveComponent component, ContainerIsInsertingAttemptEvent args)
        {

        }

        private void AddHarvestVerb(EntityUid uid, BeehiveComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (args.Using == null ||
                !args.CanInteract ||
                !_entityManager.HasComponent<RefillableSolutionComponent>(args.Using.Value))
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    AttemptHarvest(uid, args.User, args.Using.Value, component);
                },
                Text = "Harvest",
                Priority = 1
            };
            args.Verbs.Add(verb);
        }

        private void AttemptHarvest(EntityUid uid, EntityUid userUid, EntityUid containerUid,
            BeehiveComponent? hive = null)
        {
            if (!Resolve(uid, ref hive))
                return;

            if (hive.BeingDrained)
            {
                _popupSystem.PopupEntity("already being harvested", uid, Filter.Entities(userUid));
                return;
            }

            hive.BeingDrained = true;

            var doargs = new DoAfterEventArgs(userUid, 5, default, uid)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                BreakOnStun = true,
                BreakOnTargetMove = true,
                MovementThreshold = 1.0f,
                TargetFinishedEvent = new HarvestFinishedEvent(userUid, containerUid),
                //TargetCancelledEvent = new HarvestFailEvent()
            };
            _doAfterSystem.DoAfter(doargs);
        }

        private void OnHarvestFinished(EntityUid uid, BeehiveComponent component, HarvestFinishedEvent ev)
        {
            component.BeingDrained = false;

            if (!_solutionContainerSystem.TryGetSolution(uid, component.TargetSolutionName, out var solution))
                return;
            if (!_solutionContainerSystem.TryGetRefillableSolution(ev.ContainerUid, out var targetSolution))
                return;

            var quantity = solution.TotalVolume;
            if (quantity == 0)
            {
                return;
            }

            if (quantity > targetSolution.AvailableVolume)
                quantity = targetSolution.AvailableVolume;
            var split = _solutionContainerSystem.SplitSolution(uid, solution, quantity);
            _solutionContainerSystem.TryAddSolution(ev.ContainerUid, targetSolution, split);


        }
        public void OnComponentInit(EntityUid uid, BeehiveComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, "QueenSlot", component.QueenSlot);

        }

        private void OnExamined(EntityUid uid, BeehiveComponent component, ExaminedEvent args)
        {
            args.PushMarkup("Bees:");
            args.PushMarkup(component.BeeCount.ToString());
            if (!_solutionContainerSystem.TryGetSolution(component.Owner, component.TargetSolutionName,
                    out var solution)) { }
            args.PushMarkup("Honey:");
            args.PushMarkup(_solutionContainerSystem.GetReagentQuantity(component.Owner, "Honey").ToString());
        }

        private void OnAttacked(EntityUid uid, BeehiveComponent component, AttackedEvent args)
        {
            var count = component.BeeCount / 4;
            SpawnBees(uid, component, count);
            component.BeeCount -= count;


        }

        private void SpawnBees(EntityUid uid, BeehiveComponent component, int count)
        {
            if (component.HasQueen)
            {
                while (count > 0)
                {
                    _entityManager.SpawnEntity("MobAngryBee",
                            Comp<TransformComponent>(component.Owner).Coordinates);
                    count = count - 1;
                }
            }
        }

        private sealed class HarvestFinishedEvent : EntityEventArgs
        {
            public EntityUid UserUid;
            public EntityUid ContainerUid;

            public HarvestFinishedEvent(EntityUid userUid, EntityUid containerUid)
            {
                UserUid = userUid;
                ContainerUid = containerUid;
            }
        }
    }
}
