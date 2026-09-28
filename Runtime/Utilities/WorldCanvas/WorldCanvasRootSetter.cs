using UnityEngine;

namespace SketchEngine.Utilities.CanvasWorld
{
    public class WorldCanvasRootSetter : MonoBehaviour
    {
        void OnEnable()
        {
            WorldCanvasManager.Root = transform;
        }
    }
}
