using Content.Server.Administration;
using Content.Server.Chat;
using Content.Server.MachineLinking.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;

namespace Content.Server.MachineLinking.System
{
    public sealed class VoiceSignallerSystem : EntitySystem
    {
        [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
        [Dependency] private readonly SignalLinkerSystem _signalSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

        public override void Initialize()
        {

            SubscribeLocalEvent<VoiceSignallerComponent, ChatMessageHeardNearbyEvent>(OnChatMessageHeard);
            SubscribeLocalEvent<VoiceSignallerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<VoiceSignallerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        }

        private void OnInit(EntityUid uid, VoiceSignallerComponent component, ComponentInit args)
        {
            _signalSystem.EnsureTransmitterPorts(uid, component.Port);
        }

        private void OnChatMessageHeard(EntityUid uid, VoiceSignallerComponent component, ChatMessageHeardNearbyEvent args)
        {
            if (args.Message == component.ActivationPhrase)
            {
                _signalSystem.InvokePort(uid, component.Port);
                _audioSystem.PlayPvs(component.ActivateSound, component.Owner);
            }
        }

        private void OnGetAltVerbs(EntityUid uid, VoiceSignallerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (!TryComp<ActorComponent>(args.User, out var actorComponent))
                return;

            args.Verbs.Add(new AlternativeVerb()
            {
                Text = Loc.GetString("Change activation string"),
                Act = () => _quickDialog.OpenDialog(actorComponent.PlayerSession, "Voice activation string", "Text to trigger", (string phrase) =>
                {
                    component.ActivationPhrase = phrase;
                }),
                Priority = 1
            });

        }
    }
}
