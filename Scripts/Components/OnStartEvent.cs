using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class OnStart : MonoBehaviour
    {
        public UnityEvent onStart;

        [QuickAction]
        private void Start()
        {
            onStart.Invoke();
        }
    }
}