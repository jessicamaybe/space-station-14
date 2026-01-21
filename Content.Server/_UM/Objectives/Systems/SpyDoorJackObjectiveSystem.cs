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
        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<SpyDoorJackObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<SpyDoorJackObjectiveComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var access = _random.Pick(ent.Comp.Access);
        ent.Comp.RequiredAccess.Add(access);
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

        var title = "DoorJack " + _number.GetTarget(ent) + " doors with: " + accessNames + " access";
        _metaData.SetEntityName(ent, title, args.Meta);
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
        if (!_mind.TryGetObjectiveComp<SpyDoorJackObjectiveComponent>(ent, out var obj))
            return;

        if (!_accessReader.GetMainAccessReader(args.Door, out var accessReaderEnt))
            return;

        if (!TryComp<AccessReaderComponent>(accessReaderEnt, out var accessReaderComponent))
            return;

        var complete = false;

        foreach (var accessList in accessReaderComponent.AccessLists)
        {
           foreach (var access in accessList)
           {
               if (!obj.RequiredAccess.Any(set => set.Contains(access)))
                   continue;
               complete = true;
           }
        }

        if (complete)
            obj.JackedDoors += 1;
    }

}
