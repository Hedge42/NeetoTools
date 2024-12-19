using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public interface IPlayableSource
{
    Playable CreatePlayable(PlayableGraph graph);
    UniTask PlayAsync(Animator animator, Playable playable, CancellationToken token);
}

[Serializable]
public class AnimatorControllerPlayableLoop : IPlayableSource
{
    [field: SerializeField] public string name { get; set; } = "idle";

    [Min(0f)] public float blendDuration = .2f;
    public RuntimeAnimatorController asset;

    [SerializeReference, Polymorphic, ReorderableList(ListStyle.Boxed)]
    public ISourcedAnimatorValue[] values;

    public Playable CreatePlayable(PlayableGraph graph) => AnimatorControllerPlayable.Create(graph, asset);
    public async UniTask PlayAsync(Animator animator, Playable playable, CancellationToken token)
    {
        // animator = gameObject.CacheComponent<Animator>();
        playable.Play();

        while (!token.IsCancellationRequested && playable.IsValid())
        {
            foreach (var value in values)
                value.SetValue(animator);
            await UniTask.Yield(token);
        }
    }

}
[Serializable]
public class AnimationClipPlayableAction : IPlayableSource
{
    [field: SerializeField] public string name { get; set; } = "attack";
    [PreviewAnimation] public AnimationClip clip;
    [Min(0f)] public float blendDuration = 0.1f;
    [Min(0f)] public float speed = 1f;
    [MinMax(0f, 1f)] public Vector2 playWindow = Vector2.up;
    [SerializeReference, Polymorphic] public IPlayableListener[] events;

    public Playable CreatePlayable(PlayableGraph graph)
    {
        clip.wrapMode = WrapMode.ClampForever;
        var playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetSpeed(speed);
        playable.SetTime(playWindow.x * clip.length);
        playable.SetDuration(clip.length);
        playable.SetApplyPlayableIK(false);
        playable.SetApplyFootIK(false); // !!! this solves feet teleporting !!! //
        return playable;
    }

    public async UniTask PlayAsync(Animator animator, Playable playable, CancellationToken token)
    {
        playable.Play();

        Token Token = Token.Create(token);

        try // run update loop
        {
            events.Foreach(_ => _.ListenAsync(animator, playable, Token));

            await playable.WaitForNormalizedTime(playWindow.y, Token);
        }
        finally // always stop listening
        {
            Token.Cancel();
        }
    }
}