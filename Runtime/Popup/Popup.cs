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

        Sequence _sequence;

        public float OpenDuration => _openDuration;
        public float CloseDuration => _closeDuration;

        public bool IgnoreTimeScale => _ignoreTimeScale;

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
            // Get canvas group component and disable UI constrol at begin
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnDestroy()
        {
            // Kill tweens
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

            _onOpenStart?.Invoke();

            ConstructSequence();
            _sequence.Restart();

            Play_Sfx_Open(); // 🔊 Phát khi bắt đầu Play()
            _sequence.Play();
        }

        protected void OnDisable()
        {
            // Pop this popup out of stack
            PopupManager.PopFromStack(this);

            // Trigger close end event
            _onCloseEnd?.Invoke();
        }

        protected virtual void OnValidate()
        {
            if (_canvasGroup == null) _canvasGroup = CanvasGroup;
        }

        #endregion

        #region Function -> Public

        public void Close()
        {
            ProcessClose();
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

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            if (!_animations.IsNullOrEmpty())
            {
                for (int i = 0; i < _animations.Length; i++)
                    _animations[i].Apply(this, _sequence);
            }
            else
            {
                _sequence.AppendInterval(_openDuration);
            }

            _sequence.SetUpdate(_ignoreTimeScale);

            _sequence.OnStepComplete(Sequence_OnStepComplete);
            _sequence.OnRewind(Sequence_OnRewind);

            _sequence.SetAutoKill(false);
        }

        void Sequence_OnStepComplete()
        {
            if (_isOpening)
            {
                _onOpenEnd?.Invoke();
                _canvasGroup.interactable = true;
            }
        }

        void Sequence_OnRewind()
        {
            // Thực hiện các hành động khi popup đóng
            Destroy(GameObjectCached);
        }

        void ProcessClose(bool immediately = false)
        {
            if (_sequence.IsPlaying()) return;

            _isOpening = false;
            _canvasGroup.interactable = false;

            _onCloseStart?.Invoke();

            Play_Sfx_Close();         // 🔊 Phát ngay trước khi PlayBackwards()

            if (_openDuration > 0f && _closeDuration > 0f)
                _sequence.timeScale = _openDuration / _closeDuration;

            _sequence.PlayBackwards();

            if (immediately)
            {
                _onCloseEnd?.Invoke();
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