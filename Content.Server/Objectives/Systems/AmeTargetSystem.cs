using Content.Server.Ame.Components;
using Content.Shared.Objectives.Systems;

namespace Content.Server.Objectives.Systems;

public sealed partial class AmeTargetSystem : EntityTargetSystem<AmeControllerComponent>
{
    protected override bool ValidateEntity(Entity<AmeControllerComponent> entity)
    {
        return true;
    }
}
