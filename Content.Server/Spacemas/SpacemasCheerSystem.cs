using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;

namespace Content.Server.Spacemas;


/// <summary>
/// This handles <see cref="SpacemasCheerComponent"/> and keeping track of the current levels of SpacemasCheer
/// </summary>
public sealed class SpacemasCheerSystem: EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpacemasCheerComponent, ComponentAdd>(OnAdd);
    }

    private void OnAdd(Entity<SpacemasCheerComponent> ent, ref ComponentAdd args)
    {
        ent.Comp.CheerScore = 0;
        var station = _station.GetOwningStation(ent);
        Log.Debug("Spacemas added to: " + station);
    }



}
