using SketchEngine.Core.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class TextWorldCanvasAnimationLocalMove : TextWorldCanvasAnimation
    {
        [System.Serializable]
        internal enum Point
        {
            Absolutely,
            Random
        }
        
        [System.Serializable]
        internal class Config
        {
            public Point type;
            
            [ShowIf("@type == Point.Absolutely")]
            public Vector3 absolutePoint;
            
            [ShowIf("@type == Point.Random")]
            public float randomRadius;

            public Vector3 GetPoint()
            {
                if (type == Point.Absolutely)
                {
                    return absolutePoint;
                }
                else
                {
                    return Random.insideUnitSphere * randomRadius;
                }
            }
        }

        [SerializeField] Transform _target;
        [LabelText("Start")]
        [SerializeField] Config _startConfig;
        [LabelText("End")]
        [SerializeField] Config _endConfig;
        [SerializeField] bool _is3D = false;
        [SerializeField] Ease _ease = Ease.OutSine;

        Transform _trans;
        
        public override string DisplayName => "Local Move";

        protected override Tween GetTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;
            
            Vector3 startPoint = _startConfig.GetPoint();
            startPoint.z = _is3D ? startPoint.z : 0f;
            
            Vector3 endPoint = _endConfig.GetPoint();
            endPoint.z = _is3D ? endPoint.z : 0f;

            return _trans.DOLocalMove(endPoint, duration).SetEase(_ease).ChangeStartValue(startPoint);
        }

        protected override PrimeTween.Tween GetPrimeTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;
            
            Vector3 startPoint = _startConfig.GetPoint();
            startPoint.z = _is3D ? startPoint.z : 0f;

            Vector3 endPoint = _endConfig.GetPoint();
            endPoint.z = _is3D ? endPoint.z : 0f;

            return PrimeTween.Tween.LocalPosition(_trans, startPoint, endPoint, duration, EaseMapper.MapEase(_ease));
        }
    }
}
