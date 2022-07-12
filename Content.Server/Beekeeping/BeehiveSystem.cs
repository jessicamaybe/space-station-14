using Content.Server.AI.Components;
using Content.Server.Beekeeping.Components;
using Content.Server.Botany.Components;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Verbs;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Player;

namespace Content.Server.Beekeeping
{

    public sealed class BeehiveSystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BeehiveComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<BeehiveComponent, AttackedEvent>(OnAttacked);
            SubscribeLocalEvent<BeehiveComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<BeehiveComponent, GetVerbsEvent<AlternativeVerb>>(AddHarvestVerb);
            SubscribeLocalEvent<BeehiveComponent, HarvestFinishedEvent>(OnHarvestFinished);
            SubscribeLocalEvent<BeehiveComponent, HarvestFailedEvent>(OnHarvestFailed);
            SubscribeLocalEvent<BeehiveComponent, InteractUsingEvent>(OnInteractUsing);
        }

        public override void Update(float frameTime)
        {
            foreach (var hive in EntityManager.EntityQuery<BeehiveComponent>())
            {
                // Only do hive stuff if theres a queen inside
                if (hive.HasQueen)
                {
                    hive.AccumulatedTime += frameTime;

                    while (hive.AccumulatedTime > hive.UpdateRate)
                    {
//                      UpdatePlantCount(hive.Owner, hive);
                        hive.AccumulatedTime -= hive.UpdateRate;

                        if (hive.BeeCount < hive.MaxBees)
                        {
                            hive.BeeCount += 1;
                            SpawnBees(hive, 1, false);
                        }

                        //Gives more honey
                        /*
                        if (!_solutionContainerSystem.TryGetSolution(hive.Owner, hive.TargetSolutionName, out var solution))
                            continue;

                        if (_solutionContainerSystem.GetReagentQuantity(hive.Owner, "Honey") < hive.MaxHoney)
                            _solutionContainerSystem.TryAddReagent(hive.Owner, solution, "Honey", hive.BeeCount * hive.PlantCount * 0.05, out var accepted);
                        */
                    }
                }
            }
        }
        public void OnComponentInit(EntityUid uid, BeehiveComponent component, ComponentInit args)
        {

        }

        private void UpdatePlantCount(EntityUid uid, BeehiveComponent component)
        {
            component.PlantCount = 0;
            foreach (var entity in _lookup.GetEntitiesInRange(uid, 4.0f))
            {
                if (!EntityManager.TryGetComponent(entity, out PlantHolderComponent? plant)) continue;
                if (plant.Age > 1)
                {
                    component.PlantCount += 1;
                }
            }
        }

        private void OnInteractUsing(EntityUid uid, BeehiveComponent component, InteractUsingEvent args)
        {
            if (_tagSystem.HasTag(args.Used, "QueenBee"))
            {
                if (!component.HasQueen)
                {
                    component.HasQueen = true;
                    EntityManager.QueueDeleteEntity(args.Used);

                    _popupSystem.PopupCursor(Loc.GetString("hive-queen-insert"), Filter.Entities(args.User));
                    return;
                }

                _popupSystem.PopupCursor(Loc.GetString("hive-queen-exists"), Filter.Entities(args.User));
            }
        }

        private void AddHarvestVerb(EntityUid uid, BeehiveComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (args.Using == null ||
                !args.CanInteract ||
                !component.HasQueen ||
                !EntityManager.HasComponent<RefillableSolutionComponent>(args.Using.Value))
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    AttemptHarvest(uid, args.User, args.Using.Value, component);
                },
                Text = Loc.GetString("hive-verb-harvest"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void AttemptHarvest(EntityUid uid, EntityUid userUid, EntityUid containerUid, BeehiveComponent? hive = null)
        {
            if (!Resolve(uid, ref hive))
                return;

            if (hive.BeingDrained)
            {
                _popupSystem.PopupEntity(Loc.GetString("hive-harvest-busy"), uid, Filter.Entities(userUid));
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("hive-harvest-begin"), uid, Filter.Entities(userUid), PopupType.Small);

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


            //sting if they aren't wearing a beesuit
            if (!_inventorySystem.TryGetSlotEntity(ev.UserUid, "outerClothing", out var outerClothing) || !EntityManager.HasComponent<BeeSuitComponent>(outerClothing)
                || !_inventorySystem.TryGetSlotEntity(ev.UserUid, "head", out var head) || !EntityManager.HasComponent<BeeSuitComponent>(head))
            {
                _popupSystem.PopupEntity(Loc.GetString("hive-harvest-stung"), ev.UserUid, Filter.Entities(ev.UserUid), PopupType.MediumCaution);
                _stunSystem.TryParalyze(ev.UserUid, TimeSpan.FromSeconds(3), true);
                return;
            }

            if (!_solutionContainerSystem.TryGetSolution(uid, component.TargetSolutionName, out var solution))
                return;
            if (!_solutionContainerSystem.TryGetRefillableSolution(ev.ContainerUid, out var targetSolution))
                return;

            var quantity = solution.TotalVolume;
            if (quantity == 0)
            {
                _popupSystem.PopupEntity(Loc.GetString("hive-harvest-empty"), ev.UserUid, Filter.Entities(ev.UserUid), PopupType.Medium);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("hive-harvest-success"), ev.UserUid, Filter.Entities(ev.UserUid), PopupType.Medium);

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
                    if (angry) EntityManager.SpawnEntity("MobAngryBee", Comp<TransformComponent>(component.Owner).Coordinates);
                    if (!angry)
                    {
                        var bee = EntityManager.SpawnEntity("MobHoneyBee", Comp<TransformComponent>(component.Owner).Coordinates.Offset(Vector2.One));
                        EntityManager.AddComponent<BeeComponent>(bee);
                        if (TryComp<BeeComponent>(bee, out var beeComponent))
                            beeComponent.Hive = component.Owner;
                    }
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
