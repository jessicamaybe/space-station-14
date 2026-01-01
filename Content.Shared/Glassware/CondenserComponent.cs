using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared.Glassware;

[RegisterComponent, NetworkedComponent]
public sealed partial class CondenserComponent: Component
{
    private List<ReagentQuantity> oldReagents;

}
