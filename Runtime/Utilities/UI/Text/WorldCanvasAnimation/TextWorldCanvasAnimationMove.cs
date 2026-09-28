using SketchEngine.Core.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class TextWorldCanvasAnimationMove : TextWorldCanvasAnimation
    {
        [System.Serializable]
        internal class MoveLocalConfig
        {
            public MoveType startType = MoveType.RandomInCircle;
            
            [ShowIf("@startType == MoveType.RandomInCircle")]
            public float startRadius;
            [ShowIf("@startType == MoveType.LocalMove")]
            public Vector3 startPosition;
            
            public MoveType endType = MoveType.LocalMove;
            [ShowIf("@endType == MoveType.RandomInCircle")]
            public float endRadius;
            [ShowIf("@endType == MoveType.LocalMove")]
            public Vector3 endPosition;

            public bool is3D = false;

            public Vector3 GetStart()
            {
                if (startType == MoveType.LocalMove)
                {
                    return startPosition;
                }
                else
                {
                    var random = Random.insideUnitCircle;
                    random = random.normalized * startRadius;

                    return is3D ? new Vector3(random.x, 0, random.y) : random;
                }
            }
            
            public Vector3 GetEnd()
            {
                if (endType == MoveType.LocalMove)
                {
                    return endPosition;
                }
                else
                {
                    var random = Random.insideUnitCircle;
                    random = random.normalized * endRadius;

                    return is3D ? new Vector3(random.x, 0, random.y) : random;
                }
            }
        }
        
        internal enum MoveType
        {
            RandomInCircle = 0,
            LocalMove = 1
        }

        [SerializeField] Transform _target;
        [SerializeField] MoveType _type;
        
        [ShowIf("@_type == MoveType.RandomInCircle")]
        [SerializeField] readonly float _radius = 1.25f;

        [ShowIf("@_type == MoveType.LocalMove")]
        [SerializeField] MoveLocalConfig _localConfig;

        [SerializeField] readonly bool _is3D = false;
        [SerializeField] readonly Ease _ease = Ease.OutSine;

        Transform _trans;
        
        public override string DisplayName => "Move";

        protected override Tween GetTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;
            
            if (_type == MoveType.RandomInCircle)
            {
                var random = Random.insideUnitCircle;
                random = random.normalized * _radius;

                Vector3 des = _is3D ? new Vector3(random.x, 0, random.y) : random;

                return _trans.DOMove(textBase.transform.position + des, duration).SetEase(_ease).ChangeStartValue(Vector3.zero);
            }
            else
            {
                return _trans.DOLocalMove(_localConfig.GetEnd(), duration).SetEase(_ease).ChangeStartValue(_localConfig.GetStart());
            }
        }
        
        protected override PrimeTween.Tween GetPrimeTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;

            if (_type == MoveType.RandomInCircle)
            {
                var random = Random.insideUnitCircle;
                random = random.normalized * _radius;
                Vector3 des = _is3D ? new Vector3(random.x, 0, random.y) : random;

                return PrimeTween.Tween.Position(_trans, Vector3.zero, _trans.position + des, duration, EaseMapper.MapEase(_ease));
            }
            else
            {
                Vector3 start = _localConfig.GetStart();
                Vector3 end = _localConfig.GetEnd();

                return PrimeTween.Tween.LocalPosition(_trans, start, end, duration, EaseMapper.MapEase(_ease));
            }
        }
    }
}
