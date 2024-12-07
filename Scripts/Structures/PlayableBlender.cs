using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Neeto
{
    public struct PlayableBlender : IPlayable
    {
        //public static implicit operator Playable(PlayableBlender _) => _.source;

        const int PREV = 0;
        const int NEXT = 1;

        public PlayableGraph graph { get; private set; }
        public Playable source { get; private set; }
        public Playable current
        {
            get => source.GetInput(0);
            set
            {
                if (current.IsValid())
                    source.DisconnectInput(0);
                source.ConnectInput(0, value, 0, 1);
            }
        }

        public PlayableBlender(PlayableGraph _graph)
        {
            source = Playable.Create(graph = _graph, 1);
        }
        public void Blend(Playable next, CancellationToken token, float duration = .15f)
        {
            if (!current.IsValid() || duration <= 0f)
            {
                current = next;
                return;
            }

            var mixer = AnimationMixerPlayable.Create(graph, 2);
            var prev = current;
            current = mixer;
            mixer.ConnectInput(PREV, prev, 0, 1);
            mixer.ConnectInput(NEXT, next, 0, 0);
            graph.Evaluate();

            Phase.Start(duration, PlayerLoopTiming.Update, token, t =>
            {
                mixer.SetInputWeight(PREV, 1 - t);
                mixer.SetInputWeight(NEXT, t);
            });
        }

        public PlayableHandle GetHandle() => source.GetHandle();
    }

}