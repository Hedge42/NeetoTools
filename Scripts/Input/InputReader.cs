using Neeto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Neeto
{
    public class InputReader : MonoBehaviour
    {
        [SerializeReference, Polymorphic]
        public List<IInputEvent> inputs;

        private void OnEnable()
        {
            foreach (var input in inputs)
            {
                input?.Enable();
            }
        }
        private void OnDisable()
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

        [Serializable]
        public class StartedEvent : InputEvent
        {
            public UnityEvent<InputAction.CallbackContext> callback;

            public override void Enable()
            {
                base.Enable();
                action.started += callback.Invoke;
            }
        }

        [Serializable]
        public class VectorEvent : InputEvent
        {
            public UnityEvent<Vector2> on;

            public override void Enable()
            {
                base.Enable();
            }
        }

        [Serializable]
        public class ButtonEvent : InputEvent
        {
            public UnityEvent callback;
            public override void Enable()
            {
                base.Enable();
                action.started += _ => callback?.Invoke();
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

        [Serializable]
        public class VectorAction : InputEvent
        {
            public GameAction<Vector2> callback;

            public override void Enable()
            {
                base.Enable();
                /// ??
            }
        }

        [Serializable]
        public class ButtonAction : InputEvent
        {
            public GameAction callback;
            public override void Enable()
            {
                base.Enable();
                action.started += _ => callback?.Invoke();
            }
        }
    }
}