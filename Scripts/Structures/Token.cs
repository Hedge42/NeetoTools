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


    public static CancellationToken global => _global;
    public static CancellationToken scene => _scene;
    static Token _global, _scene;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Start()
    {
        _global++;
        NApp.onQuit += () => _global--;
        SceneManager.activeSceneChanged += (_, _) => _scene++;
    }

}


namespace Exp
{




    public struct Routine
    {
        Token token;
        Func<CancellationToken, UniTask> func;

        public Routine(Func<CancellationToken, UniTask> func)
        {
            this.func = func;
            token = default;
        }
        public void Pause()
        {
            token.Disable();
        }
        public void Resume()
        {
            func(token.Enable())
                .AttachExternalCancellation(Token.global)
                .Forget();
        }
    }
}
