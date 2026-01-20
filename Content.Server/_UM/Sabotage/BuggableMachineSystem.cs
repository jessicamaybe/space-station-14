using Content.Server.Containers;
using Content.Shared._UM.Sabotage;
using Content.Shared._UM.Sabotage.Components;
using Content.Shared.Construction;
using Content.Shared.Containers.ItemSlots;
using Robust.Server.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._UM.Sabotage;

/// <summary>
/// This handles...
/// </summary>
public sealed class BuggableMachineSystem : SharedBuggableMachineSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BuggableMachineComponent, MachineDeconstructedEvent>(OnMachineDeconstructed,
            before: [typeof(EmptyOnMachineDeconstructSystem), typeof(ItemSlotsSystem)]);
    }

    private void OnMachineDeconstructed(Entity<BuggableMachineComponent> ent, ref MachineDeconstructedEvent args)
    {
        _container.CleanContainer(ent.Comp.InstalledBugs);
        _audio.PlayPvs(ent.Comp.BrokenSound, Transform(ent).Coordinates);
    }

}
