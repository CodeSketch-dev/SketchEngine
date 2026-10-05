using UnityEngine;

namespace SketchEngine.Mono
{
    public class MonoCached : MonoBehaviour
    {
        GameObject _gameObject;
        Transform _transform;
        RectTransform _rectTransform;

        public Transform TransformCached
        {
            get
            {
                if (!_transform)
                    _transform = transform;

                return _transform;
            }
        }

        public RectTransform RectTransformCached
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = TransformCached as RectTransform;
                return _rectTransform;
            }
        }

        public GameObject GameObjectCached
        {
            get
            {
                if (!_gameObject)
                    _gameObject = gameObject;

                return _gameObject;
            }
        }

        public virtual void Tick()
        {
            // For Override
        }

        public virtual void LateTick()
        {
            // For Override
        }

        public virtual void FixedTick()
        {
            // For Override
        }
    }
}
