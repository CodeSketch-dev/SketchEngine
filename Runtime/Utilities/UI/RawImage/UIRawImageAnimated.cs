using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using SketchEngine.Mono;

namespace SketchEngine.Utilities.UI
{
    public class UIRawImageAnimated : MonoCached
    {
        [SerializeField] Texture[] _frames;

        [Min(1)]
        [SerializeField] int _fps = 30;
        [SerializeField] int _loopCount = 0;
        [ShowIf("@_loopCount < 0")]
        [SerializeField] LoopType _loopType;

        RawImage _rawImage;

        Sequence _sequence;

        public Sequence Sequence
        {
            get
            {
                if (_sequence == null)
                    InitSequence();
                return _sequence;
            }
        }

        RawImage RawImage
        {
            get
            {
                if (_rawImage == null)
                    _rawImage = GetComponentInChildren<RawImage>();
                return _rawImage;
            }
        }

        #region MonoBehaviour

        void Awake()
        {
            if (_rawImage == null) _rawImage = RawImage;
            InitSequence();
        }

        void OnDestroy()
        {
            _sequence?.Kill();
        }

        protected virtual void OnEnable()
        {
            _sequence?.Restart();
            _sequence?.Play();
        }

        protected virtual void OnDisable()
        {
            _sequence?.Pause();
        }

        #endregion

        void InitSequence()
        {
            if (_sequence != null)
                return;

            float delayBetween = 1.0f / _fps;

            _sequence = DOTween.Sequence();

            for (int i = 0; i < _frames.Length; i++)
            {
                int frameIndex = i;

                _sequence.AppendCallback(() => { SetFrame(frameIndex); });
                _sequence.AppendInterval(delayBetween);
            }

            _sequence.SetLoops(_loopCount, _loopType);
            _sequence.SetAutoKill(false);
        }

        void SetFrame(int frameIndex)
        {
            _rawImage.texture = _frames[frameIndex];
        }

        void OnValidate()
        {
            if (_rawImage == null) _rawImage = RawImage;
        }
    }
}
