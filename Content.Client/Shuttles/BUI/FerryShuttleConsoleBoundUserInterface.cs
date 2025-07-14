using Content.Client.Computer;
using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using JetBrains.Annotations;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class FerryShuttleConsoleBoundUserInterface : ComputerBoundUserInterface<FerryShuttleWindow, FerryShuttleConsoleBoundUserInterfaceStateState>
{
    public FerryShuttleConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
}
