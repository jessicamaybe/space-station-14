using Content.Shared._UM.Spiders.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This handles...
/// </summary>
public sealed class CocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedToolSystem _tools = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CocoonComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<CocoonComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<CocoonComponent, ActivateInWorldEvent>(OnInteract);
        SubscribeLocalEvent<CocoonComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);

        SubscribeLocalEvent<CocoonComponent, OnCocoonDestroyDoAfterEvent>(OnCocoonDestroy);
    }

    private void OnComponentInit(Entity<CocoonComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Contents = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }

    private void OnInteract(Entity<CocoonComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex || ent.Comp.Harvested)
            return;

        if (!HasComp<BroodmotherComponent>(args.User))
            return;

        args.Handled = true;
        StartBroodmotherAbsorbEnergy(ent, args.User);
    }

    private void OnGetVerbs(Entity<CocoonComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || ent.Comp.Harvested)
            return;

        if (!HasComp<BroodmotherComponent>(args.User))
            return;

        var user = args.User;

        InteractionVerb verb = new()
        {
            Act = () => StartBroodmotherAbsorbEnergy(ent, user),
            Text = "Abborb"
        };
        args.Verbs.Add(verb);
    }

    private void StartBroodmotherAbsorbEnergy(Entity<CocoonComponent> ent, Entity<BroodmotherComponent?> broodMother)
    {
        if (!Resolve(broodMother, ref broodMother.Comp))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, broodMother, 4f, new OnCocoonEnergyAbsorbDoAfterEvent(), broodMother, ent)
        {
            BreakOnMove = true,
            NeedHand = false,
            BreakOnDropItem = true,
            BreakOnWeightlessMove = true,
        });
    }

    private void OnInteractUsing(Entity<CocoonComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_tools.HasQuality(args.Used, ent.Comp.Quality))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, 5f, new OnCocoonDestroyDoAfterEvent(), ent, args.Target, args.Used)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnDropItem = true,
            BreakOnWeightlessMove = true,
        });
    }

    private void OnCocoonDestroy(Entity<CocoonComponent> ent, ref OnCocoonDestroyDoAfterEvent args)
    {
        if (args.Cancelled || args.Used == null)
            return;

        var removed = _container.EmptyContainer(ent.Comp.Contents, false, Transform(ent).Coordinates);
        args.Handled = true;
        PredictedDel(ent.Owner);
        _audio.PlayPredicted(ent.Comp.DestroySound, args.Used.Value, args.User);
    }
}
