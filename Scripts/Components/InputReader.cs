using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Neeto
{
    public class InputReader : MonoBehaviour
    {
        [SerializeReference, Polymorphic, ReorderableList]
        public List<IInputEvent> inputs = new() { new KeyDownEvent() };

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
        protected Token token;
        public virtual void Disable()
        {
            token.Disable();
        }
        public virtual void Enable()
        {
            EnableAsync(++token).Forget();
        }
        protected abstract UniTaskVoid EnableAsync(CancellationToken token);
    }
    [Serializable]
    public abstract class AsyncInputAction : AsyncInput
    {
        [SerializeReference] InputActionReference inputReference;
        public InputAction input { get; private set; }

        public override void Enable()
        {
            input?.Disable();
            input = inputReference.action.Clone();
            input.Enable();

            base.Enable();
        }
        public override void Disable()
        {
            base.Disable();
            input?.Disable();
        }
    }

    [Serializable]
    public class KeyDownEvent : AsyncInput
    {
        [SearchableEnum]
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
        [SearchableEnum]
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
        //public GameAction<InputAction.CallbackContext> callback;
        public override void Enable()
        {
            base.Enable();
            //action.started += callback.Invoke;
        }
    }

    [Serializable]
    public class Vector2InputEvent : AsyncInputAction
    {
        public PlayerLoopTiming loop = PlayerLoopTiming.Update;

        public UnityEvent<Vector2> output;
        protected override async UniTaskVoid EnableAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(loop, token);
                output?.Invoke(input.ReadValue<Vector2>());
            }
        }
    }

    [Serializable]
    public class GroupedInput : IInputEvent
    {
        public string[] groups;

        [SerializeReference, Polymorphic, ReorderableList]
        public IInputEvent input;

        public bool enabled { get; protected set; }

        public void Enable()
        {
            input.Enable();
            enabled = true;
        }
        public void Disable()
        {
            input.Disable();
            enabled = false;
        }
        public void SetActive(bool value)
        {
            if (value && !enabled)
                Enable();
            else if (!value && enabled)
                Disable();
        }

        public void UpdateDisabled(string[] disabledGroups)
        {
            bool enabled = !groups.Intersect(disabledGroups).Any();
            SetActive(enabled);
        }
    }
}