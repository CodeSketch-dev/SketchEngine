using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using ArgumentNullException = System.ArgumentNullException;

namespace SketchEngine.Modules.Lifetime
{
    /// <summary>
    /// LifetimeBindingAddressable
    /// ---------------------------------------------------------
    /// Extension methods giúp "gắn vòng đời" (lifetime) của Addressables
    /// (AsyncOperationHandle / AssetReference / SceneInstance)
    /// vào GameObject hoặc LifetimeBinding.
    ///
    /// Mục tiêu:
    /// - Tránh quên Release / Unload Addressables
    /// - Tự động cleanup khi GameObject bị Destroy
    /// - An toàn với scene load / unload
    ///
    /// Yêu cầu:
    /// - GameObject phải có hoặc được tự động add LifetimeBinding
    /// - LifetimeBinding phải phát EventRelease khi OnDestroy
    ///
    /// Pattern sử dụng:
    /// Addressables.LoadAssetAsync<T>(key)
    ///     .BindTo(gameObject);
    /// </summary>
    public static class LifetimeBindingAddressable
    {
        // =========================================================
        // BIND HANDLE → GAMEOBJECT
        // =========================================================

        /// <summary>
        /// Gắn vòng đời của AsyncOperationHandle (asset hoặc scene)
        /// vào GameObject.
        ///
        /// Khi GameObject bị Destroy:
        /// - Asset  → Addressables.Release
        /// - Scene  → Addressables.UnloadSceneAsync
        ///
        /// ⚠️ Caller phải chỉ rõ isScene
        /// (dùng khi handle KHÔNG generic)
        ///
        /// Ví dụ:
        /// var handle = Addressables.LoadSceneAsync(key);
        /// handle.BindTo(gameObject, isScene: true);
        /// </summary>
        public static AsyncOperationHandle BindTo(
            this AsyncOperationHandle self,
            GameObject gameObject,
            bool isScene
        )
        {
            // GameObject null → không thể bind → release ngay
            if (gameObject == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(
                    nameof(gameObject),
                    "GameObject null, handle sẽ được giải phóng ngay lập tức."
                );
            }

            // Đảm bảo GameObject có LifetimeBinding
            if (!gameObject.TryGetComponent(out LifetimeBinding lifetimeBinding))
                lifetimeBinding = gameObject.AddComponent<LifetimeBinding>();

            // Delegate sang bind theo LifetimeBinding
            return self.BindTo(lifetimeBinding, isScene);
        }

        /// <summary>
        /// Gắn vòng đời của AsyncOperationHandle&lt;T&gt; vào GameObject.
        ///
        /// Tự động detect SceneInstance:
        /// - T == SceneInstance → UnloadSceneAsync
        /// - Ngược lại          → Release
        ///
        /// Ví dụ:
        /// Addressables.LoadAssetAsync<GameObject>(key)
        ///     .BindTo(gameObject);
        ///
        /// Addressables.LoadSceneAsync(key)
        ///     .BindTo(gameObject);
        /// </summary>
        public static AsyncOperationHandle<T> BindTo<T>(
            this AsyncOperationHandle<T> self,
            GameObject gameObject
        )
        {
            bool isScene = typeof(T) == typeof(SceneInstance);

            if (gameObject == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(
                    nameof(gameObject),
                    "GameObject null, handle sẽ được giải phóng ngay lập tức."
                );
            }

            ((AsyncOperationHandle)self).BindTo(gameObject, isScene);
            return self;
        }

        // =========================================================
        // BIND HANDLE → LIFETIMEBINDING
        // =========================================================

        /// <summary>
        /// Gắn AsyncOperationHandle trực tiếp vào LifetimeBinding.
        ///
        /// Khi LifetimeBinding phát EventRelease:
        /// - Handle sẽ được Release / Unload
        /// - Event tự unsubscribe (an toàn GC)
        ///
        /// Dùng khi:
        /// - Lifetime không gắn trực tiếp vào GameObject
        /// - Hoặc quản lý lifetime theo logic riêng
        /// </summary>
        public static AsyncOperationHandle BindTo(
            this AsyncOperationHandle self,
            LifetimeBinding lifetimeBinding,
            bool isScene
        )
        {
            if (lifetimeBinding == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(
                    nameof(lifetimeBinding),
                    "LifetimeBinding null, handle sẽ được giải phóng ngay lập tức."
                );
            }

            // Local handler để đảm bảo unsubscribe đúng instance
            void OnRelease()
            {
                ReleaseHandle(self, isScene);
                lifetimeBinding.EventRelease -= OnRelease;
            }

            lifetimeBinding.EventRelease += OnRelease;
            return self;
        }

        /// <summary>
        /// Gắn AsyncOperationHandle&lt;T&gt; vào LifetimeBinding.
        ///
        /// Tự detect SceneInstance tương tự BindTo(GameObject)
        /// </summary>
        public static AsyncOperationHandle<T> BindTo<T>(
            this AsyncOperationHandle<T> self,
            LifetimeBinding lifetimeBinding
        )
        {
            bool isScene = typeof(T) == typeof(SceneInstance);

            if (lifetimeBinding == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(
                    nameof(lifetimeBinding),
                    "LifetimeBinding null, handle sẽ được giải phóng ngay lập tức."
                );
            }

            ((AsyncOperationHandle)self).BindTo(lifetimeBinding, isScene);
            return self;
        }

        // =========================================================
        // BIND ASSETREFERENCE → GAMEOBJECT
        // =========================================================

        /// <summary>
        /// Gắn AssetReference (thường là prefab Addressables)
        /// vào GameObject.
        ///
        /// Khi GameObject bị Destroy:
        /// - Instance được ReleaseInstance
        ///
        /// ⚠️ Lưu ý:
        /// - Phù hợp với Addressables.InstantiateAsync
        /// - Không dùng cho LoadAssetAsync
        ///
        /// Ví dụ:
        /// assetReference.InstantiateAsync()
        ///     .Completed += _ => assetReference.BindTo(gameObject);
        /// </summary>
        public static AssetReference BindTo(
            this AssetReference self,
            GameObject gameObject
        )
        {
            if (gameObject == null)
            {
                self.ReleaseAsset();
                throw new ArgumentNullException(
                    nameof(gameObject),
                    "GameObject null, asset sẽ được release ngay lập tức."
                );
            }

            if (!gameObject.TryGetComponent(out LifetimeBinding lifetimeBinding))
                lifetimeBinding = gameObject.AddComponent<LifetimeBinding>();

            void OnRelease()
            {
                self.ReleaseInstance(lifetimeBinding.gameObject);
                lifetimeBinding.EventRelease -= OnRelease;
            }

            lifetimeBinding.EventRelease += OnRelease;
            return self;
        }

        // =========================================================
        // INTERNAL RELEASE
        // =========================================================

        /// <summary>
        /// Giải phóng AsyncOperationHandle đúng cách
        /// theo loại asset:
        /// - Scene  → UnloadSceneAsync
        /// - Asset  → Release
        ///
        /// ⚠️ Không gọi trực tiếp bên ngoài
        /// </summary>
        static void ReleaseHandle(
            AsyncOperationHandle handle,
            bool isScene
        )
        {
            if (isScene)
                Addressables.UnloadSceneAsync(handle);
            else
                Addressables.Release(handle);
        }
    }
}
