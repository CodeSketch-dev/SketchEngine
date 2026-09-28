using SketchEngine.Core.Extensions;
using SketchEngine.Mono;
using UnityEngine;

#if CODESKETCH_VERTX
using Vertx.Debugging;
#endif

namespace SketchEngine.Diagnostics
{
    public class CodeSketchOrthorgrahphicDraw : MonoBase
    {
#if UNITY_EDITOR
        [SerializeField] Bounds _bounds = new Bounds();
        [SerializeField] Camera _camera;
        [SerializeField] Color _color = Color.blue;

        void OnValidate()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();
        }

        void OnDrawGizmos()
        {
#if CODESKETCH_VERTX
            if (_camera == null) return;

            Vector2 size = new Vector2(_camera.GetWidth(), _camera.GetHeight());
            _bounds.center = new Vector3(TransformCached.position.x, TransformCached.position.y, 0);
            _bounds.size = size;
            D.raw(_bounds, _color);
#endif
        }
#endif
    }
}
