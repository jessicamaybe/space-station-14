using System.Linq;
using Content.Server._UM.Antags.Spy;
using Content.Server._UM.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Server._UM.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpyDoorJackObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SpyComponent, InstalledDoorJackEvent>(AfterDoorJack);

        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, RequirementCheckEvent>(OnRequirementCheck);
        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnRequirementCheck(Entity<SpyDoorJackObjectiveComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        Log.Debug("Checking requirementdoor");

        //make sure we aren't creating the same objective twice
        foreach (var objective in args.Mind.Objectives)
        {
            if (!TryComp<SpyDoorJackObjectiveComponent>(objective, out var existingjack))
                continue;

            foreach (var accessList in existingjack.RequiredAccess)
            {
                foreach (var access in accessList)
                {
                    ent.Comp.Access.RemoveAll(set => set.Contains(access));
                }
            }
        }

        if (ent.Comp.Access.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var pickedAccess = _random.Pick(ent.Comp.Access);
        ent.Comp.RequiredAccess.Add(pickedAccess);
    }

    private void OnAssigned(Entity<SpyDoorJackObjectiveComponent> ent, ref ObjectiveAssignedEvent args)
    {

    }

    private void OnAfterAssign(Entity<SpyDoorJackObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        var accessNames = "";

        foreach (var access in ent.Comp.RequiredAccess)
        {
            foreach (var access2 in access)
            {
                accessNames += " " + access2.Id;
            }
        }

        var number = _number.GetTarget(ent);
        var title = "";
        if (number > 1)
            title = "Doorjack " + number + " doors with: ";
        else if (number == 1)
            title = "Doorjack " + number + " door with: ";

        var description = accessNames + " access";
        _metaData.SetEntityName(ent, title, args.Meta);
        _metaData.SetEntityDescription(ent, description, args.Meta);
    }

    private void OnGetProgress(Entity<SpyDoorJackObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(ent);

        // prevent divide-by-zero
        if (target == 0)
        {
            args.Progress = 1f;
            return;
        }
        args.Progress = MathF.Min(ent.Comp.JackedDoors / (float) target, 1f);
    }

    private void AfterDoorJack(Entity<SpyComponent> ent, ref InstalledDoorJackEvent args)
    {
        var mind = _mind.GetMind(ent);
        if (mind == null || !TryComp<MindComponent>(mind, out var mindComponent))
            return;

        //Have to loop through each objective on mind incase there are two doorjacks
        foreach (var objective in mindComponent.Objectives)
        {
            CheckObjectiveComplete(objective, args.Door);
        }
    }

    private void CheckObjectiveComplete(Entity<SpyDoorJackObjectiveComponent?> obj, EntityUid target)
    {
        if (!Resolve(obj.Owner, ref obj.Comp))
            return;

        if (!_accessReader.GetMainAccessReader(target, out var accessReaderEnt))
            return;

        if (!TryComp<AccessReaderComponent>(accessReaderEnt, out var accessReaderComponent))
            return;

        var complete = false;

        foreach (var accessList in accessReaderComponent.AccessLists)
        {
            foreach (var access in accessList)
            {
                if (!obj.Comp.RequiredAccess.Any(set => set.Contains(access)))
                    continue;
                complete = true;
            }
        }

        if (complete)
            obj.Comp.JackedDoors += 1;

    }

}
