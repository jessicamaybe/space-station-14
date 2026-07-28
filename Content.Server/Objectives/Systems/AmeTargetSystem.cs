using Content.Server.Ame.Components;
using Content.Shared.Objectives.Systems;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Target system that looks for entities with AmeControllerComponent
/// </summary>
public sealed partial class AmeTargetSystem : EntityTargetSystem<AmeControllerComponent>
{
    protected override bool ValidateEntity(Entity<AmeControllerComponent> entity)
    {
        return true;
    }
}

public sealed partial class AmePool : EntityPool<AmeTargetSystem>;
