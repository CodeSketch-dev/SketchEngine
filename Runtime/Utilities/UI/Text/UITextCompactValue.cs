using System;
using DG.Tweening;
using SketchEngine.Data;
using TMPro;
using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>Bind once to a currency. Formats into reusable buffers and starts new tweens from the visible value.</summary>
    [DisallowMultipleComponent]
    public sealed class UITextCompactValue : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField, Min(0)] float _duration = 0.7f;
        [SerializeField, Range(0, 15)] int _decimalDigits = 2;
        [SerializeField] Ease _ease = Ease.OutCubic;

        readonly char[] _buffer = new char[64];
        readonly char[] _previous = new char[64];
        int _previousLength = -1;
        DataValue<CompactValue> _source;
        bool _subscribed;
        bool _autoRefresh = true;
        CompactValue _displayed, _start, _target;
        Tween _tween;
        TweenCallback<float> _onProgress;
        TweenCallback _onKill;

        public CompactValue DisplayedValue => _displayed;

        /// <summary>Assign the actual DataValue instance; rebind when switching wallets/minigames.</summary>
        public void Bind(DataValue<CompactValue> source)
        {
            Unsubscribe();
            _tween?.Kill();
            _tween = null;
            _source = source;
            _autoRefresh = true;
            if (_source == null) return;
            if (isActiveAndEnabled) Subscribe();
            ForceUpdateValue(_source.Value);
        }

        void OnEnable()
        {
            _autoRefresh = true;
            Subscribe();
            if (_source != null) ForceUpdateValue(_source.Value);
        }

        void OnDisable()
        {
            Unsubscribe();
            _tween?.Kill();
            _tween = null;
        }

        void Subscribe()
        {
            if (_source == null || _subscribed) return;
            _source.OnValueChanged += OnValueChanged;
            _subscribed = true;
        }

        void Unsubscribe()
        {
            if (!_subscribed) return;
            _source.OnValueChanged -= OnValueChanged;
            _subscribed = false;
        }

        void OnValueChanged(CompactValue value)
        {
            if (_autoRefresh) AnimateTo(value);
        }

        /// <summary>Call before granting a spread reward to hold the text until coins arrive.</summary>
        public void SetAutoRefresh(bool enabled)
        {
            _autoRefresh = enabled;
            if (enabled) UpdateValue();
            else
            {
                _tween?.Kill();
                _tween = null;
            }
        }

        /// <summary>Animate toward the current balance, for example when spread coins reach the bar.</summary>
        public void UpdateValue()
        {
            if (_source != null) AnimateTo(_source.Value);
        }

        public void AnimateTo(CompactValue target)
        {
            _tween?.Kill();
            _tween = null;
            if (!isActiveAndEnabled || _duration <= 0 || target == _displayed)
            {
                ForceUpdateValue(target);
                return;
            }
            _start = _displayed;
            _target = target;
            // One cached callback; no new closure or string for each animation frame.
            if (_onProgress == null) _onProgress = OnProgress;
            if (_onKill == null) _onKill = OnTweenKilled;
            _tween = DOVirtual.Float(0, 1, _duration, _onProgress)
                .SetEase(_ease).SetUpdate(true).OnKill(_onKill);
        }

        void OnTweenKilled() => _tween = null;

        void OnProgress(float progress)
        {
            _displayed = CompactValue.Lerp(_start, _target, progress);
            UpdateText();
        }

        public void ForceUpdateValue(CompactValue value)
        {
            _tween?.Kill();
            _tween = null;
            _displayed = value;
            UpdateText();
        }

        void UpdateText()
        {
            if (_text == null || !_displayed.TryFormat(_buffer.AsSpan(), out int length, _decimalDigits)) return;
            bool changed = length != _previousLength;
            for (int i = 0; !changed && i < length; i++) changed = _buffer[i] != _previous[i];
            if (!changed) return;
            _text.SetCharArray(_buffer, 0, length);
            Array.Copy(_buffer, _previous, length);
            _previousLength = length;
        }
    }
}
