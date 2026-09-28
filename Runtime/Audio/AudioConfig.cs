using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Audio
{
    [System.Serializable]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] AudioClip _clip;
        [SerializeField] AudioType _type;
        
        [ShowIf("@_type == AudioType.Sound")]
        [SerializeField] AudioMode _mode = AudioMode.Mode2D;

        [ShowIf("@_mode == AudioMode.Mode3D")]
        [MinMaxSlider(0f, 200f, ShowFields = true)]
        [SerializeField] Vector2 _earsDistance = new Vector2(1f, 10f);

        [Range(0f, 1f)]
        [SerializeField] float _volume = 1f;

        [SerializeField] AudioBus _bus = AudioBus.Master;

        public AudioClip Clip => _clip;
        public AudioType Type => _type;
        public AudioMode Mode => _mode;
        public Vector2 EarsDistance => _earsDistance;
        public float Volume => _volume;

        public AudioBus Bus => _bus;
    }
}