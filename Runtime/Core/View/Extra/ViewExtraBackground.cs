using SketchEngine.Core.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.UIView
{
    [System.Serializable]
    public class ViewExtraBackground : ViewExtra
    {
        [SerializeField] bool _closeOnClick = true;
        [SerializeField] Sprite _sprite;
        [SerializeField] Color _color = new Color(0f, 0f, 0f, 0.8f);

        public override string DisplayName => $"View/Background";

        GameObject _objBG;

        protected override Tween GetTween(View view, float duration)
        {
            // GetTween chỉ chạy 1 LẦN trong đời của View (ConstructSequence có guard chạy 1 lần),
            // nên không thể destroy _objBG mỗi lần Close rồi mong nó được spawn lại ở Open lần sau -
            // trước đây làm vậy khiến background biến mất vĩnh viễn nếu View được mở lại (destroyOnClose = false).
            // Giờ chỉ ẩn/hiện theo Open/Close; chỉ thật sự Destroy khi View cũng sắp bị Destroy.
            view.onOpenStart.AddListener(() =>
            {
                if (_objBG) _objBG.SetActive(true);
            });

            view.onCloseEnd.AddListener(() =>
            {
                if (!_objBG) return;

                if (view.DestroyOnClose) Object.Destroy(_objBG);
                else _objBG.SetActive(false);
            });

            // Spawn background
            SpawnBackground(view);

            // Return background fade tween
            Image image = _objBG.GetComponent<Image>();
            image.color = _color;
            image.sprite = _sprite;

            return image.DOFade(_color.a, duration)
                        .ChangeStartValue(new Color(_color.r, _color.g, _color.b, 0.0f))
                        .SetEase(Ease.Linear);
        }

        void SpawnBackground(View view)
        {
            _objBG = new GameObject(DisplayName);

            RectTransform rect = _objBG.AddComponent<RectTransform>();
            rect.SetParent(view.TransformCached.parent);
            rect.SetSiblingIndex(view.transform.GetSiblingIndex());
            rect.SetScale(1.0f);

            rect.StretchByParent();

            _objBG.AddComponent<Image>();

            if (_closeOnClick)
            {
                Button button = _objBG.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => { view.Close(); button.interactable = false; });
            }
        }
    }
}
