using SketchEngine.Data;
using UnityEngine;

namespace SketchEngine.Utilities.UI
{
    public class UIHidden : MonoBehaviour
    {
        [SerializeField] CanvasGroup _canvasGroup;

        CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = CanvasGroup;

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
