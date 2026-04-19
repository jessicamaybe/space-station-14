using System.ComponentModel.DataAnnotations;

namespace Content.Server.NPC.HTN;

public sealed partial class HTNWeightedRandomCompoundTask : HTNTask
{
    [DataField(required: true)]
    public Dictionary<string, float> Weights = new();
}
