using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SketchEngine.Audio
{
    public class AudioButtonSfx : MonoBehaviour, IPointerDownHandler
    {
        [Header("References")] 
        [SerializeField, HideInInspector] Button _button;
        
        [Title("Config")]
        [SerializeField] AudioConfig _sfxOnClick;

        Button Button
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponentInChildren<Button>();
                }
                return _button;
            }
        }

        void Awake()
        {
            EnsureComponents();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (Button != null && !Button.interactable) return;
            
            AudioManager.Play(_sfxOnClick != null ? _sfxOnClick : AudioMixerFactory.SfxUIButtonClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureComponents()
        {
            if (_button == null) _button = Button;
        }

        protected virtual void OnValidate()
        {
            EnsureComponents();
        }
    }
}
