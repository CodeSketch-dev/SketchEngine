using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Utilities.UI
{
    public class UIImageAnimated : MonoCached
    {
        [SerializeField] Sprite[] _frames;

        [Min(1)]
        [SerializeField] int _fps = 30;
        [SerializeField] int _loopCount = 0;
        [ShowIf("@_loopCount < 0")]
        [SerializeField] LoopType _loopType;

        Image _image;
        Sequence _sequence;

        public Action OnRewind;
        public Action OnStart;
        public Action<int, int> OnAnimated;
        public Action OnStop;

        public Sequence Sequence
        {
            get
            {
                if (_sequence == null)
                    InitSequence();
                return _sequence;
            }
        }

        Image Image
        {
            get
            {
                if (_image == null)
                    _image = GetComponentInChildren<Image>();
                return _image;
            }
        }

        #region MonoBehaviour

        void Awake()
        {
            if (_image == null) _image = Image;
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
            OnRewind = null;
            _sequence?.Pause();
        }

        #endregion

        void InitSequence()
        {
            if (_sequence != null)
                return;

            float delayBetween = 1.0f / _fps;

            _sequence = DOTween.Sequence();

            _sequence.OnStart(() => OnStart?.Invoke());

            for (int i = 0; i < _frames.Length; i++)
            {
                int frameIndex = i;

                _sequence.AppendCallback(() => { SetFrame(frameIndex); });
                _sequence.AppendInterval(delayBetween);

                _sequence.AppendCallback(() => CallbackAnimated(i));
            }

            _sequence.OnComplete(() => OnStop?.Invoke());

            _sequence.OnRewind(() => OnRewind?.Invoke());

            _sequence.SetLoops(_loopCount, _loopType);
            _sequence.SetAutoKill(false);

            void CallbackAnimated(int i)
            {
                OnAnimated?.Invoke(i + 1, _frames.Length);
            }
        }

        public void PlayReverse()
        {
            GameObjectCached.SetActive(true);

            OnRewind += () => GameObjectCached.SetActive(false);

            _sequence.Pause();
            _sequence.PlayBackwards();
        }

        void SetFrame(int frameIndex)
        {
            _image.sprite = _frames[frameIndex];
        }

        void OnValidate()
        {
            if (_image == null) _image = Image;
        }
    }
}
