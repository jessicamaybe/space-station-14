namespace Content.Server.AI.Tracking
{
    [RegisterComponent]
    public sealed class RecentlyPollinatedComponent : Component
    {
        public float Accumulator = 0f;

        public TimeSpan RemoveTime = TimeSpan.FromMinutes(1);
    }

}
