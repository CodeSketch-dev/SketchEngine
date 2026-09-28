using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities
{
    public class SpriteRendererFitSize : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool _applyTransformScale = true;
        
        SpriteRenderer _renderer;
        
        public new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<SpriteRenderer>();
                return _renderer;
            }
        }

        [Button]
        void Fit(Vector2 size)
        {
            if (_applyTransformScale)
            {
                Vector2 spriteSize = renderer.sprite.bounds.size;
                Vector2 scale = new Vector2(size.x / spriteSize.x, size.y / spriteSize.y);
                transform.localScale = scale;
            }
            else
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = size;
            }
        }
#endif
    }
}
