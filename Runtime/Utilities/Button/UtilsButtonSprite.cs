using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SketchEngine.Utilities.Extends
{
    public class UtilsButtonSprite : MonoBehaviour
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
        public bool triggerMouseRightDragOnEnter;

        // =====================================================
        // HOVER
        // =====================================================

        public enum HoverBehaviour
        {
            Custom,
            Change_Color,
            Change_Image,
            Change_SetActive,
        }

        public HoverBehaviour hoverBehaviourType = HoverBehaviour.Custom;
        Action _hoverEnter, _hoverExit;

        public Color hoverBehaviour_Color_Enter = Color.white;
        public Color hoverBehaviour_Color_Exit = Color.white;

        public SpriteRenderer hoverBehaviour_Image;
        public Sprite hoverBehaviour_Sprite_Exit;
        public Sprite hoverBehaviour_Sprite_Enter;

        public bool hoverBehaviour_Move;
        public Vector2 hoverBehaviour_Move_Amount;

        Vector3 _posExit;
        Vector3 _posEnter;

        public bool triggerMouseOutFuncOnClick;
        public bool clickThroughUI;

        // =====================================================
        // PRESS VISUAL
        // =====================================================

        public bool pressVisual = true;
        public Transform pressRoot;
        [Range(0.5f, 1f)] public float pressScale = 0.95f;
        public float pressOffsetY = -0.02f;
        public float pressDuration = 0.06f;
        public float releaseDuration = 0.08f;

        Transform _pressT;
        Vector3 _baseScale, _basePos;
        Vector3 _fromScale, _toScale;
        Vector3 _fromPos, _toPos;
        float _t, _dur;
        bool _playing;

        // =====================================================
        // UNITY
        // =====================================================

        void Awake()
        {
            if (_getWorldCamera == null)
                _getWorldCamera = () => Camera.main;

            _posExit = transform.localPosition;
            _posEnter = _posExit + (Vector3)hoverBehaviour_Move_Amount;

            SetupHover();

            _pressT = pressRoot != null ? pressRoot : transform;
            _baseScale = _pressT.localScale;
            _basePos = _pressT.localPosition;

            UIBlockHelper.Ensure(); // ensure once
        }

        void Update()
        {
            // Right drag update
            if (_draggingMouseRight)
                MouseRightDragUpdateFunc?.Invoke(_mouseRightDragStart, GetWorldPos());

            if (Input.GetMouseButtonUp(1))
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
            TouchRouter.Ensure(_getWorldCamera).Tick();
        }
#endif

        void OnDisable()
        {
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
            if (!clickThroughUI && UIBlockHelper.Instance.IsBlockedMouse()) return;

            ClickFunc?.Invoke();
            MouseDownOnceFunc?.Invoke();
            PressDown();

            if (triggerMouseOutFuncOnClick) OnMouseExit();
        }

        void OnMouseUp()
        {
            MouseUpOnceFunc?.Invoke();
            PressUp();
            UIBlockHelper.Instance.ReleaseMouseIfUp();
        }

        void OnMouseEnter()
        {
            if (!clickThroughUI && UIBlockHelper.Instance.IsPointerOverUI_MouseThisFrame) return;

            if (hoverBehaviour_Move) transform.localPosition = _posEnter;
            _hoverEnter?.Invoke();
            MouseOverOnceFunc?.Invoke();
            MouseOverOnceTooltipFunc?.Invoke();
        }

        void OnMouseExit()
        {
            if (hoverBehaviour_Move) transform.localPosition = _posExit;
            _hoverExit?.Invoke();
            MouseOutOnceFunc?.Invoke();
            MouseOutOnceTooltipFunc?.Invoke();
        }

        void OnMouseOver()
        {
            if (!clickThroughUI && UIBlockHelper.Instance.IsBlockedMouse()) return;

            if (Input.GetMouseButtonDown(1))
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
            if (!pressVisual) return;

            _fromScale = _pressT.localScale;
            _toScale = _baseScale * pressScale;
            _fromPos = _pressT.localPosition;
            _toPos = _basePos + Vector3.up * pressOffsetY;

            _t = 0f;
            _dur = pressDuration;
            _playing = true;
        }

        void PressUp()
        {
            if (!pressVisual) return;

            _fromScale = _pressT.localScale;
            _toScale = _baseScale;
            _fromPos = _pressT.localPosition;
            _toPos = _basePos;

            _t = 0f;
            _dur = releaseDuration;
            _playing = true;
        }

        // =====================================================
        // HOVER SETUP
        // =====================================================

        void SetupHover()
        {
            switch (hoverBehaviourType)
            {
                case HoverBehaviour.Change_Color:
                    _hoverEnter = () => hoverBehaviour_Image.color = hoverBehaviour_Color_Enter;
                    _hoverExit = () => hoverBehaviour_Image.color = hoverBehaviour_Color_Exit;
                    break;
                case HoverBehaviour.Change_Image:
                    _hoverEnter = () => hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Enter;
                    _hoverExit = () => hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Exit;
                    break;
                case HoverBehaviour.Change_SetActive:
                    _hoverEnter = () => hoverBehaviour_Image.gameObject.SetActive(true);
                    _hoverExit = () => hoverBehaviour_Image.gameObject.SetActive(false);
                    break;
                default:
                    _hoverEnter = _hoverExit = null;
                    break;
            }
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
            readonly List<RaycastResult> _hits = new List<RaycastResult>(8);

            bool _overUI;
            bool _captured;

            public bool IsPointerOverUI_MouseThisFrame => _overUI;

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
                var es = EventSystem.current;
                if (es == null) { _overUI = false; return; }

                if (_ped == null) _ped = new PointerEventData(es);
                _ped.position = Input.mousePosition;
                _hits.Clear();
                es.RaycastAll(_ped, _hits);

                _overUI = _hits.Count > 0;

                if (Input.GetMouseButtonDown(0) && _overUI) _captured = true;
                if (Input.GetMouseButtonUp(0)) _captured = false;
            }

            public bool IsBlockedMouse() => _captured || _overUI;
            public void ReleaseMouseIfUp() { if (Input.GetMouseButtonUp(0)) _captured = false; }
        }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        sealed class TouchRouter
        {
            static TouchRouter _inst;
            readonly Dictionary<int, UtilsButtonSprite> _down = new Dictionary<int, UtilsButtonSprite>(8);
            Func<Camera> _getCam;

            public static TouchRouter Ensure(Func<Camera> cam)
            {
                if (_inst == null) _inst = new TouchRouter { _getCam = cam };
                return _inst;
            }

            Camera Cam => _getCam != null ? _getCam() : Camera.main;

            public void Tick()
            {
                int tc = Input.touchCount;
                if (tc == 0) return;

                var es = EventSystem.current;
                for (int i = 0; i < tc; i++)
                {
                    var t = Input.GetTouch(i);
                    int id = t.fingerId;

                    if (t.phase == TouchPhase.Began)
                    {
                        if (es != null && es.IsPointerOverGameObject(id)) continue;

                        var btn = Raycast(t.position);
                        if (btn != null)
                        {
                            _down[id] = btn;
                            btn.PressDown();
                            btn.MouseDownOnceFunc?.Invoke();
                        }
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        if (_down.TryGetValue(id, out var pressed))
                        {
                            pressed.PressUp();
                            pressed.MouseUpOnceFunc?.Invoke();

                            if (Raycast(t.position) == pressed)
                                pressed.ClickFunc?.Invoke();
                        }
                        _down.Remove(id);
                    }
                }
            }

            UtilsButtonSprite Raycast(Vector2 pos)
            {
                var cam = Cam;
                if (cam == null) return null;

                var ray = cam.ScreenPointToRay(pos);
                var hit2D = Physics2D.GetRayIntersection(ray);
                if (hit2D.collider)
                    return hit2D.collider.GetComponentInParent<UtilsButtonSprite>();

                if (Physics.Raycast(ray, out var hit))
                    return hit.collider.GetComponentInParent<UtilsButtonSprite>();

                return null;
            }
        }
#endif
    }
}
