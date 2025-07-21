using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class FerryConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool AllowSend;

}

[Serializable, NetSerializable]
public enum FerryConsoleUiKey : byte
{
    Key,
}
