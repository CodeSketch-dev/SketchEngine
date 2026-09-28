using UnityEngine;

namespace SketchEngine.Utilities.CanvasWorld
{
    public static class WorldCanvasUtility
    {
        public static void Push(Transform transform)
        {
            CheckCanvas();

            if (WorldCanvasManager.Root != null && transform != null)
            {
                transform.SetParent(WorldCanvasManager.Root, false);
                transform.SetAsLastSibling();
            }
        }

        static void CheckCanvas()
        {
            if (WorldCanvasManager.Root == null)
            {
                WorldCanvasManager.ForceRefresh();
            }
        }
    }
}