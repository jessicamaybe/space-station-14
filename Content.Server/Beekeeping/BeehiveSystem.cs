using Content.Server.Beekeeping.Components;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Verbs;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Tag;
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
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BeehiveComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<BeehiveComponent, AttackedEvent>(OnAttacked);
            SubscribeLocalEvent<BeehiveComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<BeehiveComponent, GetVerbsEvent<AlternativeVerb>>(AddHarvestVerb);
            SubscribeLocalEvent<BeehiveComponent, HarvestFinishedEvent>(OnHarvestFinished);
            SubscribeLocalEvent<BeehiveComponent, HarvestFailedEvent>(OnHarvestFailed);
            SubscribeLocalEvent<BeehiveComponent, EntInsertedIntoContainerMessage>(OnQueenInserted);
            SubscribeLocalEvent<BeehiveComponent, EntRemovedFromContainerMessage>(OnQueenRemoved);
            SubscribeLocalEvent<BeehiveComponent, ContainerIsRemovingAttemptEvent>(OnRemoveAttempt);
        }

        public override void Update(float frameTime)
        {
            foreach (var hive in EntityManager.EntityQuery<BeehiveComponent>(false))
            {
                // Only do hive stuff if theres a queen inside
                if (hive.HasQueen)
                {
                    UpdatePlantCount(hive.Owner, hive);
                    hive.AccumulatedTime += frameTime;

                    while (hive.AccumulatedTime > hive.UpdateRate)
                    {
                        hive.AccumulatedTime -= hive.UpdateRate;

                        //Gives more honey and bees
                        if (!_solutionContainerSystem.TryGetSolution(hive.Owner, hive.TargetSolutionName, out var solution))
                            continue;
                        hive.BeeCount += 1;
                        _solutionContainerSystem.TryAddReagent(hive.Owner, solution, "Honey", hive.BeeCount * hive.PlantCount * 0.05,
                            out var accepted);

                    }

                    if (hive.BeeCount > 15)
                    {
                        hive.BeeCount = hive.BeeCount - 1;
                        SpawnBees(hive, 1, false);
                    }
                }
            }
        }
        public void OnComponentInit(EntityUid uid, BeehiveComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, "QueenSlot", component.QueenSlot);
            UpdatePlantCount(uid, component);
        }

        private void UpdatePlantCount(EntityUid uid, BeehiveComponent component)
        {
            component.PlantCount = 0;
            foreach (var entity in _lookup.GetEntitiesInRange(uid, 4.0f))
            {
                if (_tagSystem.HasTag(entity, "Plant"))
                {
                    component.PlantCount = component.PlantCount + 1;
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

        private void OnRemoveAttempt(EntityUid uid, BeehiveComponent component, ContainerIsRemovingAttemptEvent args)
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
                Priority = 2
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
                TargetCancelledEvent = new HarvestFailedEvent()
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

        private void OnHarvestFailed(EntityUid uid, BeehiveComponent component, HarvestFailedEvent ev)
        {
            component.BeingDrained = false;
        }

        private void OnExamined(EntityUid uid, BeehiveComponent component, ExaminedEvent args)
        {
            args.PushMarkup("Bees:");
            args.PushMarkup(component.BeeCount.ToString());
            args.PushMarkup("Honey:");
            args.PushMarkup(_solutionContainerSystem.GetReagentQuantity(component.Owner, "Honey").ToString());
            args.PushMarkup("Nearby Plants:");
            args.PushMarkup(component.PlantCount.ToString());
        }

        private void OnAttacked(EntityUid uid, BeehiveComponent component, AttackedEvent args)
        {
            var count = component.BeeCount / 4;
            SpawnBees(component, count, true);
            component.BeeCount -= count;


        }

        private void SpawnBees(BeehiveComponent component, int count, bool angry)
        {
            if (component.HasQueen)
            {
                while (count > 0)
                {
                    if (angry) _entityManager.SpawnEntity("MobAngryBee", Comp<TransformComponent>(component.Owner).Coordinates);
                    if (!angry) _entityManager.SpawnEntity("MobBee", Comp<TransformComponent>(component.Owner).Coordinates);
                    count -= count;
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

        private sealed class HarvestFailedEvent : EntityEventArgs
        {

        }
    }
}
