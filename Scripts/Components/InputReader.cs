using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Neeto
{
    public class InputReader : MonoBehaviour
    {
        [SerializeReference, Polymorphic]
        public List<IInputEvent> inputs;

        void OnEnable()
        {
            foreach (var input in inputs)
            {
                input?.Enable();
            }
        }
        void OnDisable()
        {
            foreach (var input in inputs)
            {
                input?.Disable();
            }
        }
    }

    public interface IInputEvent
    {
        void Enable();
        void Disable();
    }
    [Serializable]
    public abstract class InputEvent : IInputEvent
    {
        public InputActionReference input;
        public InputAction action { get; protected set; }
        public virtual void Enable()
        {
            action = input.action.Clone();
            action.Enable();
        }
        public virtual void Disable()
        {
            action?.Disable();
            action?.Dispose();
        }
        
    }
    [Serializable]
    public abstract class AsyncInput : IInputEvent
    {
        protected CancellationTokenSource cts;
        public virtual void Disable()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
        public virtual void Enable()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new();
            EnableAsync(cts.Token).Forget();
        }
        protected abstract UniTaskVoid EnableAsync(CancellationToken token);
    }

    [Serializable]
    public class KeyDownEvent : AsyncInput
    {
        public KeyCode key = KeyCode.Alpha1;
        public UnityEvent onKeyDown;
        protected override async UniTaskVoid EnableAsync(CancellationToken token)
        {
            while (true)
            {
                await UniTask.Yield(token);
                if (Input.GetKeyDown(key))
                    onKeyDown?.Invoke();
            }
        }
    }
    [Serializable]
    public class KeyEvent : AsyncInput
    {
        public KeyCode key = KeyCode.Alpha1;
        [Note("Default: send TRUE when key down, FALSE when key up\nInvert: reverse these values")]
        public bool invert;
        public UnityEvent<bool> changed;
        bool status;
        protected override async UniTaskVoid EnableAsync(CancellationToken token)
        {
            while (true)
            {
                await UniTask.Yield(token);
                var status = Input.GetKey(key);
                if (this.status != status)
                {
                    this.status = status;
                    changed?.Invoke(status);
                }
            }
        }
    }
    [Serializable]
    public class StartedEvent : InputEvent
    {
        public UnityEvent<InputAction.CallbackContext> started;
        public override void Enable()
        {
            base.Enable();
            action.started += started.Invoke;
        }
    }
    [Serializable]
    public class StartedAction : InputEvent
    {
        public GameAction<InputAction.CallbackContext> callback;
        public override void Enable()
        {
            base.Enable();
            action.started += callback.Invoke;
        }
    }
}