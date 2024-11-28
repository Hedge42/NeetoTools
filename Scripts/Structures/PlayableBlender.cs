using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Neeto
{
    public class PlayableBlender
    {
        public PlayableGraph graph { get; private set; }
        const int PREV = 0;
        const int NEXT = 1;

        PlayableOutput output;
        public Playable source
        {
            get => output.GetSourcePlayable();
            private set => output.SetSourcePlayable(value);
        }

        public PlayableBlender(Animator animator, PlayableGraph graph)
        {
            this.graph = graph;
            output = AnimationPlayableOutput.Create(graph, animator.name, animator);
        }
        public void Blend(Playable next, CancellationToken token, float duration = .15f)
        {
            if (!source.IsValid() || duration <= 0f)
            {
                source = next;
                return;
            }

            var mixer = AnimationMixerPlayable.Create(graph, 2);
            mixer.ConnectInput(PREV, source, 0, 1);
            mixer.ConnectInput(NEXT, next, 0, 0);
            source = mixer;

            Phase.Start(duration, PlayerLoopTiming.Update, token, t =>
            {
                mixer.SetInputWeight(PREV, 1 - t);
                mixer.SetInputWeight(NEXT, t);
            });
        }
    }

}