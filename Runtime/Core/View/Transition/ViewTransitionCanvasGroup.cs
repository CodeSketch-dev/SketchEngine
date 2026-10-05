using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionCanvasGroup : ViewTransition
    {
        [SerializeField] Ease _ease = Ease.Linear;

        [SerializeField] bool _keepEnd = true;

        [HideIf("@_keepEnd"), Range(0f, 1f)]
        [SerializeField] float _alphaEnd;

        [SerializeField] bool _keepStart = false;

        [HideIf("@_keepStart"), Range(0f, 1f)]
        [SerializeField] float _alphaStart;

        public override string DisplayName => "Canvas Group";

        public override Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            CanvasGroup canvasGroup = entity.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                return null;

            float alphaEnd = _keepEnd ? canvasGroup.alpha : _alphaEnd;
            float alphaStart = _keepStart ? canvasGroup.alpha : _alphaStart;

            return canvasGroup.DOFade(alphaEnd, duration)
                              .ChangeStartValue(alphaStart)
                              .SetEase(_ease);
        }
    }
}
