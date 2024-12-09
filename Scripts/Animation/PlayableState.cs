using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableInstance
{
    public enum State
    {
        Playing,
        Finished,
        Cancelled,
    }

    public readonly PlayableController controller;
    public readonly IPlayableSource source;
    public readonly Playable playable;
    private readonly Token token;

    public event Action stopped;
    public State state { get; set; }

    public PlayableInstance(PlayableController controller, IPlayableSource source, CancellationToken token)
    {
        this.controller = controller;
        this.source = source;
        this.playable = source.GetPlayable(controller.graph);
        this.token = Token.Create(token);
        this.token.Register(Cancel);
    }
    public void Cancel()
    {
        if (!token)
            return;

        playable.Pause();
        token.Cancel();
        stopped?.Invoke();
    }
    public void Play()
    {
        if (!token)
            return;

        PlayAsync(token).Forget();
    }
    async UniTaskVoid PlayAsync(CancellationToken token)
    {
        controller.blender.Blend(playable, controller.enabledToken);
        await source.PlayAsync(controller.animator, playable, token);
        controller.Play(controller.BaseState);
        Cancel();
    }
}

public interface IPlayableSource
{
    Playable GetPlayable(PlayableGraph graph);
    UniTask PlayAsync(Animator animator, Playable playable, CancellationToken token);
}

public interface IPlayableListener
{
    UniTaskVoid ListenAsync(Animator animator, Playable playable, CancellationToken token);
}
public interface IPlayableLoop : IPlayableSource { }
public interface IPlayableAction : IPlayableSource { }

[Serializable]
public class AnimatorControllerPlayableLoop : IPlayableLoop
{
    [field: SerializeField] public string name { get; set; } = "idle";

    [Min(0f)] public float blendDuration = .2f;
    public RuntimeAnimatorController asset;

    [SerializeReference, Polymorphic, ReorderableList(ListStyle.Boxed)]
    public ISourcedAnimatorValue[] values;

    public Playable GetPlayable(PlayableGraph graph) => AnimatorControllerPlayable.Create(graph, asset);
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
public class AnimationClipPlayableAction : IPlayableAction
{
    [field: SerializeField] public string name { get; set; } = "attack";

    [PreviewAnimation(nameof(clip))]
    public float preview;

    public AnimationClip clip;
    [Min(0f)] public float blendDuration = 0.1f;
    [Min(0f)] public float speed = 1f;
    [MinMax(0f, 1f)] public Vector2 playWindow = Vector2.up;
    [SerializeReference, Polymorphic] public IPlayableListener[] events;

    public Playable GetPlayable(PlayableGraph graph)
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