using System;
using SketchEngine.Core.Text;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class UITextTimeCountdown : UITextBase
    {
        [Header("Config")]
        [SerializeField] bool _unscaledTime = true;

        float _timeRemain;
        
        public event Action EventTimeUp;
        public float TimeRemain => _timeRemain;
        
        #region MonoBehaviour

        protected override void Tick()
        {
            base.Tick();
            
            _timeRemain -= _unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_timeRemain <= 0.0f)
            {
                _timeRemain = 0.0f;

                SetEnabled(false);
                UpdateTimeDisplay(_timeRemain);
                EventTimeUp?.Invoke();
            }

            UpdateTimeDisplay(_timeRemain);
        }

        #endregion
        
        #region Public

        public void Init(float timeLeft)
        {
            _timeRemain = timeLeft;

            UpdateTimeDisplay(_timeRemain);

            enabled = timeLeft > 0;
        }

        public void SetEnabled(bool isEnabled)
        {
            enabled = isEnabled;
        }

        #endregion

        protected virtual void UpdateTimeDisplay(float timeToDisplay)
        {

        }
    }
}
