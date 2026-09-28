using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Oscillator component.
    ///
    /// Purpose:
    /// - Apply a continuous sinusoidal movement to a GameObject
    /// - Useful for floating platforms, pickups, decorations, or simple motion effects
    ///
    /// Behavior:
    /// - The object oscillates around its start position
    /// - Movement follows a sine wave: sin(2πt / period)
    /// - Direction, amplitude, and period are configurable
    ///
    /// Notes:
    /// - Uses MonoBase.Tick() (frame-based update, NOT physics-based)
    /// - Directly sets transform position (not physics-safe)
    /// - Best suited for visual / non-physics objects
    /// </summary>
    public class Oscillator : MonoBase
    {
        /// <summary>
        /// Maximum distance from the start position.
        /// </summary>
        [SerializeField]
        float _amplitude = 1.0f;

        /// <summary>
        /// Time (in seconds) to complete one full oscillation cycle.
        /// </summary>
        [SerializeField]
        float _period = 1.0f;

        /// <summary>
        /// Direction of oscillation.
        /// Example: Vector3.up, Vector3.right, or any normalized vector.
        /// </summary>
        [SerializeField]
        Vector3 _direction = Vector3.up;

        /// <summary>
        /// Cached starting position used as the oscillation center.
        /// </summary>
        Vector3 _startPosition;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        /// <summary>
        /// Called once when the object is initialized.
        /// Caches the starting position for oscillation.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _startPosition = TransformCached.position;
        }

        /// <summary>
        /// Frame update (called via MonoBase).
        /// Applies sinusoidal offset to the cached start position.
        /// </summary>
        protected override void Tick()
        {
            base.Tick();

            // Guard against invalid period
            if (_period <= 0.0001f)
                return;

            float phase =
                Mathf.Sin(
                    2.0f * Mathf.PI * Time.time / _period
                );

            Vector3 offset =
                _direction * _amplitude * phase;

            TransformCached.position =
                _startPosition + offset;
        }
    }
}
