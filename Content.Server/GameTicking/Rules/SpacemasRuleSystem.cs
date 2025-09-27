using Content.Server.GameTicking.Rules.Components;
using Content.Server.Spacemas;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;

namespace Content.Server.GameTicking.Rules;

public sealed class SpacemasRuleSystem : GameRuleSystem<SpacemasRuleComponent>
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Added(EntityUid uid, SpacemasRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        foreach (var station in _station.GetStations())
        {
            //its spacemas for everyone
            EnsureComp<SpacemasCheerComponent>(station);
        }
    }
}
