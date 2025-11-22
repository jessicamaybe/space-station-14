using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Magic.Events;

public sealed partial class MagicMissileSpellEvent : InstantActionEvent
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField]
    public EntProtoId Prototype = new EntProtoId("MagicMissile");


    [DataField]
    public bool PreventCollideWithCaster = true;

    /// <summary>
    /// The maximum amount of missiles to spawn
    /// </summary>
    [DataField]
    public int MaxMissiles = 5;

    /// <summary>
    /// Range to look for targets
    /// </summary>
    [DataField]
    public float Range = 7.0f;

    /// <summary>
    /// What component should they primarily target
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry TargetComponent = [];

    /// <summary>
    ///     Sound the event makes (this should be an action sound but i dont fucking know why it wont work)
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound;

}
