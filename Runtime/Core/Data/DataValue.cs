using System;
using System.Collections.Generic;
using UnityEngine;

namespace SketchEngine.Data
{
    public static class DataDirtyNotifier
    {
        public static event Action Dirty;

        public static void Raise() => Dirty?.Invoke();
    }

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
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;

                _value = value;
                OnValueChanged?.Invoke(_value);
                DataDirtyNotifier.Raise();
            }
        }

        // UI listeners are runtime state, never part of a save (including BinaryFormatter).
        [field: NonSerialized]
        public event Action<T> OnValueChanged;

        public DataValue(T _value)
        {
            this._value = _value;
        }
    }
}
