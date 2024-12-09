using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

public interface IPlayableListener
{
    UniTaskVoid ListenAsync(Animator animator, Playable playable, CancellationToken token);
}
