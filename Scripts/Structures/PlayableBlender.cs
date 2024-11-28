using Cysharp.Threading.Tasks;
using Neeto;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Neeto
{
    public class PlayableBlender
    {
        public static implicit operator Playable(PlayableBlender blender) => blender.container;

        public PlayableGraph graph { get; private set; }
        //public PlayableOutput output { get; private set; }
        const int PREV = 0;
        const int NEXT = 1;

        Playable container;
        public Playable source
        {
            get => container.GetInput(0);
            private set
            {
                container.DisconnectInput(0);
                container.ConnectInput(0, value, 0);
            }
        }
        
        public PlayableBlender(PlayableGraph graph)
        {
            this.graph = graph;
            container = Playable.Create(graph, 1);
        }
        public void Blend(Playable next, CancellationToken token, float duration = .15f)
        {
            if (!source.IsValid() || duration <= 0f)
            {
                source = next;
                return;
            }

            var mixer = AnimationMixerPlayable.Create(graph, 2);
            mixer.ConnectInput(PREV, container, 0, 1);
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