using System;
using Stateless;

namespace Development.StateMachine
{
    public abstract class State<TState, Trigger, TShared>
        where TState : Enum
        where Trigger : Enum
        where TShared : class
    {
        protected readonly StateMachine<TState, Trigger> StateMachine;
        protected readonly TShared Context;

        protected State(StateMachine<TState, Trigger> stateMachine, TShared context)
        {
            StateMachine = stateMachine;
            Context = context;
        }

    }
}
