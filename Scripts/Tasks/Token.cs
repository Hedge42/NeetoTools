using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct Token
{
    public static implicit operator Token(CancellationToken t) => Create(t);
    public static implicit operator CancellationToken(Token Token) => Token.token;
    public static implicit operator bool(Token Token) => !Token.token.IsCancellationRequested;

    public static Token operator ++(Token Token) { Token.Cancel(); return new(); }
    public static CancellationToken operator &(Token token, CancellationToken link) => new Token(token, link);
    public static CancellationToken operator &(Token token, Component component) => new Token(token, component.GetCancellationTokenOnDestroy());

    CancellationTokenSource source;
    CancellationToken token;

    public void Cancel()
    {
        source?.Cancel();
        source?.Dispose();
    }
    public void Register(Action a) => token.Register(a);

    #region EXTENSIONS
    public Token(params CancellationToken[] tokens)
    {
        source = CancellationTokenSource.CreateLinkedTokenSource(tokens);
        token = source.Token;
    }
    public static Token Create(params CancellationToken[] tokens) => new(tokens);
    public static Token Create() => new();

    /// <summary>Cancels when game stops playing</summary>
    public static CancellationToken global => _global;
    /// <summary>Cancels when active scene is changed</summary>
    public static CancellationToken scene => _scene;
    static Token _global, _scene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Start()
    {
        _global++;
        Engine.onQuit += _global.Cancel;
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
    public static bool Canceled(this CancellationToken token) => token.IsCancellationRequested;
    public static void Fire(this UniTask task, CancellationToken token) => task.AttachExternalCancellation(token).Forget();
    public static UniTask With(this UniTask task, CancellationToken token) => task.AttachExternalCancellation(token);
    public static UniTask.Awaiter GetAwaiter(this CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken secondary = default)
    {
        return UniTask.Create(async () =>
        {
            await UniTask.WaitUntil(() => token.IsCancellationRequested, timing, secondary, true);

        }).GetAwaiter();
    }

}
