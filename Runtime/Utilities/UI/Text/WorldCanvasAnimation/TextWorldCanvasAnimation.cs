using System;
using SketchEngine.Core.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    [Serializable]
    public class TextWorldCanvasAnimation
    {
        public virtual string DisplayName { get; } = "";
        
        [MinMaxSlider(0f, 1f, true)]
        [SerializeField] Vector2 _durationRage = new Vector2(0f, 1f);

        public void Apply(UITextWorldCanvas textBase, Sequence sequence)
        {
            float animDuration = textBase.LiveTime * (_durationRage.y - _durationRage.x);
            float animDelay = textBase.LiveTime * _durationRage.x;

            Tween tween = GetTween(textBase, Mathf.Max(animDuration, 0.01f));

            if (animDelay > 0.0f)
            {
                sequence.Insert(animDelay, tween);
            }
            else
            {
                sequence.Join(tween);
            }
        }
        
        public void ApplyPrime(UITextWorldCanvas textBase, PrimeTween.Sequence sequence)
        {
            float animDuration = textBase.LiveTime * (_durationRage.y - _durationRage.x);
            float animDelay = textBase.LiveTime * _durationRage.x;

            PrimeTween.Tween tween = GetPrimeTween(textBase, Mathf.Max(animDuration, 0.01f));

            if (animDelay > 0.0f)
            {
                sequence.ChainDelay(animDelay).Chain(tween);
            }
            else
            {
                sequence.Group(tween);
            }
        }

        protected virtual Tween GetTween(UITextWorldCanvas textBase, float duration)
        {
            return null;
        }

        protected virtual  PrimeTween.Tween GetPrimeTween(UITextWorldCanvas textbase, float duration)
        {
            return default;
        }
    }
}

public static class EaseMapper
{
    public static PrimeTween.Ease MapEase(DG.Tweening.Ease ease)
    {
        return ease switch
        {
            Ease.Linear => PrimeTween.Ease.Linear,
            Ease.InSine => PrimeTween.Ease.InSine,
            Ease.OutSine => PrimeTween.Ease.OutSine,
            Ease.InOutSine => PrimeTween.Ease.InOutSine,
            Ease.InQuad => PrimeTween.Ease.InQuad,
            Ease.OutQuad => PrimeTween.Ease.OutQuad,
            Ease.InOutQuad => PrimeTween.Ease.InOutQuad,
            Ease.InCubic => PrimeTween.Ease.InCubic,
            Ease.OutCubic => PrimeTween.Ease.OutCubic,
            Ease.InOutCubic => PrimeTween.Ease.InOutCubic,
            Ease.InQuart => PrimeTween.Ease.InQuart,
            Ease.OutQuart => PrimeTween.Ease.OutQuart,
            Ease.InOutQuart => PrimeTween.Ease.InOutQuart,
            Ease.InQuint => PrimeTween.Ease.InQuint,
            Ease.OutQuint => PrimeTween.Ease.OutQuint,
            Ease.InOutQuint => PrimeTween.Ease.InOutQuint,
            Ease.InExpo => PrimeTween.Ease.InExpo,
            Ease.OutExpo => PrimeTween.Ease.OutExpo,
            Ease.InOutExpo => PrimeTween.Ease.InOutExpo,
            Ease.InCirc => PrimeTween.Ease.InCirc,
            Ease.OutCirc => PrimeTween.Ease.OutCirc,
            Ease.InOutCirc => PrimeTween.Ease.InOutCirc,
            Ease.InBack => PrimeTween.Ease.InBack,
            Ease.OutBack => PrimeTween.Ease.OutBack,
            Ease.InOutBack => PrimeTween.Ease.InOutBack,
            Ease.InBounce => PrimeTween.Ease.InBounce,
            Ease.OutBounce => PrimeTween.Ease.OutBounce,
            Ease.InOutBounce => PrimeTween.Ease.InOutBounce,
            Ease.InElastic => PrimeTween.Ease.InElastic,
            Ease.OutElastic => PrimeTween.Ease.OutElastic,
            Ease.InOutElastic => PrimeTween.Ease.InOutElastic,
            
            // Fallbacks
            _ => PrimeTween.Ease.Linear
        };
    }
}
