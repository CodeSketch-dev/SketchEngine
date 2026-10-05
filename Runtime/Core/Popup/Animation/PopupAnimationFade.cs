using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIPopup
{
    public class PopupAnimationFade : PopupAnimation
    {
        public override string displayName => "Fade";

        protected override Tween GetTween(Popup popup, float duration)
        {
            CanvasGroup canvasGroup = popup.CanvasGroup;

            return canvasGroup.DOFade(1f, duration)
                              .ChangeStartValue(0.0f);
        }
    }
}
