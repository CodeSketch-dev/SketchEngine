using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SketchEngine.Patterns.Pool
{
    // Đối tượng hỗ trợ dọn dẹp trước khi trả về pool.
    interface IPoolRelease
    {
        void TaskBeforeRelease();  // Dọn dẹp đồng bộ nhanh.
    }

    // Tuỳ chọn async cleanup nếu cần (IO, chờ 1 frame, tween await, …)
    public interface IPoolReleaseAync
    {
        UniTask TaskBeforeRelease();
    }
}
