using System;
using UnityEngine;

namespace SketchEngine.Data
{
    [System.Serializable]
    public partial class DataValue<T>
    {
        [SerializeField]
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<T> OnValueChanged;

        public DataValue(T _value)
        {
            this._value = _value;
        }
    }
}
