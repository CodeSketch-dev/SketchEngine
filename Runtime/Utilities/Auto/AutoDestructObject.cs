using System;
using PrimeTween;
using UnityEngine;

namespace SketchEngine.Utilities.Auto
{
    /// <summary>
    /// This class is created for auto destroy or disable gameobject purpose
    /// </summary>
    public class AutoDestructObject : MonoBehaviour
    {
        [SerializeField] float _delay = 0f;
        [SerializeField] bool _deactiveOnly = false;

        Tween _tween;

        public event Action OnDestruct;

        #region MonoBehaviour

        protected virtual void OnEnable()
        {
            _tween.Stop();
            _tween = Tween.Delay(_delay, Destruct, false, false);
        }

        protected virtual void OnDisable()
        {
            _tween.Stop();
        }

        #endregion

        void Destruct()
        {
            if (_deactiveOnly)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);

            OnDestruct?.Invoke();
        }
    }
}