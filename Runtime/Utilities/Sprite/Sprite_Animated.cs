using SketchEngine.Mono;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities
{
    public class Sprite_Animated : MonoCached
    {
        [SerializeField] SpriteRenderer _renderer;
        [SerializeField] Sprite[] _frames;

        [Min(1)]
        [SerializeField] int _fps = 30;
        [SerializeField] int _loopCount = 0;

        [ShowIf("@_loopCount < 0")]
        [SerializeField] LoopType _loopType;

        Sequence _sequence;

        SpriteRenderer Renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponentInChildren<SpriteRenderer>();
                return _renderer;
            }
        }

        public Sequence Sequence
        {
            get
            {
                if (_sequence == null)
                    InitSequence();
                return _sequence;
            }
        }

        #region MonoBehaviour

        void Awake()
        {
            if (_renderer == null)
                _renderer = Renderer;
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
            _renderer.sprite = _frames[frameIndex];
        }

        void OnValidate()
        {
            if (_renderer == null) _renderer = Renderer;
        }
    }
}