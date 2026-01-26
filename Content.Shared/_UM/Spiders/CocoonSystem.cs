using Content.Shared._UM.Spiders.Components;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This handles...
/// </summary>
public sealed class CocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedToolSystem _tools = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CocoonComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CocoonComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnComponentInit(Entity<CocoonComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Contents = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }

    private void OnInteractUsing(Entity<CocoonComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        foreach (var entity in ent.Comp.Contents.ContainedEntities)
        {
            Log.Debug(MetaData(entity).EntityName);
        }

        if (!_tools.HasQuality(args.Used, ent.Comp.Quality))
            return;

        var removed = _container.EmptyContainer(ent.Comp.Contents, false, Transform(ent).Coordinates);
        args.Handled = true;
        PredictedDel(ent.Owner);
    }
}
