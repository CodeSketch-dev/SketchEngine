using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionEntity : MonoCached
    {
        [Title("Config")]
        [Range(0.1f, 1f)]
        [SerializeField] float _duration = 1f;

        [CustomValueDrawer("GUIDrawInsertTime")]
        [SerializeField] float _insertTime = 0f;

        [ListDrawerSettings(AddCopiesLastElement = true, ListElementLabelName = "DisplayName")]
        [SerializeReference] ViewTransition[] _transitions = Array.Empty<ViewTransition>();

        public void Apply(View view)
        {
            for (int i = 0; i < _transitions.Length; i++)
            {
                Tween tween = _transitions[i].GetTween(this, _duration);

                if (_insertTime > 0f)
                    view.sequence.Insert(_insertTime, tween);
                else
                    view.sequence.Join(tween);
            }
        }

#if UNITY_EDITOR
        float GUIDrawInsertTime(float insertTime, GUIContent label)
        {
            return UnityEditor.EditorGUILayout.Slider(label, insertTime, 0f, 1.0f - _duration);
        }

#endif
    }
}
