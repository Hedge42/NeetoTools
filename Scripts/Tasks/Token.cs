using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct Token
{
    public static implicit operator CancellationToken(Token Token) => Token.token;
    public static implicit operator bool(Token Token) => Token.enabled;
    public static Token operator ++(Token Token) => Token.Enable();
    public static Token operator --(Token Token) => Token.Disable();

    public bool enabled { get; private set; }
    CancellationTokenSource source;
    CancellationToken token;
    CancellationToken? linkedToken;

    public Token (CancellationToken? linkedToken = null)
    {
        this.linkedToken = linkedToken;
    }
    


    public Token Enable()
    {
        source?.Cancel();
        source?.Dispose();
        source = LinkedSource(linkedToken);
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
    public void SetLink(CancellationToken? linked)
    {
        linkedToken = linked;
    }
    public void Unlink()
    {
        linkedToken = null;
    }

    #region EXTENSIONS
    public static Token Create(CancellationToken? linkedToken = null)
    {
        return new Token(linkedToken);
    }
    public static CancellationTokenSource LinkedSource(CancellationToken? _token)
    {
        return _token is CancellationToken token
            ? CancellationTokenSource.CreateLinkedTokenSource(token)
            : new CancellationTokenSource();
    }

    /// <summary>Cancels when game stops playing</summary>
    public static CancellationToken global => _global;
    /// <summary>Cancels when active scene is changed</summary>
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
