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
        public float UpdateRate = 15;

        public float AccumulatedTime;

        public int MaxBees = 25;
        public int MaxHoney = 100;

        public int BeeCount;
        public int ExternalBeeCount;

        public int PlantCount;

        public bool BeingDrained;

        public bool HasQueen;
    }
}
