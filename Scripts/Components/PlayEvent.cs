using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class PlayEvent : MonoBehaviour, IPlay
    {
        public UnityEvent onPlay;

        public void Play()
        {
            onPlay?.Invoke();
        }
    }
}
