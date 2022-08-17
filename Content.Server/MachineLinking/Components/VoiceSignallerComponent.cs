using Content.Shared.MachineLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.MachineLinking.Components
{
    /// <summary>
    /// Sends a trigger when a set string is heard nearby
    /// </summary>
    [RegisterComponent]
    public sealed class VoiceSignallerComponent : Component
    {
        /// <summary>
        ///     The port that gets signaled when the switch turns on.
        /// </summary>
        [DataField("port", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string Port = "Pressed";

        public string? ActivationPhrase;

        [DataField("activateSound")]
        public SoundSpecifier ActivateSound { get; set; } = new SoundPathSpecifier("/Audio/Effects/RingtoneNotes/a.ogg");
    }
}
