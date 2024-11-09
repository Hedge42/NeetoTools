using Neeto;
using System;
using System.Threading;
using UnityEngine;

public struct Token
{
    public static implicit operator CancellationToken(Token token) => token.token;
    public static Token operator ++(Token t) => t.Update();

    public CancellationTokenSource Source { get; private set; }
    public CancellationToken token { get; private set; }

    public Token Enable()
    {
        Source = new();
        return this;
    }
    public void Disable()
    {
        Source.Kill();
    }
    public Token Update(Action onCancel = null)
    {
        Source = Source.Refresh();
        if (onCancel != null)
            token.Register(onCancel);
        return this;
    }


    public static CancellationToken Global { get; private set; }

    [RuntimeInitializeOnLoadMethod] static void Start()
    {
        var cts = new CancellationTokenSource();
        Global = cts.Token;
        AppHelper.onQuit += cts.Kill;
    }

}
