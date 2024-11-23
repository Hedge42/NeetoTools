using System.Collections.Generic;

namespace Neeto
{

    public class PriorityStateMachine<T> where T : IState, IPriority
    {
        public T State { get; private set; }
        private readonly SortedList<int, T> states = new();

        public void Push(T newState)
        {
            states[newState.Priority] = newState;
            if (State == null || newState.Priority >= State.Priority)
                TransitionTo(newState);
        }

        public void Pop(T oldState)
        {
            if (states.Remove(oldState.Priority) && State.Priority == oldState.Priority)
                TransitionTo(states.Count > 0 ? states.Values[^1] : default);
        }

        private void TransitionTo(T newState)
        {
            State?.Exit();
            State = newState;
            State?.Enter();
        }
    }
    public interface IState
    {
        bool IsPersistent { get; }
        void Enter();
        void Exit();
    }
    public interface IPriority
    {
        int Priority { get; }
    }
}