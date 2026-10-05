using System;
using SketchEngine.Core.Text;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class UITextTimeCountdown : UITextBase
    {
        [SerializeField] bool _useUnscaledTime = true;

        float _timeRemain;

        public event Action EventTimeUp;
        public float TimeRemain => _timeRemain;

        #region MonoBehaviour

        void Update()
        {
            Tick();
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

        public override void Tick()
        {
            base.Tick();

            _timeRemain -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

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

        protected virtual void UpdateTimeDisplay(float timeToDisplay)
        {

        }
    }
}
