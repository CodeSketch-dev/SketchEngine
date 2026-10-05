using System;
using SketchEngine.Core.Extensions;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.CameraSystem
{
    public class PerspectiveFitSize : MonoBehaviour
    {
        internal enum CallMethod
        {
            Awake = 0,
            Start = 1,
            OnEnable = 2,
            None = 3
        }

        [SerializeField] CallMethod _callMethod;
        [SerializeField] Vector2 _viewSize;
        [SerializeField] float _distance;

        Camera _camera;

        Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponentInChildren<Camera>();
                if (_camera == null)
                    _camera = Camera.main;
                return _camera;
            }
        }

        void Awake()
        {
            if (_callMethod == CallMethod.Awake)
            {
                FitView();
            }
        }

        void Start()
        {
            if (_callMethod == CallMethod.Start)
            {
                FitView();
            }
        }

        void OnEnable()
        {
            if (_callMethod == CallMethod.OnEnable)
            {
                FitView();
            }
        }

        void FitView()
        {
            if (Camera == null)
            {
#if UNITY_EDITOR
                throw new Exception("Main camera doesn't exist on this scene.");
#endif
                return;
            }

            float fov = Camera.FOVFitRectAtDistance(_viewSize, _distance);
            Camera.fieldOfView = fov;
        }

        public Tween FitView(Vector2 viewSize, float distance, float duration, Ease ease, Action onStart = null, Action onComplete = null)
        {
            if (Camera == null)
            {
#if UNITY_EDITOR
                throw new Exception("Main camera doesn't exist on this scene.");
#endif
                return null;
            }

            float fov = Camera.FOVFitRectAtDistance(_viewSize, _distance);

            Tween tween = Camera.DOFieldOfView(fov, duration).SetEase(ease).OnStart(() => onStart?.Invoke())
                .OnComplete(() => onComplete?.Invoke());
            return tween;
        }

#if UNITY_EDITOR
        [Button]
        void SaveThisView()
        {
            Plane plane = new Plane(Vector3.up, Vector2.zero);
            Ray ray = Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

            if (plane.Raycast(ray, out var length))
            {
                Vector3 point = ray.GetPoint(length);

                Vector3 localPoint = Camera.transform.InverseTransformPoint(point);
                _distance = Vector3.Distance(localPoint, Camera.transform.position);

                _viewSize = Camera.SizeAtDistance(_distance);
            }
        }
#endif
    }
}
