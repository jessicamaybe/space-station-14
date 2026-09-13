using Robust.Client.Graphics;

namespace Content.Client.FunnyWall;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class FunnyWallSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {

        base.Initialize();

        _overlayManager.AddOverlay(new FunnyWallOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayManager.RemoveOverlay<FunnyWallOverlay>();
    }
}
