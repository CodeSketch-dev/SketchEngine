using SketchEngine.Diagnostics;
using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsAnimator
    {
        /// <summary>Lấy độ dài của clip theo tên. Trả về 0 nếu không có controller hoặc không tìm thấy clip.</summary>
        public static float GetLength(this Animator animator, string clipName)
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                SketchDebug.Log(typeof(ExtensionsAnimator), $"GetLength failed: {animator.name} has no AnimatorController", Color.cyan);
                return 0f;
            }

            AnimationClip[] clips = controller.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && clips[i].name == clipName)
                    return clips[i].length;
            }

            SketchDebug.Log(typeof(ExtensionsAnimator), $"GetLength failed: clip {clipName} doesn't exist!", Color.cyan);
            return 0f;
        }

        /// <summary>
        /// Phát state có tên TRÙNG tên clip. Animator.Play nhận tên STATE trong Animator Controller,
        /// nên chỉ đúng khi state và clip cùng tên. Nếu khác tên, hãy dùng animator.Play(stateName) trực tiếp.
        /// </summary>
        public static void Play(this Animator animator, AnimationClip clip, int layer = 0)
        {
            if (clip == null) return;
            animator.Play(clip.name, layer);
        }
    }
}
