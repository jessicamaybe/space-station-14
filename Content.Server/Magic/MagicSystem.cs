using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Physics.Components;
using Content.Shared.Magic;
using Content.Shared.Magic.Events;
using Content.Shared.Mind;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Magic;

public sealed class MagicSystem : SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly ProtoId<TagPrototype> InvalidForSurvivorAntagTag = "InvalidForSurvivorAntag";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MagicMissileSpellEvent>(OnMagicMissile);
    }

    public override void OnVoidApplause(VoidApplauseSpellEvent ev)
    {
        base.OnVoidApplause(ev);

        _chat.TryEmoteWithChat(ev.Performer, ev.Emote);

        var perfXForm = Transform(ev.Performer);
        var targetXForm = Transform(ev.Target);

        Spawn(ev.Effect, perfXForm.Coordinates);
        Spawn(ev.Effect, targetXForm.Coordinates);
    }

    protected override void OnRandomGlobalSpawnSpell(RandomGlobalSpawnSpellEvent ev)
    {
        base.OnRandomGlobalSpawnSpell(ev);

        if (!ev.MakeSurvivorAntagonist)
            return;

        if (_mind.TryGetMind(ev.Performer, out var mind, out _) && !_tag.HasTag(mind, InvalidForSurvivorAntagTag))
            _tag.AddTag(mind, InvalidForSurvivorAntagTag);

        EntProtoId survivorRule = "Survivor";

        if (!_gameTicker.IsGameRuleActive<SurvivorRuleComponent>())
            _gameTicker.StartGameRule(survivorRule);
    }


    /// <summary>
    /// Handles the instant action (i.e. on the caster) attempting to spawn an entity.
    /// </summary>
    private void OnMagicMissile(MagicMissileSpellEvent args)
    {
        if (args.Handled)
            return;

        var transform = Transform(args.Performer);

        var compType = _random.Pick(args.TargetComponent.Values).Component.GetType();

        HashSet<Entity<IComponent>> chasetargets = new();
        chasetargets.Clear();

        _lookup.GetEntitiesInRange(compType, _transform.GetMapCoordinates(transform), args.Range, chasetargets, LookupFlags.Uncontained);

        var count = 0;

        foreach (var target in chasetargets)
        {
            if (args.Performer == target.Owner)
                continue;

            if (count > args.MaxMissiles)
                continue;

            var missile = Spawn(args.Prototype, transform.Coordinates);
            EnsureComp<ChasingWalkComponent>(missile, out var chasingComp);
            chasingComp.ChasingEntity = target.Owner;
            chasingComp.ImpulseInterval = 0.5f;
            chasingComp.RotateWithImpulse = true;
            
            EnsureComp<ProjectileComponent>(missile, out var projectileComponent);
            projectileComponent.Shooter = args.Performer;

            count++;
        }

        args.Handled = true;
    }
}
