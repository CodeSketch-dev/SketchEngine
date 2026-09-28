using UnityEngine;

namespace SketchEngine.UIPopup
{
    public class PopupRootSetter : MonoBehaviour
    {
        void Start()
        {
            PopupManager.SetRoot(transform);
            PopupManager.EventRootUndefine += OnRootUndefine;
        }

        void OnDestroy()
        {
            PopupManager.EventRootUndefine -= OnRootUndefine;
        }

        void OnRootUndefine()
        {
            PopupManager.SetRoot(transform);
        }
    }
}
