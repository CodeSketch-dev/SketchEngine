using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;

using SketchEngine.Audio;
using SketchEngine.Core.Extensions;
using SketchEngine.Core.Extensions.CSharp;
using SketchEngine.Mono;

namespace SketchEngine.UIPopup
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Popup : MonoCached
    {
        [SerializeField, HideInInspector] CanvasGroup _canvasGroup;

        [Title("Config")]
        [SerializeField, Min(0.01f)] float _openDuration = 0.1f;
        [SerializeField, Min(0.01f)] float _closeDuration = 0.1f;
        [SerializeField] bool _ignoreTimeScale;
        [SerializeField] AudioConfig _sfxOpen;
        [SerializeField] AudioConfig _sfxClose;

        [Space]

        [FoldoutGroup("Animation", Expanded = false)]
        [ListDrawerSettings(ListElementLabelName = "displayName", AddCopiesLastElement = false)]
        [SerializeReference] PopupAnimation[] _animations;

        [Space]

        [FoldoutGroup("Event", Expanded = false)]
        [SerializeField] UnityEvent _onOpenStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onOpenEnd;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onCloseStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onCloseEnd;

        bool _isOpening;
        bool _isEnabled;
        bool _isClosing;
        bool _closeEndRaised;

        Sequence _sequence;

        public event Action Destroyed;

        public float OpenDuration => _openDuration;
        public float CloseDuration => _closeDuration;
        public bool IgnoreTimeScale => _ignoreTimeScale;
        public bool IsClosing => _isClosing;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        public Sequence sequence => _sequence;

        public UnityEvent OnOpenStart => _onOpenStart;
        public UnityEvent OnOpenEnd => _onOpenEnd;
        public UnityEvent OnCloseStart => _onCloseStart;
        public UnityEvent OnCloseEnd => _onCloseEnd;

        #region MonoBehaviour

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnDestroy()
        {
            Destroyed?.Invoke();
            _sequence?.Kill();
        }

        void Update()
        {
            if (_isEnabled)
                InputCheck();
        }

        protected void OnEnable()
        {
            PopupManager.PushToStack(this);

            _isOpening = true;
            _isEnabled = true;
            _isClosing = false;
            _closeEndRaised = false;

            _onOpenStart?.Invoke();

            ConstructSequence();

            // ProcessClose đổi timeScale để chạy ngược đúng thời lượng close; phải reset về 1 trước khi mở lại.
            _sequence.timeScale = 1f;
            _sequence.Restart();

            Play_Sfx_Open();
            _sequence.Play();
        }

        protected void OnDisable()
        {
            PopupManager.PopFromStack(this);

            RaiseCloseEnd();
        }

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null) _canvasGroup = CanvasGroup;
        }

        #endregion

        #region Function -> Public

        public void Close()
        {
            ProcessClose(false);
        }

        public void CloseForced(bool immediately = false)
        {
            ProcessClose(immediately);
        }

        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        #endregion

        #region Function -> Private

        void ConstructSequence()
        {
            if (_sequence != null)
                return;

            _sequence = DOTween.Sequence();

            if (_animations != null && _animations.Length > 0)
            {
                for (int i = 0; i < _animations.Length; i++)
                {
                    if (_animations[i] != null)
                        _animations[i].Apply(this, _sequence);
                }
            }
            else
            {
                _sequence.AppendInterval(_openDuration);
            }

            _sequence.SetUpdate(_ignoreTimeScale);
            _sequence.OnComplete(Sequence_OnComplete);
            _sequence.OnRewind(Sequence_OnRewind);
            _sequence.SetAutoKill(false);
        }

        // OnComplete (không phải OnStepComplete) để event mở chỉ bắn đúng 1 lần sau khi toàn bộ animation xong.
        void Sequence_OnComplete()
        {
            if (!_isOpening) return;

            // Bật interactable trước khi invoke: nếu listener gọi Close() thì ProcessClose sẽ tắt lại ngay sau.
            CanvasGroup.interactable = true;
            _onOpenEnd?.Invoke();
        }

        void Sequence_OnRewind()
        {
            Destroy(GameObjectCached);
        }

        // Bảo đảm onCloseEnd chỉ bắn 1 lần dù đi qua nhánh immediately hay Destroy qua OnDisable.
        void RaiseCloseEnd()
        {
            if (_closeEndRaised) return;

            _closeEndRaised = true;
            _onCloseEnd?.Invoke();
        }

        void ProcessClose(bool immediately)
        {
            if (_isClosing) return;

            ConstructSequence();

            if (_sequence.IsPlaying()) return;

            _isClosing = true;
            _isOpening = false;
            CanvasGroup.interactable = false;

            _onCloseStart?.Invoke();

            Play_Sfx_Close();

            if (_openDuration > 0f && _closeDuration > 0f)
                _sequence.timeScale = _openDuration / _closeDuration;

            _sequence.PlayBackwards();

            if (immediately)
            {
                RaiseCloseEnd();
                _sequence.Complete();
            }
        }

        void Play_Sfx_Open()
        {
            if (_sfxOpen) AudioManager.Play(_sfxOpen);
        }

        void Play_Sfx_Close()
        {
            if (_sfxClose) AudioManager.Play(_sfxClose);
        }

        void InputCheck()
        {
#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
#endif
        }

        [Button]
        void SetupRectTransform()
        {
            RectTransform rect = GetComponent<RectTransform>();

            rect.StretchByParent();
        }

        #endregion
    }
}
