using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;


namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class FerryShuttleConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private FerryShuttleWindow? _window;
    public FerryShuttleConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindowCenteredLeft<FerryShuttleWindow>();
        _window.OnLaunchPressed += LaunchPressed;
    }

    private void LaunchPressed()
    {
        SendMessage(new FerryShuttleSendShipMessage());
    }
}
