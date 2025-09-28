using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.ParcelWrap.Components;
using Content.Shared.ParcelWrap.Systems;
using Content.Shared.Storage;

namespace Content.Server.Spacemas;


/// <summary>
/// This handles <see cref="SpacemasCheerComponent"/> and keeping track of the current levels of SpacemasCheer
/// </summary>
public sealed class SpacemasCheerSystem: EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpacemasCheerComponent, ComponentAdd>(OnAdd);
        SubscribeLocalEvent<SpacemasCheerComponent, SpacemasScoreChangedEvent>(OnSpacemasScoreChanged);
        SubscribeLocalEvent<SpacemasScoreComponent, ParcelWrapItemDoAfterEvent>(OnWrapItem);
        SubscribeLocalEvent<SpacemasScoreComponent, UnwrapWrappedParcelDoAfterEvent>(OnUnwrapItem);
        SubscribeLocalEvent<SpacemasScoreComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnAdd(Entity<SpacemasCheerComponent> ent, ref ComponentAdd args)
    {
        //ent.Comp.CheerScore = 0;
        var station = _station.GetOwningStation(ent);
        Log.Debug("Spacemas added to: " + station);
    }

    private void OnSpacemasScoreChanged(Entity<SpacemasCheerComponent> ent, ref SpacemasScoreChangedEvent args)
    {
        Log.Debug("Spacemas Score changed to: " + ent.Comp.CheerScore);

        if (ent.Comp.CheerScore >= ent.Comp.SantaThreshold && !ent.Comp.SantaSpawned)
        {
            _gameTicker.AddGameRule(ent.Comp.SantaRule);
            ent.Comp.SantaSpawned = true;
        }

        if (ent.Comp.CheerScore <= ent.Comp.GrinchThreshold && !ent.Comp.GrinchSpawned)
        {
            _gameTicker.AddGameRule(ent.Comp.GrinchRule);
            ent.Comp.GrinchSpawned = true;
        }
    }

    public bool GetSpacemasComponent(EntityUid uid, out Entity<SpacemasCheerComponent>? entity)
    {
        var station = _station.GetOwningStation(uid);
        if (!TryComp(station, out SpacemasCheerComponent? cheerComponent))
        {
            entity = null!;
            return false;
        }

        entity = new Entity<SpacemasCheerComponent>(station.Value, cheerComponent);
        return true;
    }

    private void OnWrapItem(Entity<SpacemasScoreComponent> ent, ref ParcelWrapItemDoAfterEvent args)
    {
        if (!GetSpacemasComponent(ent.Owner, out var cheerEntity))
            return;

        if (cheerEntity == null)
            return;
        cheerEntity.Value.Comp.CheerScore += ent.Comp.CheerScore;

        var scoreChangedEvent = new SpacemasScoreChangedEvent();
        RaiseLocalEvent(cheerEntity.Value, scoreChangedEvent, false);
    }

    private void OnUnwrapItem(Entity<SpacemasScoreComponent> ent, ref UnwrapWrappedParcelDoAfterEvent args)
    {
        if (args.Target == null)
            return;

        if (!GetSpacemasComponent(args.Target.Value, out var cheerEntity))
            return;
        if (cheerEntity == null)
            return;

        cheerEntity.Value.Comp.CheerScore += ent.Comp.CheerScore;

        var scoreChangedEvent = new SpacemasScoreChangedEvent();
        RaiseLocalEvent(cheerEntity.Value, scoreChangedEvent, false);
    }

    private void OnUseInHand(Entity<SpacemasScoreComponent> ent, ref UseInHandEvent args)
    {
        if (!GetSpacemasComponent(args.User, out var cheerEntity))
            return;
        if (cheerEntity == null)
            return;

        cheerEntity.Value.Comp.CheerScore += ent.Comp.CheerScore;

        var scoreChangedEvent = new SpacemasScoreChangedEvent();
        RaiseLocalEvent(cheerEntity.Value, scoreChangedEvent, false);
    }

}

public sealed class SpacemasScoreChangedEvent : EntityEventArgs
{
}
