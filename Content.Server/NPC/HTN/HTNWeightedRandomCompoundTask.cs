using System.ComponentModel.DataAnnotations;

namespace Content.Server.NPC.HTN;

/// <summary>
/// Selects a HTNCompoundTask based off of random weights
/// </summary>
public sealed partial class HTNWeightedRandomCompoundTask : HTNTask
{
    [DataField(required: true)]
    public Dictionary<string, float> Weights = new();
}
