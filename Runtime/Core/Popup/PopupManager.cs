using System;
using System.Collections.Generic;
using SketchEngine.Core.Extensions;
using SketchEngine.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SketchEngine.UIPopup
{
    public static class PopupManager
    {
        static readonly List<Popup> _mainStack = new List<Popup>();

        static Transform _root;
        static Canvas _canvas;

        public static event Action EventRootUndefine;
        public static event Action<Popup> EventPopupOpened;
        public static event Action<Popup> EventPopupClosed;

        public static int Count => _mainStack.Count;

        public static Canvas Canvas
        {
            get
            {
                if (_canvas == null) _ = Root;
                return _canvas;
            }
        }

        public static Transform Root
        {
            get
            {
                if (_root != null) return _root;

                EventRootUndefine?.Invoke();
                if (_root != null) return _root;

                SelectRootFromScene();
                return _root;
            }
        }

        // Ưu tiên canvas có PopupRootSetter (sortingOrder cao nhất), không có thì lấy canvas overlay enabled có sortingOrder cao nhất.
        static void SelectRootFromScene()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            Canvas fallback = null;
            Canvas withSetter = null;
            PopupRootSetter setter = null;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || !canvas.enabled || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;

                if (fallback == null || canvas.sortingOrder > fallback.sortingOrder)
                    fallback = canvas;

                PopupRootSetter candidate = canvas.GetComponentInChildren<PopupRootSetter>();
                if (candidate != null && (withSetter == null || canvas.sortingOrder > withSetter.sortingOrder))
                {
                    withSetter = canvas;
                    setter = candidate;
                }
            }

            if (setter != null)
            {
                _root = setter.transform;
                _canvas = setter.GetComponentInParent<Canvas>();
            }
            else if (fallback != null)
            {
                _root = fallback.transform;
                _canvas = fallback;
            }
        }

        public static void PushToStack(Popup popup, bool isTopHidden = true)
        {
            int count = _mainStack.Count;
            if (isTopHidden && count > 0)
            {
                Popup top = _mainStack[count - 1];
                if (top) top.SetEnabled(false);
            }

            _mainStack.Add(popup);

            EventPopupOpened?.Invoke(popup);
        }

        public static void PopFromStack(Popup popup)
        {
            int count = _mainStack.Count;

            if (count == 0)
            {
                SketchDebug.LogWarning(typeof(PopupManager), "There is no popup in stack");
                return;
            }

            if (_mainStack[count - 1] != popup)
            {
                SketchDebug.LogWarning(typeof(PopupManager), $"This popup {popup} is not on top of the stack! try to remove it from stack anyway", Color.cyan);
                _mainStack.Remove(popup);
                return;
            }

            _mainStack.RemoveAt(count - 1);

            if (count > 1)
            {
                Popup next = _mainStack[count - 2];
                if (next) next.SetEnabled(true);
            }

            EventPopupClosed?.Invoke(popup);
        }

        public static Popup Create(GameObject prefab) => Create(prefab, Root);

        public static Popup Create(GameObject prefab, Transform specifiedRoot)
        {
            if (prefab == null)
            {
                SketchDebug.LogError(typeof(PopupManager), "Create popup failed: prefab is null");
                return null;
            }

            GameObject instance = prefab.Create(specifiedRoot, false);
            Popup popup = instance.GetComponent<Popup>();

            if (popup == null)
            {
                SketchDebug.LogError(typeof(PopupManager), $"Prefab {prefab.name} has no Popup component");
                Object.Destroy(instance);
                return null;
            }

            popup.TransformCached.SetAsLastSibling();

            return popup;
        }

        public static void SetRoot(Transform target, bool force = false)
        {
            if (_root != null && !force) return;

            _root = target;
            _canvas = target ? target.GetComponentInParent<Canvas>() : null;
        }
    }
}
