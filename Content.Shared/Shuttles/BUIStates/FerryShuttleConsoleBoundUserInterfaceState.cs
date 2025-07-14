using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class FerryShuttleConsoleBoundUserInterfaceStateState : BoundUserInterfaceState
{

}

[Serializable, NetSerializable]
public enum FerryShuttleConsoleUiKey : byte
{
    Key,
}
