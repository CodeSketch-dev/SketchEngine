using SketchEngine.Mono;

namespace SketchEngine.Utilities.CanvasWorld
{
    /// <summary>
    /// Đặt component này vào 1 GameObject trong scene bootstrap CHỈ KHI game của bạn thực sự cần
    /// World Canvas dùng chung (HP bar, damage text, name tag nổi trên đầu object...).
    ///
    /// Nếu game không dùng world canvas thì đừng thêm script này - <see cref="WorldCanvasManager"/>
    /// sẽ không tự tạo "CanvasWorld" nữa, tránh rác GameObject DontDestroyOnLoad không ai dùng tới.
    /// Canvas vẫn có thể được tạo "ngầm" nếu code của bạn gọi thẳng <see cref="WorldCanvasUtility.Push"/>,
    /// script này chỉ dùng để tạo canvas NGAY từ đầu (prewarm) thay vì đợi lần Push đầu tiên.
    /// </summary>
    public sealed class WorldCanvasEnabler : MonoSingleton<WorldCanvasEnabler>
    {
        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();

            if (WorldCanvasManager.Root == null)
                WorldCanvasManager.ForceRefresh();
        }
    }
}
