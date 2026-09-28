using System;
using System.Collections.Generic;
using SketchEngine.Core.Extensions;
using SketchEngine.Core.Extensions.CSharp;
using SketchEngine.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SketchEngine.UIPopup
{
    public static class PopupManager
    {
        // Global stack of popup
        static readonly List<Popup> _mainStack = new List<Popup>();

        static Transform _root;
        static Canvas _canvas;

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
                if (_root == null)
                {
                    var all = Object.FindObjectsOfType<Canvas>();
                    if (all == null) return null;

                    EventRootUndefine?.Invoke();
                    if (!_root) return _root;

                    Canvas selected = null;
                    PopupRootSetter rootSetter = null;

                    foreach (var canvas in all)
                    {
                        if (canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            rootSetter = canvas.GetComponentInChildren<PopupRootSetter>();

                            if (selected == null)
                            {
                                if (canvas.enabled)
                                {
                                    selected = canvas;
                                }

                                if (rootSetter == null)
                                {
                                    selected = null;
                                }
                            }
                            else
                            {
                                if (rootSetter == null)
                                {
                                    selected = null;
                                    continue;
                                }

                                if (selected.sortingOrder < canvas.sortingOrder && canvas.enabled) selected = canvas;
                            }
                        }
                    }

                    if (rootSetter)
                    {
                        _root = rootSetter.transform;
                        _canvas = rootSetter.GetComponentInParent<Canvas>();
                    }

                    if (!_root && selected)
                    {
                        _root = selected.transform;
                        _canvas = selected;
                    }

                }

                return _root;
            }
        }

        public static event Action EventRootUndefine;
        public static event Action<Popup> EventPopupOpened;
        public static event Action<Popup> EventPopupClosed;
        public static int Count => _mainStack.Count;

        public static void PushToStack(Popup popup, bool isTopHidden = true)
        {
            // Disable current top popup
            if (_mainStack.Count > 0 && isTopHidden)
                _mainStack.Last().SetEnabled(false);

            // Add this popup into stack
            _mainStack.Add(popup);

            EventPopupOpened?.Invoke(popup);
        }

        public static void PopFromStack(Popup popup)
        {
            if (_mainStack.Count == 0)
            {
                CodeSketchDebug.LogWarning(typeof(PopupManager), "There is no popup in stack");

                return;
            }
            else if (_mainStack.Last() != popup)
            {
                CodeSketchDebug.LogWarning(typeof(PopupManager), $"This popup {popup} is not on top of the stack! try to remove it from stack anyway", Color.cyan);

                _mainStack.Remove(popup);

                return;
            }

            // Pop top popup from stack
            _mainStack.RemoveAt(_mainStack.Count - 1);

            // Get current top popup and enable it
            if (_mainStack.Count > 0)
                _mainStack.Last().SetEnabled(true);

            EventPopupClosed?.Invoke(popup);
        }

        public static Popup Create(GameObject prefab)
        {
            if (_root == null) EventRootUndefine?.Invoke();

            Popup popup = prefab.Create(_root != null ? _root : Root, false).GetComponent<Popup>();
            popup.TransformCached.SetAsLastSibling();

            return popup;
        }

        public static Popup Create(GameObject prefab, Transform specifiedRoot)
        {
            Popup popup = prefab.Create(specifiedRoot, false).GetComponent<Popup>();
            popup.TransformCached.SetAsLastSibling();

            return popup;
        }

        public static void SetRoot(Transform target, bool force = false)
        {
            if (_root == null || force)
            {
                _root = target;
                _canvas = target ? target.GetComponentInParent<Canvas>() : null;
            }
        }
    }
}
