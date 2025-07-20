using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class FerryConsoleBoundUserInterfaceState : BoundUserInterfaceState
{

}

[Serializable, NetSerializable]
public enum FerryConsoleUiKey : byte
{
    Key,
}
