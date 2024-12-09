using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.PlayerLoop;


public class PlayableController : MonoBehaviour
{
    [field: SerializeField, GetComponent]
    public Animator animator { get; set; }

    [field: SerializeField, SerializeReference, Polymorphic]
    public IPlayableSource BaseState { get; set; }

    public PlayableGraph graph { get; protected set; }
    public PlayableOutput output { get; protected set; }
    public PlayableBlender blender { get; protected set; }

    protected Token playToken { get; set; }
    public Token enabledToken { get; protected set; }

    protected virtual void OnEnable()
    {
        graph = PlayableGraph.Create(animator.name);
        output = AnimationPlayableOutput.Create(graph, animator.name, animator);
        ++playToken;
        blender = new(graph);

        graph.Play();
    }
    protected virtual void OnDisable()
    {
        playToken.Cancel();
        graph.Stop();
        graph.Destroy();
    }
    public virtual void Play(IPlayableSource source)
    {
        new PlayableInstance(this, source, ++playToken).Play();
    }
}
