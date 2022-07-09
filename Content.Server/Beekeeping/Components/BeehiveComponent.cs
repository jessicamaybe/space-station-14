using Content.Shared.Containers.ItemSlots;

namespace Content.Server.Beekeeping.Components
{
    [RegisterComponent]
    public sealed class BeehiveComponent : Component
    {

        [DataField("QueenSlot")]
        public ItemSlot QueenSlot = new();

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("targetSolution")]
        public string TargetSolutionName = "hive";

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("updateRate")]
        public float UpdateRate = 5;

        public float AccumulatedTime;

        public int BeeCount;

        public bool BeingDrained;

        public bool HasQueen = true;
    }
}
