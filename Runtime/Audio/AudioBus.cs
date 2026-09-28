using System;

namespace SketchEngine.Audio
{
    /// <summary>
    /// Logical audio routing buses.
    /// 
    /// Design rules:
    /// - Only Voice bus is allowed to duck Music
    /// - UI / Sound sounds must never duck Music
    /// - Ambience is always non-intrusive
    /// </summary>
    [Serializable]
    public enum AudioBus
    {
        None = 0,

        /// <summary>
        /// Master output (root mixer)
        /// </summary>
        Master = 10,

        /// <summary>
        /// Background music (duckable by Voice only)
        /// </summary>
        Music = 20,

        /// <summary>
        /// UI sound effects (clicks, notifications)
        /// Never ducks music
        /// </summary>
        UI = 30,

        /// <summary>
        /// Gameplay sound effects (impacts, skills, footsteps)
        /// Never ducks music
        /// </summary>
        Sound = 40,

        /// <summary>
        /// Spoken voice / narration
        /// This bus can duck Music
        /// </summary>
        Voice = 50,

        /// <summary>
        /// Environmental looping sounds (wind, rain, crowd)
        /// Never ducks music
        /// </summary>
        Ambience = 60
    }
}