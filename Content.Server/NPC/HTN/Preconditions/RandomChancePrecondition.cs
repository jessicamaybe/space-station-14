using Robust.Shared.Random;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Uses random probabilities as a precondition
/// </summary>
public sealed partial class RandomChancePrecondition : HTNPrecondition
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Chance of returning true
    /// </summary>
    [DataField(required: true)]
    public float Chance;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        return _random.Prob(Chance);
    }
}
