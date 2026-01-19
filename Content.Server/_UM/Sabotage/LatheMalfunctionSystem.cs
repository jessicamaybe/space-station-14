using Content.Server.Lathe;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Lathe;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class LatheMalfunctionSystem : EntitySystem
{
    [Dependency] private readonly SharedLatheSystem _latheSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LatheComponent, MachineMalfunctionEvent>(OnMachineMalfunction);
    }

    private void OnMachineMalfunction(Entity<LatheComponent> ent, ref MachineMalfunctionEvent args)
    {

    }
}
