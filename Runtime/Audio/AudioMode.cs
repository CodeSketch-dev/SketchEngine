using System;
using Sirenix.OdinInspector;

namespace SketchEngine.Audio
{
    [Serializable]
    public enum AudioMode
    {
        [LabelText("2D")]
        Mode2D,
        [LabelText("3D")]
        Mode3D
    }
}
