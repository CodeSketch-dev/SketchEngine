using PrimeTween;
using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Extension cho SpriteRenderer: alpha, fade và size. Dùng PrimeTween.
    /// </summary>
    public static class ExtensionsSpriteRenderer
    {
        /// <summary>Gán alpha tức thì, không tween.</summary>
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        /// <summary>Tween alpha tới giá trị đích trong duration giây.</summary>
        public static Tween Fade(this SpriteRenderer spriteRenderer, float targetAlpha, float duration, Ease ease = Ease.Linear)
        {
            return Tween.Custom(spriteRenderer.color.a, targetAlpha, duration,
                value => spriteRenderer.SetAlpha(value), ease);
        }

        /// <summary>Gán size tức thì (chỉ có hiệu lực khi drawMode là Sliced hoặc Tiled).</summary>
        public static void SetSize(this SpriteRenderer spriteRenderer, Vector2 targetSize)
        {
            spriteRenderer.size = targetSize;
        }

        /// <summary>Tween size tới giá trị đích trong duration giây.</summary>
        public static Tween ResizeTo(this SpriteRenderer spriteRenderer, Vector2 targetSize, float duration, Ease ease = Ease.Linear)
        {
            return Tween.Custom(spriteRenderer.size, targetSize, new TweenSettings(duration, ease),
                value => spriteRenderer.size = value);
        }
    }
}
