using SketchEngine.Diagnostics;
using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsAnimator
    {
        public static float GetLength(this Animator animator, string clipName)
        {
            var controller = animator.runtimeAnimatorController;
            var clips = controller.animationClips;
            int count = clips.Length;

            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip.name.Equals(clipName)) return clip.length;
            }
            
            CodeSketchDebug.Log(typeof(ExtensionsAnimator), $"Get clip length failed: Clip {clipName} doesn't exist!", Color.cyan);
            return 0f;
        }

        public static void Play(this Animator animator, AnimationClip clip, int layer = 0)
        {
            animator.Play(clip.name, layer);
        }
    }
}
