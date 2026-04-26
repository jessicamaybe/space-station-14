using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PlaySoundOperator : HTNOperator
{
    [Dependency] private readonly AudioSystem _audio = default!;

    [DataField(required: true)]
    public SoundSpecifier? Sound;

    public override HTNOperatorStatus Update(Entity<HTNComponent> ent, NPCBlackboard blackboard, float frameTime)
    {
        var uid = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        _audio.PlayPvs(Sound, uid);

        return base.Update(ent, blackboard, frameTime);
    }
}
