using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SketchEngine.Utilities
{
    public class Sprite_Button : MonoBehaviour
    {
        // =====================================================
        // CAMERA
        // =====================================================

        static Func<Camera> _getWorldCamera;
        static Camera _cachedCam;
        static int _camFrame = -1;

        static Camera WorldCam
        {
            get
            {
                if (_camFrame != Time.frameCount)
                {
                    _camFrame = Time.frameCount;
                    _cachedCam = _getWorldCamera != null ? _getWorldCamera() : Camera.main;
                }
                return _cachedCam;
            }
        }

        public static void SetGetWorldCamera(Func<Camera> getWorldCamera)
        {
            _getWorldCamera = getWorldCamera;
            _camFrame = -1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _getWorldCamera = null;
            _cachedCam = null;
            _camFrame = -1;
            UIBlockHelper.Reset();
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            TouchRouter.Reset();
#endif
        }

        // =====================================================
        // EVENTS
        // =====================================================

        public Action ClickFunc;
        public Action MouseRightDownOnceFunc;
        public Action MouseRightDownFunc;
        public Action MouseRightUpFunc;
        public Action MouseDownOnceFunc;
        public Action MouseUpOnceFunc;
        public Action MouseOverOnceFunc;
        public Action MouseOutOnceFunc;
        public Action MouseOverOnceTooltipFunc;
        public Action MouseOutOnceTooltipFunc;

        bool _draggingMouseRight;
        Vector3 _mouseRightDragStart;
        public Action<Vector3, Vector3> MouseRightDragFunc;
        public Action<Vector3, Vector3> MouseRightDragUpdateFunc;
        [FoldoutGroup("Nâng cao"), LabelText("Kéo chuột phải khi đi vào")]
        [Tooltip("Bắt đầu kéo khi con trỏ đi vào nút trong lúc đang giữ chuột phải.")]
        public bool TriggerMouseRightDragOnEnter;

        // =====================================================
        // HOVER
        // =====================================================

        public enum HoverBehaviour
        {
            [LabelText("Không đổi hình ảnh (callback tùy chỉnh)")] Custom,
            [LabelText("Đổi màu")] Change_Color,
            [LabelText("Đổi sprite")] Change_Image,
            [LabelText("Hiện / ẩn đối tượng")] Change_SetActive,
        }

        [BoxGroup("Khi rê chuột"), LabelText("Hiệu ứng")]
        public HoverBehaviour HoverBehaviourType = HoverBehaviour.Custom;

        [BoxGroup("Khi rê chuột"), LabelText("Màu khi rê vào")]
        [ShowIf(nameof(HoverBehaviourType), HoverBehaviour.Change_Color)]
        public Color HoverBehaviour_Color_Enter = Color.white;
        [BoxGroup("Khi rê chuột"), LabelText("Màu khi rời khỏi")]
        [ShowIf(nameof(HoverBehaviourType), HoverBehaviour.Change_Color)]
        public Color HoverBehaviour_Color_Exit = Color.white;

        [BoxGroup("Khi rê chuột"), LabelText("Sprite hiển thị")]
        [ShowIf("@HoverBehaviourType != HoverBehaviour.Custom")]
        [Tooltip("Để trống sẽ tự lấy SpriteRenderer trên đối tượng này.")]
        public SpriteRenderer HoverBehaviour_Image;
        [BoxGroup("Khi rê chuột"), LabelText("Sprite khi rời khỏi")]
        [ShowIf(nameof(HoverBehaviourType), HoverBehaviour.Change_Image)]
        public Sprite HoverBehaviour_Sprite_Exit;
        [BoxGroup("Khi rê chuột"), LabelText("Sprite khi rê vào")]
        [ShowIf(nameof(HoverBehaviourType), HoverBehaviour.Change_Image)]
        public Sprite HoverBehaviour_Sprite_Enter;

        [FoldoutGroup("Nâng cao"), LabelText("Dịch chuyển khi rê chuột")]
        public bool HoverBehaviour_Move;
        [FoldoutGroup("Nâng cao"), LabelText("Khoảng dịch chuyển"), ShowIf(nameof(HoverBehaviour_Move))]
        public Vector2 HoverBehaviour_Move_Amount;

        Vector3 _posExit;
        Vector3 _posEnter;

        [FoldoutGroup("Nâng cao"), LabelText("Thoát hover khi nhấn")]
        [Tooltip("Tắt trạng thái hover và gọi callback rời nút ngay khi nhấn chuột.")]
        public bool TriggerMouseOutFuncOnClick;
        [FoldoutGroup("Nâng cao"), LabelText("Cho phép nhấn xuyên UI")]
        [Tooltip("Cho nút nhận input ngay cả khi con trỏ nằm trên giao diện UI.")]
        public bool ClickThroughUI;

        // =====================================================
        // PRESS VISUAL
        // =====================================================

        [BoxGroup("Khi nhấn"), LabelText("Hiệu ứng nhấn")]
        public bool PressVisual = true;
        [FoldoutGroup("Nâng cao"), LabelText("Đối tượng tạo hiệu ứng"), ShowIf(nameof(PressVisual))]
        [Tooltip("Để trống để thu nhỏ và dịch chuyển chính đối tượng này.")]
        public Transform PressRoot;
        [BoxGroup("Khi nhấn"), LabelText("Tỷ lệ thu nhỏ"), ShowIf(nameof(PressVisual))]
        [Tooltip("0.95 nghĩa là thu nhỏ còn 95% kích thước ban đầu khi nhấn.")]
        [Range(0.5f, 1f)] public float PressScale = 0.95f;
        [FoldoutGroup("Nâng cao"), LabelText("Độ dịch chuyển Y"), ShowIf(nameof(PressVisual))]
        public float PressOffsetY = -0.02f;
        [FoldoutGroup("Nâng cao"), LabelText("Thời gian nhấn (giây)"), ShowIf(nameof(PressVisual)), Min(0f)]
        public float PressDuration = 0.06f;
        [FoldoutGroup("Nâng cao"), LabelText("Thời gian thả (giây)"), ShowIf(nameof(PressVisual)), Min(0f)]
        public float ReleaseDuration = 0.08f;

        Transform _pressT;
        Vector3 _baseScale, _basePos;
        Vector3 _fromScale, _toScale;
        Vector3 _fromPos, _toPos;
        float _t, _dur;
        bool _playing;
        bool _hovered;
        bool _pressed;

        // =====================================================
        // UNITY
        // =====================================================

        void Awake()
        {
            EnsureHitArea();
            _posExit = transform.localPosition;
            _posEnter = _posExit + (Vector3)HoverBehaviour_Move_Amount;

            _pressT = PressRoot != null ? PressRoot : transform;
            _baseScale = _pressT.localScale;
            _basePos = _pressT.localPosition;

            UIBlockHelper.Ensure(); // ensure once
        }

        void Reset()
        {
            HoverBehaviour_Image = GetComponent<SpriteRenderer>();
            EnsureHitArea();
        }

        void EnsureHitArea()
        {
            if (HoverBehaviour_Image == null)
                HoverBehaviour_Image = GetComponent<SpriteRenderer>();

            // OnMouse và raycast touch cần collider để nhận tương tác.
            if (GetComponentInChildren<Collider2D>() != null || GetComponentInChildren<Collider>() != null)
                return;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;

            var collider = gameObject.AddComponent<BoxCollider2D>();
            var bounds = spriteRenderer.sprite.bounds;
            collider.offset = spriteRenderer.drawMode == SpriteDrawMode.Simple
                ? (Vector2)bounds.center
                : (Vector2)((Vector3)spriteRenderer.size * 0.5f -
                    Vector3.Scale((Vector3)spriteRenderer.size,
                        (Vector3)(spriteRenderer.sprite.pivot / spriteRenderer.sprite.rect.size)));
            if (spriteRenderer.flipX) collider.offset = new Vector2(-collider.offset.x, collider.offset.y);
            if (spriteRenderer.flipY) collider.offset = new Vector2(collider.offset.x, -collider.offset.y);
            collider.size = spriteRenderer.drawMode == SpriteDrawMode.Simple
                ? (Vector2)bounds.size : spriteRenderer.size;
        }

        void Update()
        {
            // Right drag update
            if (_draggingMouseRight)
                MouseRightDragUpdateFunc?.Invoke(_mouseRightDragStart, GetWorldPos());

            if (_draggingMouseRight && Input.GetMouseButtonUp(1))
            {
                if (_draggingMouseRight)
                {
                    _draggingMouseRight = false;
                    MouseRightDragFunc?.Invoke(_mouseRightDragStart, GetWorldPos());
                }
                MouseRightUpFunc?.Invoke();
            }

            // press animation
            if (!_playing) return;

            if (_dur <= 0f)
            {
                _pressT.localScale = _toScale;
                _pressT.localPosition = _toPos;
                _playing = false;
                return;
            }

            _t += Time.unscaledDeltaTime / _dur;
            if (_t >= 1f) _t = 1f;

            float e = 1f - (1f - _t) * (1f - _t);
            _pressT.localScale = Vector3.LerpUnclamped(_fromScale, _toScale, e);
            _pressT.localPosition = Vector3.LerpUnclamped(_fromPos, _toPos, e);

            if (_t >= 1f) _playing = false;
        }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        void LateUpdate()
        {
            TouchRouter.Ensure().Tick();
        }
#endif

        void OnDisable()
        {
            _draggingMouseRight = false;
            _pressed = false;
            _hovered = false;
            _playing = false;
            if (HoverBehaviour_Move) transform.localPosition = _posExit;
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            TouchRouter.Forget(this);
#endif
            if (_pressT != null)
            {
                _pressT.localScale = _baseScale;
                _pressT.localPosition = _basePos;
                _playing = false;
            }
        }

        // =====================================================
        // MOUSE EVENTS
        // =====================================================

        void OnMouseDown()
        {
            if (Input.touchCount > 0 || !isActiveAndEnabled) return;
            if (!ClickThroughUI && UIBlockHelper.Instance.IsBlockedMouse()) return;

            _pressed = true;
            ClickFunc?.Invoke();
            if (!isActiveAndEnabled) return;
            MouseDownOnceFunc?.Invoke();
            if (!isActiveAndEnabled) return;
            PressDown();

            if (TriggerMouseOutFuncOnClick) OnMouseExit();
        }

        void OnMouseUp()
        {
            if (Input.touchCount > 0 || !_pressed) return;
            _pressed = false;
            MouseUpOnceFunc?.Invoke();
            PressUp();
            UIBlockHelper.Instance.ReleaseMouseIfUp();
        }

        void OnMouseEnter()
        {
            if (_hovered || !isActiveAndEnabled) return;
            if (!ClickThroughUI && UIBlockHelper.Instance.IsPointerOverUI_MouseThisFrame) return;

            _hovered = true;
            RefreshHoverPosition();
            ApplyHoverVisual(true);
            MouseOverOnceFunc?.Invoke();
            MouseOverOnceTooltipFunc?.Invoke();
        }

        void OnMouseExit()
        {
            if (!_hovered) return;
            _hovered = false;
            RefreshHoverPosition();
            ApplyHoverVisual(false);
            MouseOutOnceFunc?.Invoke();
            MouseOutOnceTooltipFunc?.Invoke();
        }

        void OnMouseOver()
        {
            if (Input.touchCount > 0 || !isActiveAndEnabled) return;
            if (!ClickThroughUI && UIBlockHelper.Instance.IsBlockedMouse()) return;
            if (!_hovered) OnMouseEnter();

            if (Input.GetMouseButtonDown(1) || (TriggerMouseRightDragOnEnter && !_draggingMouseRight && Input.GetMouseButton(1)))
            {
                _draggingMouseRight = true;
                _mouseRightDragStart = GetWorldPos();
                MouseRightDownOnceFunc?.Invoke();
            }

            if (Input.GetMouseButton(1))
                MouseRightDownFunc?.Invoke();
        }

        // =====================================================
        // PRESS VISUAL
        // =====================================================

        void PressDown()
        {
            _pressed = true;
            if (!PressVisual) return;

            _fromScale = _pressT.localScale;
            _toScale = _baseScale * PressScale;
            _fromPos = _pressT.localPosition;
            _toPos = RestPosition + Vector3.up * PressOffsetY;

            _t = 0f;
            _dur = PressDuration;
            _playing = true;
        }

        void PressUp()
        {
            if (!PressVisual) return;

            _fromScale = _pressT.localScale;
            _toScale = _baseScale;
            _fromPos = _pressT.localPosition;
            _toPos = RestPosition;

            _t = 0f;
            _dur = ReleaseDuration;
            _playing = true;
        }

        // =====================================================
        // HOVER SETUP
        // =====================================================

        void ApplyHoverVisual(bool entered)
        {
            if (HoverBehaviour_Image == null) return;
            switch (HoverBehaviourType)
            {
                case HoverBehaviour.Change_Color:
                    HoverBehaviour_Image.color = entered ? HoverBehaviour_Color_Enter : HoverBehaviour_Color_Exit;
                    break;
                case HoverBehaviour.Change_Image:
                    HoverBehaviour_Image.sprite = entered ? HoverBehaviour_Sprite_Enter : HoverBehaviour_Sprite_Exit;
                    break;
                case HoverBehaviour.Change_SetActive:
                    HoverBehaviour_Image.gameObject.SetActive(entered);
                    break;
                default:
                    break;
            }
        }

        Vector3 RestPosition => HoverBehaviour_Move && _pressT == transform
            ? (_hovered ? _posEnter : _posExit) : _basePos;

        void RefreshHoverPosition()
        {
            if (!HoverBehaviour_Move) return;
            if (_pressT != transform)
                transform.localPosition = _hovered ? _posEnter : _posExit;
            else if (_playing)
                _toPos = RestPosition + (_pressed && PressVisual ? Vector3.up * PressOffsetY : Vector3.zero);
            else
                _pressT.localPosition = RestPosition + (_pressed && PressVisual ? Vector3.up * PressOffsetY : Vector3.zero);
        }

        static Vector3 GetWorldPos()
        {
            var cam = WorldCam;
            return cam != null
                ? cam.ScreenToWorldPoint(Input.mousePosition)
                : Vector3.zero;
        }

        // =====================================================
        // UI BLOCK HELPER (GLOBAL, SINGLE UPDATE)
        // =====================================================

        sealed class UIBlockHelper : MonoBehaviour
        {
            static UIBlockHelper _inst;
            public static UIBlockHelper Instance => _inst;

            PointerEventData _ped;
            EventSystem _eventSystem;
            int _frame = -1;
            readonly List<RaycastResult> _hits = new List<RaycastResult>(8);

            bool _overUI;
            bool _captured;

            public bool IsPointerOverUI_MouseThisFrame { get { RefreshMouse(); return _overUI; } }
            public static void Reset() { _inst = null; }

            public static UIBlockHelper Ensure()
            {
                if (_inst != null) return _inst;
                var go = new GameObject("[UIBlockHelper]");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<UIBlockHelper>();
                return _inst;
            }

            void Update()
            {
                RefreshMouse();
            }

            void RefreshMouse()
            {
                if (_frame == Time.frameCount) return;
                _frame = Time.frameCount;
                _overUI = IsOverUI(Input.mousePosition, -1);

                if (Input.GetMouseButtonDown(0) && _overUI) _captured = true;
                if (!Input.GetMouseButton(0)) _captured = false;
            }

            public bool IsOverUI(Vector2 position, int pointerId)
            {
                var es = EventSystem.current;
                if (es == null) return false;
                if (_ped == null || _eventSystem != es)
                {
                    _eventSystem = es;
                    _ped = new PointerEventData(es);
                }
                _ped.Reset();
                _ped.pointerId = pointerId;
                _ped.position = position;
                _hits.Clear();
                es.RaycastAll(_ped, _hits);
                bool blocked = false;
                for (int i = 0; i < _hits.Count; i++)
                    if (_hits[i].module is GraphicRaycaster) { blocked = true; break; }
                _hits.Clear();
                return blocked;
            }

            public bool IsBlockedMouse() { RefreshMouse(); return _captured || _overUI; }
            public void ReleaseMouseIfUp() { if (Input.GetMouseButtonUp(0)) _captured = false; }
        }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        sealed class TouchRouter
        {
            static TouchRouter _inst;
            readonly Dictionary<int, Sprite_Button> _down = new Dictionary<int, Sprite_Button>(8);
            readonly List<int> _removed = new List<int>(8);
            int _frame = -1;
            public static void Reset() { _inst = null; }

            public static void Forget(Sprite_Button button)
            {
                if (_inst == null) return;
                _inst._removed.Clear();
                foreach (var pair in _inst._down)
                    if (pair.Value == button) _inst._removed.Add(pair.Key);
                for (int i = 0; i < _inst._removed.Count; i++)
                    _inst._down.Remove(_inst._removed[i]);
                _inst._removed.Clear();
            }

            public static TouchRouter Ensure()
            {
                if (_inst == null) _inst = new TouchRouter();
                return _inst;
            }

            Camera Cam => WorldCam;

            public void Tick()
            {
                if (_frame == Time.frameCount) return;
                _frame = Time.frameCount;
                int tc = Input.touchCount;
                if (tc == 0)
                {
                    foreach (var pair in _down)
                    {
                        var button = pair.Value;
                        if (button == null || !button.isActiveAndEnabled) continue;
                        button._pressed = false;
                        button.PressUp();
                    }
                    _down.Clear();
                    return;
                }

                for (int i = 0; i < tc; i++)
                {
                    var t = Input.GetTouch(i);
                    int id = t.fingerId;

                    if (t.phase == TouchPhase.Began)
                    {
                        var btn = Raycast(t.position);
                        if (btn != null && btn.isActiveAndEnabled &&
                            (btn.ClickThroughUI || !UIBlockHelper.Ensure().IsOverUI(t.position, id)))
                        {
                            if (_down.ContainsValue(btn)) continue;
                            _down[id] = btn;
                            btn.PressDown();
                            btn.MouseDownOnceFunc?.Invoke();
                        }
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        if (_down.TryGetValue(id, out var pressed))
                        {
                            _down.Remove(id);
                            if (pressed == null || !pressed.isActiveAndEnabled) continue;
                            pressed._pressed = false;
                            pressed.PressUp();
                            pressed.MouseUpOnceFunc?.Invoke();

                            if (t.phase == TouchPhase.Ended && pressed != null && pressed.isActiveAndEnabled &&
                                (pressed.ClickThroughUI || !UIBlockHelper.Ensure().IsOverUI(t.position, id)) &&
                                Raycast(t.position) == pressed)
                                pressed.ClickFunc?.Invoke();
                        }
                        _down.Remove(id);
                    }
                }
            }

            Sprite_Button Raycast(Vector2 pos)
            {
                var cam = Cam;
                if (cam == null) return null;

                var ray = cam.ScreenPointToRay(pos);
                int mask = cam.cullingMask & Physics.DefaultRaycastLayers;
                var hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, mask);
                if (hit2D.collider)
                    return hit2D.collider.GetComponentInParent<Sprite_Button>();

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, mask))
                    return hit.collider.GetComponentInParent<Sprite_Button>();

                return null;
            }
        }
#endif
    }
}
