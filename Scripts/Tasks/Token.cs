using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct Token
{
    public static implicit operator CancellationToken(Token token) => token.token;
    public static implicit operator bool(Token token) => token.enabled;
    public static Token operator ++(Token t) => t.Refresh();
    public static Token operator --(Token t) => t.Disable();

    public bool enabled { get; private set; }
    public CancellationToken token { get; private set; }
    CancellationTokenSource source;

    public static Token Create() => new Token().Enable();
    public Token Enable()
    {
        source?.Cancel();
        source = new();
        token = source.Token;
        enabled = true;
        return this;
    }
    public Token Disable()
    {
        source?.Cancel();
        source?.Dispose();
        source = null;
        enabled = false;
        return this;
    }
    public void Register(Action onCancel)
    {
        token.Register(onCancel);
    }
    public Token Refresh(Action onCancel = null)
    {
        source?.Cancel();
        source?.Dispose();
        source = new();
        token = source.Token;
        if (onCancel != null)
            token.Register(onCancel);
        return this;
    }

    #region EXTENSIONS
    public static CancellationToken global => _global;
    public static CancellationToken scene => _scene;
    static Token _global, _scene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Start()
    {
        _global++;
        Engine.onQuit += () => _global--;
        SceneManager.activeSceneChanged += (_, _) => _scene++;
    }
    #endregion
}


public static class TokenExtensions
{
    public static CancellationTokenSource Refresh(this CancellationTokenSource source)
    {
        Kill(source);

        source = new CancellationTokenSource();

        return source;
    }
    public static CancellationTokenSource Refresh(this CancellationTokenSource source, out CancellationToken token)
    {
        Kill(source);

        source = new CancellationTokenSource();
        token = source.Token;

        return source;
    }
    public static CancellationTokenSource Refresh(this CancellationTokenSource source, Component component)
    {
        source.Refresh();
        source.RegisterRaiseCancelOnDestroy(component);
        return source;
    }
    public static CancellationTokenSource Refresh(this CancellationTokenSource source, GameObject gameOobject)
    {
        source.Refresh();
        source.RegisterRaiseCancelOnDestroy(gameOobject);
        return source;
    }
    public static void Kill(this CancellationTokenSource source)
    {
        try
        {
            source?.Cancel();
            source?.Dispose();
        }
        catch { }
    }
    public static void Restart(this CancellationTokenSource source, params UniTask[] tasks)
    {
        source.Refresh(out var token);

        foreach (var t in tasks)
            t.AttachExternalCancellation(token).Forget();
    }
    public static UniTask.Awaiter GetAwaiter(this CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken secondary = default)
    {
        return UniTask.Create(async () =>
        {
            await UniTask.WaitUntil(() => token.IsCancellationRequested, timing, secondary, true);

        }).GetAwaiter();
    }

}
