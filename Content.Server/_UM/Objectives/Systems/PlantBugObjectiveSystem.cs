using System.Linq;
using Content.Server._UM.Antags.Spy;
using Content.Server._UM.Objectives.Components;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Whitelist;

namespace Content.Server._UM.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class PlantBugObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpyComponent, AfterMachineBugInsertEvent>(AfterMachineBugInsert);
        SubscribeLocalEvent<PlantBugObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void AfterMachineBugInsert(Entity<SpyComponent> ent, ref AfterMachineBugInsertEvent args)
    {
        var mind = _mind.GetMind(ent);
        if (mind == null || !TryComp<MindComponent>(mind, out var mindComponent))
            return;

        foreach (var objective in mindComponent.Objectives)
        {
            CheckObjectiveComplete(objective, args.Target);
        }
    }

    private void CheckObjectiveComplete(Entity<PlantBugObjectiveComponent?> obj, EntityUid target)
    {
        if (!Resolve(obj.Owner, ref obj.Comp))
            return;

        if (obj.Comp.Whitelist != null && obj.Comp.Whitelist.Components != null)
            Log.Debug("whitelist: " + obj.Comp.Whitelist.Components.First());

        if (_entityWhitelist.IsWhitelistFail(obj.Comp.Whitelist, target) && obj.Comp.Whitelist != null)
            return;

        obj.Comp.Complete = true;
    }

    private void OnGetProgress(Entity<PlantBugObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = ent.Comp.Complete ? 1.0f : 0.0f;
    }
}
