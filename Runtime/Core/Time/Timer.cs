using System;

namespace SketchEngine.Core.Time
{
    /// <summary>
    /// Base class for frame-based runtime timers.
    ///
    /// PURPOSE
    /// - Measure time using deltaTime while the game is running.
    /// - NOT persistent (no save/load, no offline).
    /// - Designed for gameplay logic, animation timing, combo windows, etc.
    ///
    /// DESIGN NOTES
    /// - Zero allocation during runtime (no GC).
    /// - No virtual calls except Tick (intended).
    /// - Explicit lifecycle: Start / Pause / Resume / Stop.
    ///
    /// DO NOT USE FOR
    /// - Cooldown across sessions
    /// - Offline progress
    /// - IAP / reward timers
    /// Use UtilsTime (Unix time) instead.
    /// </summary>
    public abstract class Timer
    {
        // =====================================================
        // FIELDS
        // =====================================================

        /// <summary>
        /// Initial time value when Start() is called.
        /// For Countdown: total duration.
        /// For Stopwatch: always 0.
        /// </summary>
        protected float _initialTime;

        /// <summary>
        /// Current time value.
        /// - Countdown: remaining time (seconds)
        /// - Stopwatch: elapsed time (seconds)
        /// </summary>
        public float Time { get; protected set; }

        /// <summary>
        /// Whether the timer is actively ticking.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Normalized progress [0..1].
        /// - Countdown: 1 → 0
        /// - Stopwatch: 0 → 1 (clamped)
        ///
        /// Safe against division by zero.
        /// </summary>
        public float Progress
        {
            get
            {
                if (_initialTime <= 0f) return 1f;
                return Time / _initialTime;
            }
        }

        // =====================================================
        // EVENTS (NO ALLOC AFTER INIT)
        // =====================================================

        /// <summary>
        /// Called once when Start() transitions to running state.
        /// </summary>
        public Action OnStarted;

        /// <summary>
        /// Called once when Stop() transitions to stopped state.
        /// </summary>
        public Action OnStopped;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        protected Timer(float initialTime)
        {
            _initialTime = initialTime;
            Time = initialTime;
            IsRunning = false;
        }

        // =====================================================
        // LIFECYCLE
        // =====================================================

        /// <summary>
        /// Reset time to initial value and start running.
        /// </summary>
        public void Start()
        {
            Time = _initialTime;

            if (IsRunning)
                return;

            IsRunning = true;
            OnStarted?.Invoke();
        }

        /// <summary>
        /// Stop timer immediately.
        /// Does NOT reset time.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            OnStopped?.Invoke();
        }

        /// <summary>
        /// Pause ticking without resetting time.
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Resume ticking without resetting time.
        /// </summary>
        public void Resume()
        {
            IsRunning = true;
        }

        // =====================================================
        // UPDATE
        // =====================================================

        /// <summary>
        /// Advance timer by deltaTime.
        /// Must be called manually (usually from Update).
        /// </summary>
        public abstract void Tick(float deltaTime);
    }

    // =========================================================
    // COUNTDOWN TIMER
    // =========================================================

    /// <summary>
    /// Countdown timer (time decreases to zero).
    ///
    /// USE CASES
    /// - Skill cooldown (runtime only)
    /// - Delay logic
    /// - Combo timeout
    ///
    /// BEHAVIOR
    /// - Time starts at duration.
    /// - Decreases every Tick.
    /// - Automatically stops when reaching zero.
    /// </summary>
    public sealed class CountdownTimer : Timer
    {
        public CountdownTimer(float durationSeconds)
            : base(durationSeconds)
        {
        }

        /// <summary>
        /// Whether countdown has finished (Time <= 0).
        /// </summary>
        public bool IsFinished => Time <= 0f;

        public override void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            Time -= deltaTime;

            if (Time > 0f)
                return;

            Time = 0f;
            Stop();
        }

        /// <summary>
        /// Reset countdown to initial duration without starting.
        /// </summary>
        public void Reset()
        {
            Time = _initialTime;
        }

        /// <summary>
        /// Reset countdown with a new duration.
        /// </summary>
        public void Reset(float newDuration)
        {
            _initialTime = newDuration;
            Time = newDuration;
        }
    }

    // =========================================================
    // STOPWATCH TIMER
    // =========================================================

    /// <summary>
    /// Stopwatch timer (time increases from zero).
    ///
    /// USE CASES
    /// - Measure how long something lasts
    /// - Charge mechanics (hold button)
    /// - Combo timing
    /// - State duration tracking
    ///
    /// BEHAVIOR
    /// - Time starts at 0.
    /// - Increases every Tick.
    /// - Never auto-stops.
    /// </summary>
    public sealed class StopwatchTimer : Timer
    {
        public StopwatchTimer()
            : base(0f)
        {
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            Time += deltaTime;
        }

        /// <summary>
        /// Reset elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            Time = 0f;
        }

        /// <summary>
        /// Get elapsed time in seconds.
        /// </summary>
        public float Elapsed => Time;
    }
}
