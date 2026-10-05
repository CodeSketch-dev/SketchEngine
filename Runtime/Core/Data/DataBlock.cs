using System;
using PrimeTween;
using SketchEngine.Mono;

namespace SketchEngine.Data
{
    [Serializable]
    public class DataBlock<T> where T : DataBlock<T>
    {
        // Thời gian gom các thay đổi liên tiếp thành một lần ghi (để crash giữa chừng vẫn chỉ mất tối đa vài giây).
        const float AutoSaveDelay = 2f;

        static readonly Action s_autoSave = AutoSave;

        static T _INS;
        static Tween _autoSaveTimer;
        static bool s_warmed;

        [NonSerialized] Action<bool> _onApplicationPause;
        [NonSerialized] Action _onApplicationQuit;
        [NonSerialized] Action _onDirty;

        public static T INSTANCE
        {
            get
            {
                if (_INS == null)
                {
                    _INS = DataFileHandler.LoadFromDevice<T>(typeof(T).ToString());

                    if (_INS == null)
                        _INS = (T)Activator.CreateInstance(typeof(T));

                    _INS.Init();
                }

                return _INS;
            }
        }

        protected virtual void Init()
        {
            // Trên Android, OnApplicationPause(true) LUÔN fire khi app rời foreground
            // (Home / vuốt tắt / khoá màn hình / hệ thống kill) -> đây là điểm lưu đáng tin duy nhất.
            // OnApplicationQuit chỉ fire ở một số máy (back thoát hẳn) -> giữ thêm cho chắc.
            // KHÔNG subscribe OnApplicationFocus: nó fire cả khi KHÔNG thực sự thoát
            // (kéo notification, dialog xin quyền, ad overlay...) -> gây ghi đĩa thừa lúc pause (ANR).
            _onApplicationPause = OnApplicationPause;
            _onApplicationQuit = OnApplicationQuit;
            _onDirty = MarkDirty;

            MonoCallback callback = MonoSingleton<MonoCallback>.SafeInstance;
            callback.EventApplicationPause += _onApplicationPause;
            callback.EventApplicationQuit += _onApplicationQuit;

            DataDirtyNotifier.Dirty += _onDirty;
        }

        void Unhook()
        {
            DataDirtyNotifier.Dirty -= _onDirty;

            if (!MonoSingleton<MonoCallback>.HasInstance) return;

            MonoCallback callback = MonoSingleton<MonoCallback>.Instance;
            callback.EventApplicationPause -= _onApplicationPause;
            callback.EventApplicationQuit -= _onApplicationQuit;
        }

        void OnApplicationQuit()
        {
            SaveAndFlush();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveAndFlush();
        }

        // Lúc pause/quit app có thể bị kill ngay sau đó nên phải chờ ghi xong xuống đĩa.
        static void SaveAndFlush()
        {
            CancelAutoSave();
            Save();
            DataFileHandler.Flush();
        }

        // Có thay đổi: đặt lịch ghi sau AutoSaveDelay nếu chưa có lịch. Không tạo thêm lịch mới khi đã có sẵn nên không tốn gì mỗi frame.
        void MarkDirty()
        {
            if (_INS == null) return;
            if (_autoSaveTimer.isAlive) return;

            _autoSaveTimer = Tween.Delay(AutoSaveDelay, s_autoSave, useUnscaledTime: true);
        }

        static void AutoSave()
        {
            _autoSaveTimer = default;
            Save();
        }

        static void CancelAutoSave()
        {
            if (_autoSaveTimer.isAlive) _autoSaveTimer.Stop();
            _autoSaveTimer = default;
        }

        // Load + khởi tạo + build metadata serializer trước, để lúc chơi không phải trả chi phí đó. Chỉ chạy 1 lần.
        [UnityEngine.Scripting.Preserve]
        public static void Warmup()
        {
            if (s_warmed) return;
            s_warmed = true;

            DataSerializer.Serialize<T>(INSTANCE);
        }

        public static void Save()
        {
            // Chưa từng truy cập thì chưa có gì thay đổi để ghi; không tạo instance mới chỉ để lưu lúc thoát app.
            if (_INS == null) return;

            DataFileHandler.SaveToDevice(_INS, typeof(T).ToString());
        }

        public static void Delete()
        {
            CancelAutoSave();
            _INS?.Unhook();
            _INS = null;

            DataFileHandler.DeleteInDevice(typeof(T).ToString());
        }
    }
}
