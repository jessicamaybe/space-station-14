using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;


namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class FerryConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private FerryShuttleWindow? _window;
    public FerryConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }
    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindowCenteredLeft<FerryShuttleWindow>();
        _window.OnLaunchPressed += LaunchPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FerryConsoleBoundUserInterfaceState bState)
            return;

        _window?.UpdateState(bState);
    }
    private void LaunchPressed()
    {
        SendMessage(new FerrySendShipMessage());
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Close();
            _window = null;
        }
    }
}
