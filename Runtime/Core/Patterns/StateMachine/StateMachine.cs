using System;
using System.Collections.Generic;

namespace SketchEngine.Patterns.StateSystem
{
    /// <summary>
    /// A lightweight state machine.
    /// </summary>
    /// <remarks>
    /// 	<para>To use it: </para>
    /// 	<list type="bullet">
    /// 		<item>
    /// 			<description>Define your own label. Enums are probably the best
    /// choice.</description>
    /// 		</item>
    /// 		<item>
    /// 			<description>Construct a new state machine, typically in a
    /// MonoBehaviour's Start method.</description>
    /// 		</item>
    /// 		<item>
    /// 			<description>Add the various states with the appropriate delegates.
    /// </description>
    /// 		</item>
    /// 		<item>
    /// 			<description>Call the state machine's Update method from the
    /// MonoBehaviour's Update method.</description>
    /// 		</item>
    /// 		<item>
    /// 			<description>Set the CurrentState property on the state machine to transition.
    /// (You can either set it from one of the state delegates, or from anywhere else.
    /// </description>
    /// 		</item>
    /// 	</list>
    /// 	<para>When a state is changed, the OnStop on existing state is called, then the
    /// OnStart of the new state, and from there on OnUpdate of the new state each time
    /// the update is called.</para>
    /// </remarks>
    /// <typeparam name="TLabel">The label type of this state machine. Enums are common,
    /// but strings or int are other possibilities.</typeparam>
    public class StateMachine<TLabel> : IDisposable
    {
        #region Types

        class State
        {
            #region Public Fields

            public readonly TLabel lable;
            public readonly Action onStart;
            public readonly Action onStop;
            public readonly Action onUpdate;
            public readonly Action onFixedUpdate;
            public readonly Action onDestroy;

            #endregion

            #region Constructors

            public State(TLabel lable, Action onStart, Action onUpdate, Action onFixedUpdate, Action onStop, Action onDestroy)
            {
                this.onStart = onStart;
                this.onUpdate = onUpdate;
                this.onFixedUpdate = onFixedUpdate;
                this.onStop = onStop;
                this.onDestroy = onDestroy;
                this.lable = lable;
            }

            #endregion
        }
     
        #endregion

        #region Private Fields

        readonly Dictionary<TLabel, State> stateDictionary;
        State currentState;
        State previousState;
        
        bool disposed;

        #endregion

        #region Properties

        public TLabel PreviousState => previousState == null ? currentState.lable : previousState.lable;

        public TLabel CurrentState
        {
            get => currentState.lable;
            set => ChangeState(value);
        }

        #endregion

        #region Constructors

        public StateMachine()
        {
            stateDictionary = new Dictionary<TLabel, State>();
        }

        #endregion

        #region Destructors

        ~StateMachine()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (disposed) return;

            foreach (var state in stateDictionary)
            {
                state.Value.onDestroy?.Invoke();
            }

            stateDictionary.Clear();
            disposed = true;
        }


        #endregion

        #region Unity Callbacks

        /// <summary>
        /// This method should be called every frame
        /// </summary>
        public void Update()
        {
            if (currentState != null && currentState.onUpdate != null) currentState.onUpdate();
        }
        
        /// <summary>
        /// This method should be called every frame
        /// </summary>
        public void FixedUpdate()
        {
            if (currentState != null && currentState.onFixedUpdate != null) currentState.onFixedUpdate();
        }

        #endregion

        #region Public Methods

        public void AddState(TLabel lable, IStateMachine stateMachine)
        {
            stateMachine.Init();

            stateDictionary[lable] = new State(lable, stateMachine.OnStart, stateMachine.OnUpdate, stateMachine.OnFixedUpdate, stateMachine.OnStop, stateMachine.OnDestroy);
        }

        /// <summary>
        /// Adds a state, and the delegates that should run
        /// when the state starts, stops,
        /// and when the state machine is updated
        /// 
        /// Any delegate can be null, and wont be executed
        /// </summary>
        /// <param name="lable">The name of the state to add.</param>
        /// <param name="onStart">The action performed when the state is entered.</param>
        /// <param name="onUpdate">The action performed when the state machine is updated in the given state.</param>
        /// <param name="onStop">The action performed when the state machine is left.</param>
        /// <param name="onDestroy"></param>
        public void AddState(TLabel lable, Action onStart, Action onUpdate, Action onFixedUpdate, Action onStop, Action onDestroy)
        {
            stateDictionary[lable] = new State(lable, onStart, onUpdate, onFixedUpdate, onStop, onDestroy);
        }
        
        /// <summary>
        /// Adds a state, and the delegates that should run 
        /// when the state starts, 
        /// and when the state machine is updated.
        /// 
        /// Any delegate can be null, and wont be executed.
        /// </summary>
        /// <param name="label">The name of the state to add.</param>
        /// <param name="onStart">The action performed when the state is entered.</param>
        /// <param name="onUpdate">The action performed when the state machine is updated in the given state.</param>
        public void AddState(TLabel label, Action onStart, Action onUpdate, Action onFixedUpdate)
        {
            AddState(label, onStart, onUpdate, onFixedUpdate, null, null);
        }
        
        /// <summary>
        /// Adds a state, and the delegates that should run 
        /// when the state starts.
        /// 
        /// Any delegate can be null, and wont be executed.
        /// </summary>
        /// <param name="label">The name of the state to add.</param>
        /// <param name="onStart">The action performed when the state is entered.</param>
        public void AddState(TLabel label, Action onStart)
        {
            AddState(label, onStart, null,null);
        }
        
        /// <summary>
        /// Adds a state.
        /// </summary>
        /// <param name="label">The name of the state to add.</param>
        public void AddState(TLabel label)
        {
            AddState(label, null, null, null);
        }
        
        /// <summary>
        /// Adds a sub state machine for the given state.
        ///
        /// The sub state machine need not be updated, as long as this state machine
        /// is being updated.
        /// </summary>
        /// <typeparam name="TSubStateLabel">The type of the sub-machine.</typeparam>
        /// <param name="label">The name of the state to add.</param>
        /// <param name="subMachine">The sub-machine that will run during the given state.</param>
        /// <param name="subMachineStartState">The starting state of the sub-machine.</param>
        public void AddState<TSubStateLabel>(TLabel label, StateMachine<TSubStateLabel> subMachine, TSubStateLabel subMachineStartState)
        {
            AddState(
                label,
                () => subMachine.ChangeState(subMachineStartState),
                subMachine.Update, subMachine.FixedUpdate);
        }
        
        /// <summary>
        /// Returns the current state name
        /// </summary>
        public override string ToString()
        {
            return CurrentState.ToString();
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Changes the state from the existing one to the state with the given label.
        /// 
        /// It is legal (and useful) to transition to the same state, in which case the 
        /// current state's onStop action is called, the onStart action is called, and the
        /// state keeps on updating as before. The behaviour is exactly the same as switching to
        /// a new state.
        /// </summary>
        void ChangeState(TLabel newState)
        {
            //previousState = currentState;
            TLabel prevState = currentState != null ? currentState.lable : default; // Lưu trạng thái trước đó

            if (currentState != null && currentState.onStop != null)
            {
                currentState.onStop();
            }

            currentState = stateDictionary[newState];

            currentState.onStart?.Invoke();

            OnStateChanged?.Invoke(prevState, newState);
        }

        #endregion
        
        #region Callback Events

        public event Action<TLabel, TLabel> OnStateChanged;

        #endregion
    }
}
