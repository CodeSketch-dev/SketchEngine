using System;
using SketchEngine.Mono;

namespace SketchEngine.Data
{
    [Serializable]
    public class DataBlock<T> where T : DataBlock<T>
    {
        static T s_instance;

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = DataFileHandler.LoadFromDevice<T>(typeof(T).ToString());

                    if (s_instance == null)
                        s_instance = (T)Activator.CreateInstance(typeof(T));

                    s_instance.Init();
                }

                return s_instance;
            }
        }

        protected virtual void Init()
        {
            // Trên Android, OnApplicationPause(true) LUÔN fire khi app rời foreground
            // (Home / vuốt tắt / khoá màn hình / hệ thống kill) -> đây là điểm lưu đáng tin duy nhất.
            // OnApplicationQuit chỉ fire ở một số máy (back thoát hẳn) -> giữ thêm cho chắc.
            // KHÔNG subscribe OnApplicationFocus: nó fire cả khi KHÔNG thực sự thoát
            // (kéo notification, dialog xin quyền, ad overlay...) -> gây ghi đĩa thừa lúc pause (ANR).
            MonoSingleton<MonoCallback>.SafeInstance.EventApplicationPause += MonoCallback_ApplicationOnPause;
            MonoSingleton<MonoCallback>.SafeInstance.EventApplicationQuit += MonoCallback_ApplicationOnQuit;
        }

        void MonoCallback_ApplicationOnQuit()
        {
            Save();
        }

        void MonoCallback_ApplicationOnPause(bool paused)
        {
            if (paused)
                Save();
        }

        public static void Save()
        {
            DataFileHandler.SaveToDevice(Instance, typeof(T).ToString());
        }

        public static void Delete()
        {
            s_instance = null;
            DataFileHandler.DeleteInDevice(typeof(T).ToString());
        }
    }
}