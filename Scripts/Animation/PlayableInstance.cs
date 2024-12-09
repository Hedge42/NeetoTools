using Cysharp.Threading.Tasks;
using System;
using System.Threading;
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
        this.playable = source.CreatePlayable(controller.graph);
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
