using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

using SketchEngine.Core.Extensions;

namespace SketchEngine.UIPopup
{
    public class PopupAnimationBackground : PopupAnimation
    {
        [SerializeField] bool _closeOnClick = true;
        [SerializeField] Color _color = new Color(0f, 0f, 0f, 0.94f);

        public override string displayName => "Background";

        protected override Tween GetTween(Popup popup, float duration)
        {
            GameObject background = SpawnBackground(popup);

            // Ẩn/hiện theo vòng đời popup thay vì destroy khi close: popup có thể được bật lại mà không bị destroy,
            // destroy sớm sẽ khiến lần mở sau thao tác trên object đã hủy.
            popup.OnOpenStart.AddListener(() =>
            {
                if (background) background.SetActive(true);
            });

            popup.OnCloseEnd.AddListener(() =>
            {
                if (background) background.SetActive(false);
            });

            popup.Destroyed += () =>
            {
                if (background) Object.Destroy(background);
            };

            Image image = background.GetComponent<Image>();
            image.color = _color;

            return image.DOFade(_color.a, duration)
                        .ChangeStartValue(new Color(_color.r, _color.g, _color.b, 0f))
                        .SetEase(Ease.Linear);
        }

        GameObject SpawnBackground(Popup popup)
        {
            var background = new GameObject($"{popup.name} Background");

            RectTransform rect = background.AddComponent<RectTransform>();
            rect.SetParent(popup.TransformCached.parent);
            rect.SetSiblingIndex(popup.TransformCached.GetSiblingIndex());
            rect.SetScale(1.0f);
            rect.StretchByParent();

            background.AddComponent<Image>();

            if (_closeOnClick)
            {
                Button button = background.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => { if (popup) popup.Close(); });
            }

            return background;
        }
    }
}
