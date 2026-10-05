using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsGameObject
    {
        #region Remove Components

        public static void RemoveComponent<T>(this GameObject gameObject) where T : Component
        {
            var comp = gameObject.GetComponent<T>();
            if (comp != null)
                Object.Destroy(comp);
        }

        #endregion

        #region Destroy Children

        public static void DestroyChildren(this GameObject gameObject)
        {
            var t = gameObject.transform;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i).gameObject;
                
                Object.Destroy(child);
            }
        }

        public static void DestroyChildrenImmediate(this GameObject gameObject)
        {
            var t = gameObject.transform;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i).gameObject;
                Object.DestroyImmediate(child);
            }
        }

        #endregion

        #region Utilities

        public static void SetActiveChildren(this GameObject gameObject, bool isActive)
        {
            var t = gameObject.transform;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                t.GetChild(i).gameObject.SetActive(isActive);
            }
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layerNumber)
        {
            var transforms = SharedComponents<Transform>.List;
            gameObject.GetComponentsInChildren(true, transforms);
            for (int i = 0; i < transforms.Count; i++)
                transforms[i].gameObject.layer = layerNumber;
            transforms.Clear();
        }

        public static Bounds GetRendererBounds(this GameObject go)
        {
            var renderers = SharedComponents<Renderer>.List;
            go.GetComponentsInChildren(true, renderers);

            if (renderers.Count == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++)
                bounds.Encapsulate(renderers[i].bounds);

            renderers.Clear();
            return bounds;
        }

        // Destroy an toàn khi đang Play; DestroyImmediate chỉ dùng trong Editor.
        public static void TryRemove(this GameObject go)
        {
            if (go == null) return;

            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        // List tái sử dụng theo kiểu, tránh cấp phát mảng mỗi lần gọi GetComponents.
        static class SharedComponents<T> where T : Component
        {
            internal static readonly List<T> List = new List<T>(16);
        }

        #endregion

        #region Instantiate Helpers

        public static T Create<T>(this T obj) where T : Object => Object.Instantiate(obj);

        public static T Create<T>(this T obj, Transform parent) where T : Object =>
            Object.Instantiate(obj, parent);

        public static T Create<T>(this T obj, Transform parent, bool worldPositionStays) where T : Object =>
            Object.Instantiate(obj, parent, worldPositionStays);

        public static T Create<T>(this T obj, Vector3 pos, Quaternion rot) where T : Object =>
            Object.Instantiate(obj, pos, rot);

        public static T Create<T>(this T obj, Vector3 pos, Quaternion rot, Transform parent) where T : Object =>
            Object.Instantiate(obj, pos, rot, parent);

        public static GameObject CreateRelativeLocal(this GameObject prefab, Transform parent)
        {
            var obj = Object.Instantiate(prefab, parent);
            obj.transform.localPosition = prefab.transform.localPosition;
            obj.transform.localRotation = prefab.transform.localRotation;
            obj.transform.localScale = prefab.transform.localScale;
            return obj;
        }

        public static GameObject CreateRelative(this GameObject prefab, Vector3 offset)
        {
            var obj = Object.Instantiate(prefab);
            obj.transform.position = prefab.transform.position + offset;
            obj.transform.rotation = prefab.transform.rotation;
            return obj;
        }

        #endregion
    }
}
