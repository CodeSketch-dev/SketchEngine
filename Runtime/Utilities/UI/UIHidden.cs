using SketchEngine.Data;
using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Utilities.UI
{
    public class UIHidden : MonoBase
    {
        CanvasGroup _canvasGroup;

         void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = GameObjectCached.AddComponent<CanvasGroup>();

            DataMaster.UIHidden.OnValueChanged += UIHiddenValue_EventValueChanged;

            _canvasGroup.alpha = DataMaster.UIHidden.Value ? 0.0f : 1.0f;
        }

         void OnDestroy()
        {
            DataMaster.UIHidden.OnValueChanged -= UIHiddenValue_EventValueChanged;
        }

         void UIHiddenValue_EventValueChanged(bool isHidden)
        {
            _canvasGroup.alpha = DataMaster.UIHidden.Value ? 0.0f : 1.0f;
        }
    }
}
